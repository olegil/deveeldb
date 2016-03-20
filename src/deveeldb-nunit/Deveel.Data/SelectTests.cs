﻿using System;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Objects;
using Deveel.Data.Sql.Tables;
using Deveel.Data.Sql.Types;

using NUnit.Framework;

namespace Deveel.Data {
	[TestFixture]
	public sealed class SelectTests : ContextBasedTest {
		protected override void OnSetUp(string testName) {
			CreateTestTable(Query);
			AddTestData(Query);
		}

		private static void CreateTestTable(IQuery context) {
			var tableInfo = new TableInfo(ObjectName.Parse("APP.test_table"));
			var idColumn = tableInfo.AddColumn("id", PrimitiveTypes.Integer());
			idColumn.DefaultExpression = SqlExpression.FunctionCall("UNIQUEKEY",
				new SqlExpression[] { SqlExpression.Constant(tableInfo.TableName.FullName) });
			tableInfo.AddColumn("first_name", PrimitiveTypes.String());
			tableInfo.AddColumn("last_name", PrimitiveTypes.String());
			tableInfo.AddColumn("birth_date", PrimitiveTypes.DateTime());
			tableInfo.AddColumn("active", PrimitiveTypes.Boolean());

			context.Session.Access.CreateTable(tableInfo);
			context.Session.Access.AddPrimaryKey(tableInfo.TableName, "id", "PK_TEST_TABLE");
		}

		private static void AddTestData(IQuery context) {
			var table = context.Access.GetMutableTable(ObjectName.Parse("APP.test_table"));
			var row = table.NewRow();

			// row.SetValue("id", Field.Integer(0));
			row.SetDefault(0, context);
			row.SetValue("first_name", Field.String("John"));
			row.SetValue("last_name", Field.String("Doe"));
			row.SetValue("birth_date", Field.Date(new SqlDateTime(1977, 01, 01)));
			row.SetValue("active", Field.Boolean(false));
			table.AddRow(row);

			row = table.NewRow();

			// row.SetValue("id", Field.Integer(1));
			row.SetDefault(0, context);
			row.SetValue("first_name", Field.String("Jane"));
			row.SetValue("last_name", Field.String("Doe"));
			row.SetValue("birth_date", Field.Date(new SqlDateTime(1978, 11, 01)));
			row.SetValue("active", Field.Boolean(true));
			table.AddRow(row);

			row = table.NewRow();

			// row.SetValue("id", Field.Integer(2));
			row.SetDefault(0, context);
			row.SetValue("first_name", Field.String("Roger"));
			row.SetValue("last_name", Field.String("Rabbit"));
			row.SetValue("birth_date", Field.Date(new SqlDateTime(1985, 05, 05)));
			row.SetValue("active", Field.Boolean(true));
			table.AddRow(row);
		}

		[Test]
		public void AllColumns() {
			var query = (SqlQueryExpression)SqlExpression.Parse("SELECT * FROM test_table");

			var result = Query.Select(query);

			Assert.IsNotNull(result);
			Assert.AreEqual(3, result.RowCount);
		}

		[Test]
		public void OrderedSelect() {
			var query = (SqlQueryExpression)SqlExpression.Parse("SELECT * FROM test_table");
			var sort = new[] { new SortColumn(SqlExpression.Reference(new ObjectName("birth_date")), false) };

			var result = Query.Select(query, sort);

			Assert.IsNotNull(result);
			Assert.AreEqual(3, result.RowCount);

			var firstName = result.GetValue(0, 1);

			Assert.AreEqual("Roger", firstName.Value.ToString());
		}

		[Test]
		public void SelectAliasedWithGroupedExpression() {
			var query = (SqlQueryExpression)SqlExpression.Parse("SELECT * FROM test_table t0 WHERE (t0.id = 1 AND t0.id <> 0)");

			var result = Query.Select(query);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.RowCount);
		}

		[Test]
		public void SelectFromAliased() {
			var query = (SqlQueryExpression)SqlExpression.Parse("SELECT * FROM test_table t0 WHERE t0.id = 1");

			var result = Query.Select(query);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.RowCount);
		}

	}
}