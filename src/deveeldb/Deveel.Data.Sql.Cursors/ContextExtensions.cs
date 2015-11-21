﻿using System;

using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Cursors {
	public static class ContextExtensions {
		public static bool DeclareCursor(this IContext context, CursorInfo cursorInfo) {
			if (context.CursorExists(cursorInfo.CursorName))
				throw new InvalidOperationException(String.Format("A cursor named '{0}' was already defined in the context.",
					cursorInfo.CursorName));

			var currentContext = context;
			while (currentContext != null) {
				if (currentContext is ICursorScope) {
					var scope = (ICursorScope)currentContext;
					scope.DeclareCursor(cursorInfo);
					return true;
				}

				currentContext = currentContext.Parent;
			}

			return false;
		}

		public static void DeclareCursor(this IContext context, string cursorName, SqlQueryExpression query) {
			DeclareCursor(context, cursorName, (CursorFlags)0, query);
		}

		public static void DeclareCursor(this IContext context, string cursorName, CursorFlags flags, SqlQueryExpression query) {
			context.DeclareCursor(new CursorInfo(cursorName, flags, query));
		}

		public static void DeclareInsensitiveCursor(this IContext context, string cursorName, SqlQueryExpression query) {
			DeclareInsensitiveCursor(context, cursorName, query, false);
		}

		public static void DeclareInsensitiveCursor(this IContext context, string cursorName, SqlQueryExpression query, bool withScroll) {
			var flags = CursorFlags.Insensitive;
			if (withScroll)
				flags |= CursorFlags.Scroll;

			context.DeclareCursor(cursorName, flags, query);
		}


		public static bool CursorExists(this IContext context, string cursorName) {
			var currentContext = context;
			while (currentContext != null) {
				if (currentContext is ICursorScope) {
					var scope = (ICursorScope) currentContext;
					if (scope.CursorExists(cursorName))
						return true;
				}

				currentContext = currentContext.Parent;
			}

			return false;
		}

		public static Cursor FindCursor(this IContext context, string cursorName) {
			var currentContext = context;
			while (currentContext != null) {
				if (currentContext is ICursorScope) {
					var scope = (ICursorScope)currentContext;
					var cursor = scope.GetCursor(cursorName);
					if (cursor != null)
						return cursor;
				}

				currentContext = currentContext.Parent;
			}

			return null;
		}

		public static bool DropCursor(this IContext context, string cursorName) {
			var currentContext = context;
			while (currentContext != null) {
				if (currentContext is ICursorScope) {
					var scope = (ICursorScope)currentContext;
					if (scope.CursorExists(cursorName))
						return scope.DropCursor(cursorName);
				}

				currentContext = currentContext.Parent;
			}

			return false;
		}

		public static bool CloseCursor(this IContext context, string cursorName) {
			var cursor = context.FindCursor(cursorName);
			if (cursor == null)
				return false;

			cursor.Close();
			return true;
		}

		public static bool OpenCursor(this IContext context, IQuery query, string cursorName, params  SqlExpression[] args) {
			var cursor = context.FindCursor(cursorName);
			if (cursor == null)
				return false;

			// TODO: support the evaluate in context (and not just IQueryContext)
			throw new NotImplementedException();
		}
	}
}