﻿using System;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Triggers;

using NUnit.Framework;

namespace Deveel.Data {
	[TestFixture]
	public sealed class DropTriggerTests : ContextBasedTest {
		private ObjectName triggerName;

		protected override bool OnSetUp(string testName, IQuery query) {
			triggerName = ObjectName.Parse("APP.trigger1");
			query.Access().CreateTrigger(new ProcedureTriggerInfo(triggerName, ObjectName.Parse("APP.table1"),
				TriggerEventType.AfterDelete, ObjectName.Parse("APP.proc1")));

			return true;
		}

		protected override bool OnTearDown(string testName, IQuery query) {
			query.Access().DropTrigger(triggerName);
			return true;
		}

		[Test]
		public void ExistingTrigger() {
			Query.DropTrigger(triggerName);

			var exists = Query.Access().TriggerExists(triggerName);

			Assert.IsFalse(exists);
		}
	}
}
