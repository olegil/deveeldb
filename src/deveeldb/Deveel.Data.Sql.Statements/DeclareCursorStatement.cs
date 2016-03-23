﻿// 
//  Copyright 2010-2015 Deveel
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Deveel.Data.Security;
using Deveel.Data.Sql.Cursors;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Query;

namespace Deveel.Data.Sql.Statements {
	[Serializable]
	public sealed class DeclareCursorStatement : SqlStatement, IDeclarationStatement {
		public DeclareCursorStatement(string cursorName, SqlQueryExpression queryExpression) 
			: this(cursorName, null, queryExpression) {
		}

		public DeclareCursorStatement(string cursorName, IEnumerable<CursorParameter> parameters, SqlQueryExpression queryExpression) 
			: this(cursorName, parameters, CursorFlags.Insensitive, queryExpression) {
		}

		public DeclareCursorStatement(string cursorName, CursorFlags flags, SqlQueryExpression queryExpression) 
			: this(cursorName, null, flags, queryExpression) {
		}

		public DeclareCursorStatement(string cursorName, IEnumerable<CursorParameter> parameters, CursorFlags flags, SqlQueryExpression queryExpression) {
			if (queryExpression == null)
				throw new ArgumentNullException("queryExpression");
			if (String.IsNullOrEmpty(cursorName))
				throw new ArgumentNullException("cursorName");

			CursorName = cursorName;
			Parameters = parameters;
			Flags = flags;
			QueryExpression = queryExpression;
		}

		private DeclareCursorStatement(SerializationInfo info, StreamingContext context) {
			CursorName = info.GetString("CursorName");
			QueryExpression = (SqlQueryExpression) info.GetValue("QueryExpression", typeof(SqlQueryExpression));
			Flags = (CursorFlags) info.GetInt32("Flags");

			var parameters = (CursorParameter[]) info.GetValue("Parameters", typeof(CursorParameter[]));

			if (parameters != null) {
				Parameters = new List<CursorParameter>(parameters);
			}
		}

		public string CursorName { get; private set; }

		public SqlQueryExpression QueryExpression { get; private set; }

		public CursorFlags Flags { get; set; }

		public IEnumerable<CursorParameter> Parameters { get; set; }

		protected override void GetData(SerializationInfo info) {
			info.AddValue("CursorName", CursorName);
			info.AddValue("QueryExpression", QueryExpression);
			info.AddValue("Flags", (int)Flags);

			if (Parameters != null) {
				var parameters = Parameters.ToArray();
				info.AddValue("Parameters", parameters);
			}
		}

		protected override void ExecuteStatement(ExecutionContext context) {
			var cursorInfo = new CursorInfo(CursorName, Flags, QueryExpression);
			if (Parameters != null) {
				foreach (var parameter in Parameters) {
					cursorInfo.Parameters.Add(parameter);
				}
			}

			var queryPlan = context.Request.Context.QueryPlanner().PlanQuery(new QueryInfo(context.Request, QueryExpression));
			var selectedTables = queryPlan.DiscoverTableNames();
			foreach (var tableName in selectedTables) {
				if (!context.User.CanSelectFromTable(tableName))
					throw new MissingPrivilegesException(context.User.Name, tableName, Privileges.Select);
			}


			context.Request.Context.DeclareCursor(cursorInfo);
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.Append("DECLARE ");
			(this as IDeclarationStatement).AppendDeclarationTo(builder);
		}

		void IDeclarationStatement.AppendDeclarationTo(SqlStringBuilder builder) {
			// TODO: Flags ...

			builder.Append(CursorName);

			if (Parameters != null) {
				var pars = Parameters.ToArray();

				builder.Append(" (");

				for (int i = 0; i < pars.Length; i++) {
					var p = pars[i];

					builder.Append(p);

					if (i < pars.Length - 1)
						builder.Append(", ");
				}

				builder.Append(")");
			}

			builder.AppendLine();
			builder.Indent();

			builder.Append(" IS ");
			builder.Append(QueryExpression);

			builder.DeIndent();
		}

		#region Serializer

		//internal class Serializer : ObjectBinarySerializer<DeclareCursorStatement> {
		//	public override void Serialize(DeclareCursorStatement obj, BinaryWriter writer) {
		//		writer.Write(obj.CursorName);
		//		writer.Write((byte)obj.Flags);

		//		if (obj.Parameters != null) {
		//			var pars = obj.Parameters.ToArray();
		//			var parLength = pars.Length;
		//			writer.Write(parLength);

		//			for (int i = 0; i < parLength; i++) {
		//				CursorParameter.Serialize(pars[i], writer);
		//			}
		//		} else {
		//			writer.Write(0);
		//		}

		//		SqlExpression.Serialize(obj.QueryExpression, writer);
		//	}

		//	public override DeclareCursorStatement Deserialize(BinaryReader reader) {
		//		var cursorName = reader.ReadString();
		//		var flags = (CursorFlags) reader.ReadByte();

		//		var pars = new List<CursorParameter>();
		//		var parLength = reader.ReadInt32();
		//		for (int i = 0; i < parLength; i++) {
		//			var param = CursorParameter.Deserialize(reader);
		//			pars.Add(param);
		//		}

		//		var queryExpression = SqlExpression.Deserialize(reader) as SqlQueryExpression;

		//		return new DeclareCursorStatement(cursorName, pars.ToArray(), flags, queryExpression);
		//	}
		//}

		#endregion
	}
}
