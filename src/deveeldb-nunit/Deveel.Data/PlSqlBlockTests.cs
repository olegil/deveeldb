﻿using System;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Cursors;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Statements;
using Deveel.Data.Sql.Tables;
using Deveel.Data.Sql.Types;

using NUnit.Framework;

namespace Deveel.Data {
	[TestFixture]
	public sealed class PlSqlBlockTests : ContextBasedTest {
		protected override bool OnSetUp(string testName, IQuery query) {
			var tableInfo = new TableInfo(ObjectName.Parse("APP.test_table"));
			tableInfo.AddColumn("a", PrimitiveTypes.Integer());
			tableInfo.AddColumn("b", PrimitiveTypes.String(), false);

			query.Access().CreateTable(tableInfo);
			return true;
		}

		protected override bool OnTearDown(string testName, IQuery query) {
			var tableName = ObjectName.Parse("APP.test_table");
			query.Access().DropObject(DbObjectType.Table, tableName);
			return true;
		}

		[Test]
		public void DeclareAndOpenCursor() {
			var query = (SqlQueryExpression) SqlExpression.Parse("SELECT * FROM test_table");
			var block = new PlSqlBlockStatement();
			block.Declarations.Add(new DeclareCursorStatement("c1", query));
			block.Statements.Add(new OpenStatement("c1"));

			Query.ExecuteStatement(block);

			Assert.IsTrue(Query.Context.CursorExists("c1"));

			var cursor = Query.Context.FindCursor("c1");
			Assert.AreEqual(CursorStatus.Open, cursor.Status);
		}
	}
}
