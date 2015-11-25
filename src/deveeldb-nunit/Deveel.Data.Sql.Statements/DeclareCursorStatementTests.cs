﻿using System;
using System.Linq;

using Deveel.Data.Sql.Cursors;
using Deveel.Data.Sql.Tables;
using Deveel.Data.Types;

using NUnit.Framework;

namespace Deveel.Data.Sql.Statements {
	[TestFixture]
	public class DeclareCursorStatementTests : ContextBasedTest {
		protected override ISession CreateAdminSession(IDatabase database) {
			using (var session = base.CreateAdminSession(database)) {
				using (var query = session.CreateQuery()) {
					var tableInfo = new TableInfo(ObjectName.Parse("APP.test_table"));
					tableInfo.AddColumn("a", PrimitiveTypes.Integer());
					tableInfo.AddColumn("b", PrimitiveTypes.String(), false);

					query.CreateTable(tableInfo, false, false);
					query.Commit();
				}
			}

			return base.CreateAdminSession(database);
		}

		[Test]
		public void DeclareInsensitiveCursor_NoArguments() {
			const string sql = "DECLARE CURSOR c1 INSENSITIVE IS SELECT * FROM test_table WHERE a = 1";

			var statements = SqlStatement.Parse(sql);
			Assert.IsNotNull(statements);

			var statementList = statements.ToList();
			Assert.IsNotEmpty(statementList);
			Assert.AreEqual(1, statementList.Count);
			Assert.IsInstanceOf<DeclareCursorStatement>(statementList[0]);

			var statement = (DeclareCursorStatement) statementList[0];
			Assert.AreEqual("c1", statement.CursorName);
			Assert.AreNotEqual(0, (statement.Flags & CursorFlags.Insensitive));
			Assert.IsEmpty(statement.Parameters);
		}

		[Test]
		public void DeclareCursorWithArguments() {
			const string sql = "DECLARE CURSOR c1 (arg1 INT, arg2 VARCHAR) IS SELECT * FROM test_table WHERE a = arg1";

			var statements = SqlStatement.Parse(sql);
			Assert.IsNotNull(statements);

			var statementList = statements.ToList();
			Assert.IsNotEmpty(statementList);
			Assert.AreEqual(1, statementList.Count);
			Assert.IsInstanceOf<DeclareCursorStatement>(statementList[0]);

			var statement = (DeclareCursorStatement)statementList[0];
			Assert.AreEqual("c1", statement.CursorName);
			Assert.IsNotEmpty(statement.Parameters);

			var arg1 = statement.Parameters.First();
			Assert.IsNotNull(arg1);
			Assert.AreEqual("arg1", arg1.ParameterName);
			Assert.IsInstanceOf<NumericType>(arg1.ParameterType);
			Assert.AreEqual(0, arg1.Offset);
		}
	}
}
