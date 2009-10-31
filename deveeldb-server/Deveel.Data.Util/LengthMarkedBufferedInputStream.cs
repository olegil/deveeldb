﻿using System;
using System.IO;
using System.Net.Sockets;

namespace Deveel.Data.Util {
	/// <summary>
	/// Reads a command block on the underlying stream that is constrained by 
	/// a length marker preceeding the command.
	/// </summary>
	/// <remarks>
	/// This can be used as a hack work around for non-blocking IO because we 
	/// know ahead of time how much data makes up the next block of information 
	/// over the stream.
	/// </remarks>
	sealed class LengthMarkedBufferedInputStream : Stream {
		/// <summary>
		/// The initial buffer size of the internal input buffer.
		/// </summary>
		private const int INITIAL_BUFFER_SIZE = 512;

		/// <summary>
		/// The chained InputStream that is underneath this object.
		/// </summary>
		private IInputStream input;

		/// <summary>
		/// The buffer that is used to read in whatever is on the stream.
		/// </summary>
		private byte[] buf;

		/// <summary>
		/// The number of valid bytes in the buffer.
		/// </summary>
		private int count;

		/// <summary>
		/// The area of the buffer that is marked as being an available command.
		/// If it's -1 then there is no area marked.
		/// </summary>
		private int marked_length;

		/// <summary>
		/// The current index of the marked area that is being read.
		/// </summary>
		private int marked_index;

		public LengthMarkedBufferedInputStream(IInputStream input) {
			this.input = input;
			buf = new byte[INITIAL_BUFFER_SIZE];
			count = 0;
			marked_length = -1;
			marked_index = -1;
		}

		/// <summary>
		/// Ensures that the buffer is large enough to store the given value.
		/// </summary>
		/// <param name="new_size"></param>
		/// <remarks>
		/// If the buffer is not large enough this method grows it so it is big enough.
		/// </remarks>
		private void EnsureCapacity(int new_size) {
			int old_size = buf.Length;
			if (new_size > old_size) {
				int cap = (old_size * 3) / 2 + 1;
				if (cap < new_size)
					cap = new_size;
				byte[] old_buf = buf;
				buf = new byte[cap];
				//      // Copy all the contents except the first 4 bytes (the size marker)
				//      System.arraycopy(old_buf, 4, buf, 4, count - 4);
				Array.Copy(old_buf, 0, buf, 0, count);
			}
		}

		/// <summary>
		/// Called when the end of the marked length is reached.
		/// </summary>
		/// <remarks>
		/// It performs various maintenance operations to ensure the buffer consistency 
		/// is maintained.
		/// <para>
		/// Assumes we are calling from a synchronized method.
		/// </para>
		/// </remarks>
		private void HandleEndReached() {
			//    System.out.println();
			//    System.out.println("Shifting Buffer: ");
			//    System.out.println(" Index: " + marked_index +
			//                         ", Length: " + (count - marked_length));
			// Move anything from the end of the buffer to the start.
			Array.Copy(buf, marked_index, buf, 0, count - marked_length);
			count -= marked_length;

			// Reset the state
			marked_length = -1;
			marked_index = -1;
		}

		// ---------- Overwritten from Stream ----------

		public override int ReadByte() {
			lock (this) {
				if (marked_index == -1) {
					throw new IOException("No mark has been read yet.");
				}
				if (marked_index >= marked_length) {
					String debug_msg = "Read over end of length marked buffer.  ";
					debug_msg += "(marked_index=" + marked_index;
					debug_msg += ",marked_length=" + marked_length + ")";
					debug_msg += ")";
					throw new IOException(debug_msg);
				}
				int n = buf[marked_index++] & 0x0FF;
				if (marked_index >= marked_length) {
					HandleEndReached();
				}
				return n;
			}
		}

		public override void Write(byte[] buffer, int offset, int count) {
			throw new NotSupportedException();
		}

