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

using Deveel.Data.Sql;
using Deveel.Data.Sql.Statements;

namespace Deveel.Data.Routines {
	public sealed class PlSqlProcedureInfo : ProcedureInfo {
		public PlSqlProcedureInfo(ObjectName procedureName, RoutineParameter[] parameters, SqlStatement body)
			: base(procedureName, parameters) {
			if (body == null)
				throw new ArgumentNullException("body");

			Body = body;
		}

		public SqlStatement Body { get; private set; }
	}
}
