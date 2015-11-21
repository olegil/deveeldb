﻿using System;

using Deveel.Data.Security;
using Deveel.Data.Sql.Variables;
using Deveel.Data.Transactions;

namespace Deveel.Data.Sql {
	public static class QueryContext {
		public static bool ObjectExists(this IQueryContext context, ObjectName objectName) {
			// Special types for these database objects that can be 
			// declared in a limited context
			if (context.CursorManager.CursorExists(objectName))
				return true;
			if (context.VariableExists(objectName.Name))
				return true;

			if (context.ParentContext != null &&
				context.ParentContext.ObjectExists(objectName))
				return true;

			// We haven't found it neither in this context nor in the parent: 
			//   fallback to the transaction scope
			return context.Session().ObjectExists(objectName);
		}

		public static bool ObjectExists(this IQueryContext context, DbObjectType objectType, ObjectName objectName) {
			// Special types for these database objects that can be 
			// declared in a limited context
			if (objectType == DbObjectType.Cursor &&
				context.CursorManager.CursorExists(objectName))
				return true;

			if (objectType == DbObjectType.Variable &&
				context.VariableExists(objectName.Name))
				return true;

			if (context.ParentContext != null &&
				context.ParentContext.ObjectExists(objectType, objectName))
				return true;

			// We haven't found it neither in this context nor in the parent: 
			//   fallback to the transaction scope
			return context.Session().ObjectExists(objectType, objectName);
		}

		public static IDbObject GetObject(this IQueryContext context, DbObjectType objType, ObjectName objName) {
			return GetObject(context, objType, objName, AccessType.ReadWrite);
		}

		public static IDbObject GetObject(this IQueryContext context, DbObjectType objType, ObjectName objName, AccessType accessType) {
			// First handle the special cases of cursors and variable, that can be declared
			//  in a query context
			// If they are declared in the context, the user owns them and we don't need
			//  to verify the ownership
			if (objType == DbObjectType.Cursor) {
				var obj = context.CursorManager.GetCursor(objName.Name);
				if (obj != null)
					return obj;
			} else if (objType == DbObjectType.Variable) {
				var obj = context.FindVariable(objName.Name);
				if (obj != null)
					return obj;
			}

			if (context.ParentContext != null) {
				var obj = context.ParentContext.GetObject(objType, objName, accessType);
				if (obj != null)
					return obj;
			}

			// TODO: throw a specialized exception
			if (!context.UserCanAccessObject(objType, objName))
				throw new InvalidOperationException();

			return context.Session().GetObject(objType, objName, accessType);
		}

		internal static void CreateObject(this IQueryContext context, IObjectInfo objectInfo) {
			// TODO: throw a specialized exception
			if (!context.UserCanCreateObject(objectInfo.ObjectType, objectInfo.FullName))
				throw new InvalidOperationException();

			context.Session().CreateObject(objectInfo);
		}

		public static bool DropObject(this IQueryContext context, DbObjectType objectType, ObjectName objectName) {
			if (objectType == DbObjectType.Variable &&
				context.DropVariable(objectName.Name)) {
				return true;
			}
			if (objectType == DbObjectType.Cursor &&
				context.CursorManager.DropCursor(objectName)) {
				return true;
			}

			if (context.ParentContext != null &&
				context.ParentContext.DropObject(objectType, objectName))
				return true;

			if (!context.UserCanDropObject(objectType, objectName))
				throw new MissingPrivilegesException(context.UserName(), objectName, Privileges.Drop);

			context.Session().DropObject(objectType, objectName);
			return true;
		}

		public static void AlterObject(this IQueryContext context, IObjectInfo objectInfo) {
			if (objectInfo == null)
				throw new ArgumentNullException("objectInfo");

			if (!context.UserCanAlterObject(objectInfo.ObjectType, objectInfo.FullName))
				throw new MissingPrivilegesException(context.UserName(), objectInfo.FullName, Privileges.Alter);

			context.Session().AlterObject(objectInfo);
		}

		public static ObjectName ResolveObjectName(this IQueryContext context, string name) {
			if (context.VariableExists(name) ||
				context.CursorManager.CursorExists(new ObjectName(name)))
				return new ObjectName(name);

			ObjectName resolved;
			if (context.ParentContext != null &&
				(resolved = context.ParentContext.ResolveObjectName(name)) != null)
				return resolved;

			return context.Session().ResolveObjectName(name);
		}

		public static ObjectName ResolveObjectName(this IQueryContext context, DbObjectType objectType, ObjectName objectName) {
			if (objectType == DbObjectType.Variable &&
				context.VariableExists(objectName.Name))
				return new ObjectName(objectName.Name);
			if (objectType == DbObjectType.Cursor &&
				context.CursorManager.CursorExists(objectName))
				return new ObjectName(objectName.Name);

			ObjectName resolved;
			if (context.ParentContext != null &&
				(resolved = context.ParentContext.ResolveObjectName(objectType, objectName)) != null)
				return resolved;

			return context.Session().ResolveObjectName(objectType, objectName);
		}

		public static IDbObject FindObject(this IQueryContext context, ObjectName objectName) {
			return context.Session().FindObject(objectName);
		}
	}
}