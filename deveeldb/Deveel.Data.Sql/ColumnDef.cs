//  
//  ColumnDef.cs
//  
//  Author:
//       Antonello Provenzano <antonello@deveel.com>
//       Tobias Downer <toby@mckoi.com>
// 
//  Copyright (c) 2009 Deveel
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;

namespace Deveel.Data.Sql {
	/// <summary>
	/// Represents a column definition (description).
	/// </summary>
	[Serializable]
	public sealed class ColumnDef : IStatementTreeObject {
		private String name;

		private TType type;
		private String index_str;

		internal Expression default_expression;
		internal Expression original_default_expression;

		private bool not_null;
		private bool primary_key;
		private bool unique;
		private bool identity;

		internal ColumnDef() {
		}


		/// <summary>
		/// Returns true if this column has a primary key constraint set on it.
		/// </summary>
		public bool IsPrimaryKey {
			get { return primary_key; }
		}

		/// <summary>
		/// Returns true if this column has the unique constraint set for it.
		/// </summary>
		public bool IsUnique {
			get { return unique; }
		}

		/// <summary>
		/// Returns true if this column has the not null constraint set for it.
		/// </summary>
		public bool IsNotNull {
			get { return not_null; }
		}

		/// <summary>
		/// Gets or sets the name of the column.
		/// </summary>
		public string Name {
			get { return name; }
			set { name = value; }
		}

		///<summary>
		/// Sets the type of data of this column.
		///</summary>
		public TType Type {
			get { return type; }
			set { type = value; }
		}

		///<summary>
		///</summary>
		public string IndexScheme {
			get { return index_str; }
		}

		public bool Identity {
			get { return identity; }
			set {
				if (value && !(Type is TNumericType))
					throw new ParseException("Cannot set a non-numeric column as PRIMARY.");
				identity = value;
			}
		}

		///<summary>
		/// Adds a constraint to this column.
		///</summary>
		///<param name="constraint"></param>
		///<exception cref="Exception"></exception>
		public void AddConstraint(String constraint) {
			if (constraint.Equals("NOT NULL")) {
				not_null = true;
			} else if (constraint.Equals("NULL")) {
				not_null = false;
			} else if (constraint.Equals("PRIMARY")) {
				primary_key = true;
			} else if (constraint.Equals("UNIQUE")) {
				unique = true;
			} else {
				throw new Exception("Unknown constraint: " + constraint);
			}
		}


		///<summary>
		/// Sets the indexing.
		///</summary>
		///<param name="t"></param>
		///<exception cref="ParseException"></exception>
		public void SetIndex(Token t) {
			if (t.kind == SQLConstants.INDEX_NONE) {
				index_str = "BlindSearch";
			} else if (t.kind == SQLConstants.INDEX_BLIST) {
				index_str = "InsertSearch";
			} else {
				throw new ParseException("Unrecognized indexing scheme.");
			}
		}

		///<summary>
		/// Sets the default expression (this is used to make a new constraint).
		///</summary>
		///<param name="exp"></param>
		///<exception cref="ApplicationException"></exception>
		public void SetDefaultExpression(Expression exp) {
			if (identity)
				throw new ParseException("Cannot specify a DEFAULT expression for an IDENTITY column.");

			default_expression = exp;
			try {
				original_default_expression = (Expression)exp.Clone();
			} catch (Exception e) {
				throw new ApplicationException(e.Message);
			}
		}


		/// <inheritdoc/>
		public void PrepareExpressions(IExpressionPreparer preparer) {
			if (default_expression != null) {
				default_expression.Prepare(preparer);
			}
		}

		/// <inheritdoc/>
		public Object Clone() {
			ColumnDef v = (ColumnDef)MemberwiseClone();
			if (default_expression != null) {
				v.default_expression = (Expression)default_expression.Clone();
			}
			return v;
		}
	}
}