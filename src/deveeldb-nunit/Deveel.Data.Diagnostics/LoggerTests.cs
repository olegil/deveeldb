﻿using System;
using System.Linq;

using Deveel.Data.Services;

using NUnit.Framework;

namespace Deveel.Data.Diagnostics {
	[TestFixture]
	public class LoggerTests : ContextBasedTest {
		/*
		TODO:
		[Test]
		public void LogErrorToConsole() {
			var routers = SystemContext.ResolveAllServices<IEventRouter>().ToList();
			Assert.IsNotEmpty(routers);
			Assert.AreEqual(1, routers.Count);
			Assert.IsInstanceOf<LogEventRouter>(routers[0]);

			var loggers = SystemContext.ResolveAllServices<IEventLogger>().ToList();
			Assert.IsNotEmpty(loggers);
			Assert.AreEqual(1, loggers.Count);
			Assert.IsInstanceOf<ConsoleEventLogger>(loggers[0]);

			Assert.DoesNotThrow(() => Query.RegisterError("Error one"));
		}
		*/
	}
}
