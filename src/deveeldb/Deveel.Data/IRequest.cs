﻿// 
//  Copyright 2010-2016 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


using System;

namespace Deveel.Data {
	/// <summary>
	/// Represents a generic request for execution of a 
	/// command in a system.
	/// </summary>
	/// <remarks>
	/// This object represents a nested state of the execution
	/// in a hierarchy.
	/// </remarks>
	/// <seealso cref="IBlock"/>
	/// <seealso cref="IQuery"/>
	public interface IRequest : IDisposable {
		/// <summary>
		/// Gets the query that provides the request.
		/// </summary>
		/// <remarks>
		/// If the request is a <see cref="IQuery"/>, this
		/// property returns the query itself.
		/// </remarks>
		IQuery Query { get; }

		/// <summary>
		/// Gets the isolated context of the request.
		/// </summary>
		/// <seealso cref="IContext"/>
		/// <seealso cref="IBlockContext"/>
		IBlockContext Context { get; }


		/// <summary>
		/// Creates a block that is the child of this request.
		/// </summary>
		/// <returns>
		/// Returns an instance of <see cref="IBlock"/> that is
		/// the direct child of this request and that inherits
		/// the context and the reference of this request.
		/// </returns>
		/// <seealso cref="IBlock"/>
		IBlock CreateBlock();
	}
}
