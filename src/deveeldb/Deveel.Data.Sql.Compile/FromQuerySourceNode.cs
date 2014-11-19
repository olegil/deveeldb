﻿// 
//  Copyright 2010-2014 Deveel
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

using System;
using System.Linq;

namespace Deveel.Data.Sql.Compile {
	/// <summary>
	/// A node in the grammar tree that defines a sub-query in a
	/// <c>FROM</c> clause.
	/// </summary>
	/// <seealso cref="IFromSourceNode"/>
	public sealed class FromQuerySourceNode : SqlNode, IFromSourceNode {
		/// <summary>
		/// Gets the <see cref="SqlQueryExpressionNode">node</see> that represents
		/// the sub-qury that is the source of a query.
		/// </summary>
		public SqlQueryExpressionNode Query { get; private set; }

		/// <inheritdoc/>
		public string Alias { get; private set; }

		/// <inheritdoc/>
		public JoinNode Join { get; private set; }

		/// <inheritdoc/>
		protected override ISqlNode OnChildNode(ISqlNode node) {
			if (node.NodeName == "query") {
				var expression = node.ChildNodes.FirstOrDefault();
				if (expression != null)
					Query = (SqlQueryExpressionNode) expression;
			} else if (node is IdentifierNode) {
				Alias = ((IdentifierNode) node).Text;
			} else if (node.NodeName == "join_opt") {
				var join = node.ChildNodes.FirstOrDefault();
				if (join != null)
					Join = (JoinNode) join;
			}

			return base.OnChildNode(node);
		}
	}
}