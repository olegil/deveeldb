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

using Deveel.Data;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Query;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Statements {
	public sealed class SelectStatement : SqlStatement {
		public SelectStatement(SqlQueryExpression queryExpression) 
			: this(queryExpression, null) {
		}

		public SelectStatement(SqlQueryExpression queryExpression, IEnumerable<SortColumn> orderBy) {
			if (queryExpression == null)
				throw new ArgumentNullException("queryExpression");

			QueryExpression = queryExpression;
			OrderBy = orderBy;
		}

		public SqlQueryExpression QueryExpression { get; private set; }

		public IEnumerable<SortColumn> OrderBy { get; set; }

		public QueryLimit Limit { get; set; }

		protected override SqlStatement PrepareStatement(IRequest context) {
			var queryPlan = context.Query.Context.QueryPlanner().PlanQuery(context, QueryExpression, OrderBy, Limit);
			return new Prepared(queryPlan);
		}

		#region Prepared

		class Prepared : SqlStatement {
			public Prepared(IQueryPlanNode queryPlan) {
				QueryPlan = queryPlan;
			}

			public IQueryPlanNode QueryPlan { get; private set; }

			protected override bool IsPreparable {
				get { return false; }
			}

			protected override ITable ExecuteStatement(IRequest context) {
				return QueryPlan.Evaluate(context);
			}
		}

		#endregion
	}
}
