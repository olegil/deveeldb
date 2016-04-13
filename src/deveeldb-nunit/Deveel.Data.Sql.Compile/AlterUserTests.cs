﻿using System;
using System.Linq;

using Deveel.Data.Security;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Statements;

using NUnit.Framework;

namespace Deveel.Data.Sql.Compile {
	[TestFixture]
	public sealed class AlterUserTests : SqlCompileTestBase {
		[Test]
		public void SetPassword() {
			const string sql = "ALTER USER test IDENTIFIED BY PASSWORD '5674hgr'";

			var result = Compile(sql);

			Assert.IsNotNull(result);
			Assert.IsFalse(result.HasErrors);

			Assert.IsNotEmpty(result.Statements);
			Assert.AreEqual(1, result.Statements.Count);

			var statement = result.Statements.ElementAt(0);

			Assert.IsNotNull(statement);
			Assert.IsInstanceOf<AlterUserStatement>(statement);

			var alterUser = (AlterUserStatement) statement;

			Assert.AreEqual("test", alterUser.UserName);

			Assert.IsInstanceOf<SetPasswordAction>(alterUser.AlterAction);

			var setPassword = (SetPasswordAction) alterUser.AlterAction;

			Assert.IsNotNull(setPassword.PasswordExpression);
			Assert.IsInstanceOf<SqlConstantExpression>(setPassword.PasswordExpression);

			var password = (SqlConstantExpression) setPassword.PasswordExpression;

			Assert.AreEqual("5674hgr", password.Value.Value.ToString());
		}

		[Test]
		public void SetLockedStatus() {
			const string sql = "ALTER USER test ACCOUNT LOCK";

			var result = Compile(sql);

			Assert.IsNotNull(result);
			Assert.IsFalse(result.HasErrors);

			Assert.IsNotEmpty(result.Statements);
			Assert.AreEqual(1, result.Statements.Count);

			var statement = result.Statements.ElementAt(0);

			Assert.IsNotNull(statement);
			Assert.IsInstanceOf<AlterUserStatement>(statement);

			var alterUser = (AlterUserStatement)statement;

			Assert.AreEqual("test", alterUser.UserName);

			Assert.IsInstanceOf<SetAccountStatusAction>(alterUser.AlterAction);

			var setStatus = (SetAccountStatusAction)alterUser.AlterAction;

			Assert.AreEqual(UserStatus.Locked, setStatus.Status);
		}
	}
}
