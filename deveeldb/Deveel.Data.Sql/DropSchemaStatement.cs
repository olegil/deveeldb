//  
//  DropSchemaStatement.cs
//  
//  Author:
//       Antonello Provenzano <antonello@deveel.com>
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
	public sealed class DropSchemaStatement : Statement {
		/// <summary>
		/// The name of the schema.
		/// </summary>
		private String schema_name;

		internal override void Prepare() {
			schema_name = GetString("schema_name");
		}

		internal override Table Evaluate() {
			DatabaseQueryContext context = new DatabaseQueryContext(Connection);

			if (!Connection.Database.CanUserCreateAndDropSchema(context, User, schema_name))
				throw new UserAccessException("User not permitted to create or drop schema.");

			bool ignore_case = Connection.IsInCaseInsensitiveMode;
			SchemaDef schema =
						Connection.ResolveSchemaCase(schema_name, ignore_case);
			// Only allow user to drop USER typed schemas
			if (schema == null) {
				throw new DatabaseException("Schema '" + schema_name + "' does not exist.");
			} else if (schema.Type.Equals("USER")) {
				// Check if the schema is empty.
				TableName[] all_tables = Connection.Tables;
				String resolved_schema_name = schema.Name;
				for (int i = 0; i < all_tables.Length; ++i) {
					if (all_tables[i].Schema.Equals(resolved_schema_name)) {
						throw new DatabaseException("Schema '" + schema_name + "' is not empty.");
					}
				}
				// Drop the schema
				Connection.DropSchema(schema.Name);
				// Revoke all the grants for the schema
				Connection.GrantManager.RevokeAllGrantsOnObject(GrantObject.Schema, schema.Name);

			} else {
				throw new DatabaseException("Can not drop schema '" + schema_name + "'");
			}

			return FunctionTable.ResultTable(context, 0);
		}
	}
}