		public override bool CanRead {
			get { return true; }
		}

		public override bool CanSeek {
			get { return false; }
		}

		public override bool CanWrite {
			get { return false; }
		}

		public override long Length {
			get { throw new NotImplementedException(); }
		}

		public override long Position {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public override void Flush() {
		}

		public override long Seek(long offset, SeekOrigin origin) {
			throw new NotSupportedException();
		}

		public override void SetLength(long value) {
			throw new NotSupportedException();
		}

		public override int Read(byte[] b, int off, int len) {
			lock (this) {
				if (marked_index == -1) {
					throw new IOException("No mark has been read yet.");
				}
				int read_upto = marked_index + len;
				if (read_upto > marked_length) {
					String debug_msg = "Read over end of length marked buffer.  ";
					debug_msg += "(marked_index=" + marked_index;
					debug_msg += ",len=" + len;
					debug_msg += ",marked_length=" + marked_length + ")";
					throw new IOException(debug_msg);
				}
				Array.Copy(buf, marked_index, b, off, len);
				marked_index = read_upto;
				if (marked_index >= marked_length) {
					HandleEndReached();
				}
				return len;
			}
		}

		public int Available {
			get {
				lock (this) {
					// This method only returns a non 0 value if there is a complete command
					// waiting on the stream.
					if (marked_length >= 0) {
						return (marked_length - marked_index);
					}
					return 0;
				}
			}
		}

		// ---------- These methods aid in reading state from the stream ----------

		/// <summary>
		/// Checks to see if there is a complete command waiting on the input stream.
		/// </summary>
		/// <param name="max_size">The maximum number of bytes we are allowing before an 
		/// <see cref="IOException"/> is thrown.</param>
		/// <remarks>
		/// If this method returns true then it is safe to go ahead and process a single 
		/// command from this stream. This will return true only once while there is a 
		/// command pending until that command is completely read in.
		/// </remarks>
		/// <returns>
		/// Returns true if there is a complete command.
		/// </returns>
		public bool PollForCommand(int max_size) {
			lock (this) {
				if (marked_length == -1) {
					int available = input.Available;
					if (count > 0 || available > 0) {
						if ((count + available) > max_size) {
							throw new IOException("Marked length is greater than max size ( " +
												  (count + available) + " > " + max_size + " )");
						}

						EnsureCapacity(count + available);
						int read_in = input.Read(buf, count, available);

						if (read_in == 0) {
							//TODO: Check this format...
							// throw new EndOfStreamException();

							// zero bytes read means that the stream is finished...
							return false;
						}
						count = count + read_in;

						// Check: Is a complete command available?
						if (count >= 4) {
							int length_marker = ByteBuffer.ReadInt4(buf, 0);

							if (count >= length_marker + 4) {
								// Yes, complete command available.
								// mark this area up.
								marked_length = length_marker + 4;
								marked_index = 4;
								return true;
							}
						}
					}
				}
				return false;
			}
		}

		/// <summary>
		/// Blocks until a complete command has been read in.
		/// </summary>
		public void BlockForCommand() {
			lock (this) {
				while (true) {

					// Is there a command available?
					if (count >= 4) {
						int length_marker = ByteBuffer.ReadInt4(buf, 0);
						if (count >= length_marker + 4) {
							// Yes, complete command available.
							// mark this area up.
							marked_length = length_marker + 4;
							marked_index = 4;
							return;
						}
					}

					// If the buffer is full grow it larger.
					if (count >= buf.Length) {
						EnsureCapacity(count + INITIAL_BUFFER_SIZE);
					}
					// Read in a block of data, block if nothing there
					int read_in = input.Read(buf, count, buf.Length - count);
					if (read_in == 0) {
						//TODO: Check this format...
						// throw new EndOfStreamException();

						// zero bytes read means that the stream is finished...
						return;
					}
					count += read_in;
				}
			}
		}
	}
}