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
using System.Collections.Generic;
using System.Linq;

namespace Deveel.Data.Sql.Compile {
	/// <summary>
	/// The root node of an expression used to select a set of items
	/// from a set of sources defined, given some conditions specified.
	/// </summary>
	[Serializable]
	public sealed class SqlQueryExpressionNode : SqlNode, IExpressionNode {
		/// <summary>
		/// Gets a boolean value indicating if the selection will return
		/// all columns from the sources specified in <see cref="FromClause"/>.
		/// </summary>
		public bool IsAll { get; private set; }

		/// <summary>
		/// Gets a boolean value indicating if the selection will return
		/// only results that are unique.
		/// </summary>
		public bool IsDistinct { get; private set; }

		/// <summary>
		/// Gets a read-only list of <see cref="SelectItemNode">items</see> that
		/// will be returned by the query.
		/// </summary>
		public IEnumerable<SelectItemNode> SelectItems { get; private set; }

		/// <summary>
		/// Gets the clause defining the sources from where to query.
		/// </summary>
		public FromClauseNode FromClause { get; private set; }

		/// <summary>
		/// Gets an optional clause that is used to filter the queried objects.
		/// </summary>
		public WhereClauseNode WhereExpression { get; private set; }

		/// <summary>
		/// Gets an optional clause used to group and filter the results of
		/// a query.
		/// </summary>
		public GroupByNode GroupBy { get; private set; }

		/// <summary>
		/// Gets a read-oly list of <see cref="OrderBy">order</see> criteria
		/// for sorting the results of the query.
		/// </summary>
		/// <seealso cref="OrderByNode"/>
		public IEnumerable<OrderByNode> OrderBy { get; private set; }

		/// <summary>
		/// Gets an optional definition for a composition between this
		/// query and another.
		/// </summary>
		/// <seealso cref="QueryCompositeNode"/>
		public QueryCompositeNode Composite { get; private set; }

		/// <inheritdoc/>
		protected override ISqlNode OnChildNode(ISqlNode node) {
			if (node.NodeName == "select_restrict_opt") {
				GetRestrict(node);
			} else if (node.NodeName == "select_set") {
				GetSelectSet(node);
			} else if (node.NodeName == "from_clause_opt") {
				var clause = node.ChildNodes.FirstOrDefault();
				if (clause != null)
					FromClause = (FromClauseNode)clause;
			} else if (node.NodeName == "query_composite_opt") {
				var composite = node.ChildNodes.FirstOrDefault();
				if (composite != null)
					Composite = (QueryCompositeNode) composite;
			}

			return base.OnChildNode(node);
		}

		private void GetRestrict(ISqlNode node) {
			foreach (var childNode in node.ChildNodes) {
				if (childNode is SqlKeyNode) {
					var value = ((SqlKeyNode) childNode).Text;
					if (value == "ALL") {
						IsAll = true;
					} else if (value == "DISTINCT") {
						IsDistinct = true;
					}
				}
			}
		}

		private void GetSelectSet(ISqlNode node) {
			foreach (var childNode in node.ChildNodes) {
				if (childNode is SqlKeyNode &&
				    ((SqlKeyNode) childNode).Text == "*") {
					IsAll = true;
				} else if (childNode.NodeName == "select_item_list") {
					GetSelectItems(childNode);
				}
			}
		}

		private void GetSelectItems(ISqlNode node) {
			var items = new List<SelectItemNode>();
			foreach (var childNode in node.ChildNodes) {
				if (childNode is SelectItemNode)
					items.Add((SelectItemNode)childNode);
			}

			SelectItems = items.AsReadOnly();
		}
	}
}