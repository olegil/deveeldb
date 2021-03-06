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

using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Objects;
using Deveel.Data.Sql.Sequences;
using Deveel.Data.Sql.Tables;
using Deveel.Data.Sql.Types;

using NUnit.Framework;

namespace Deveel.Data {
	[TestFixture]
	public sealed class SequenceFunctionTests : FunctionTestBase {
		protected override bool OnSetUp(string testName, IQuery query) {
			var info = new SequenceInfo(ObjectName.Parse("APP.seq1"),
				new SqlNumber(0), new SqlNumber(1), new SqlNumber(0), new SqlNumber(Int64.MaxValue), false);

			query.Access().CreateObject(info);

			if (testName.EndsWith("TableId")) {
				CreateTestTable(query);
				InsertTestData(query);
			}

			return true;
		}

		protected override bool OnTearDown(string testName, IQuery query) {
			query.Access().DropObject(DbObjectType.Sequence, ObjectName.Parse("APP.seq1"));

			if (testName.EndsWith("TableId")) {
				var tableName = ObjectName.Parse("APP.test_table");
				query.Access().DropAllTableConstraints(tableName);
				query.Access().DropObject(DbObjectType.Table, tableName);
			}

			return true;
		}

		private void CreateTestTable(IQuery query) {
			var tableName = ObjectName.Parse("APP.test_table");
			var tableInfo = new TableInfo(tableName);
			var id = tableInfo.AddColumn("id", PrimitiveTypes.Integer());
			id.DefaultExpression = SqlExpression.FunctionCall("UNIQUEKEY", new SqlExpression[] {
				SqlExpression.Constant("APP.test_table")
			});
			tableInfo.AddColumn("name", PrimitiveTypes.String());
			query.Access().CreateTable(tableInfo);
			query.Access().AddPrimaryKey(tableName, "id");
		}

		private void InsertTestData(IQuery query) {
			query.Insert(ObjectName.Parse("APP.test_table"), new[] { "name" }, new SqlExpression[] {
				SqlExpression.Constant("Antonello")
			});
		}

		[Test]
		public void NextValue() {
			var result = Select("NEXTVAL", SqlExpression.Constant("APP.seq1"));

			Assert.IsNotNull(result);
			Assert.IsInstanceOf<NumericType>(result.Type);
			Assert.IsInstanceOf<SqlNumber>(result.Value);

			var value = (SqlNumber) result.Value;
			Assert.AreEqual(new SqlNumber(1), value);
		}

		[Test]
		public void CurrentValue() {
			var result = Select("CURVAL", SqlExpression.Constant("APP.seq1"));

			Assert.IsNotNull(result);
			Assert.IsInstanceOf<NumericType>(result.Type);
			Assert.IsInstanceOf<SqlNumber>(result.Value);

			var value = (SqlNumber)result.Value;
			Assert.AreEqual(new SqlNumber(0), value);
		}

		[Test]
		public void CurrentValue_TableId() {
			var result = Select("CURVAL", SqlExpression.Constant("test_table"));

			Assert.IsNotNull(result);
			Assert.IsInstanceOf<NumericType>(result.Type);
			Assert.IsInstanceOf<SqlNumber>(result.Value);

			var value = (SqlNumber)result.Value;
			Assert.AreEqual(new SqlNumber(1), value);
		}
	}
}
