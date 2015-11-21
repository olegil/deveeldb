﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Deveel.Data.Sql.Parser {
	class GrantRoleStatementNode : SqlNode, IStatementNode {
		public IEnumerable<string> Grantees { get; private set; }
				
		public	 IEnumerable<string> Roles { get; private set; }
		
		public	bool WithAdmin { get; private set; }

		protected override ISqlNode OnChildNode(ISqlNode node) {
			if (node.NodeName.Equals("role_list")) {
				GetRoleList(node);
			} else if (node.NodeName.Equals("distribution_list")) {
				GetGrantees(node);
			} else if (node.NodeName.Equals("with_admin_opt")) {
				GetWithAdmin(node);
			}

			return base.OnChildNode(node);
		}

		private void GetGrantees(ISqlNode node) {
			Grantees = node.ChildNodes.OfType<IdentifierNode>().Select(x => x.Text);
		}

		private void GetRoleList(ISqlNode node) {
			Roles = node.ChildNodes.OfType<IdentifierNode>().Select(x => x.Text);
		}

		private void GetWithAdmin(ISqlNode node) {
			if (node.ChildNodes.Any())
				WithAdmin = true;
		}
	}
}