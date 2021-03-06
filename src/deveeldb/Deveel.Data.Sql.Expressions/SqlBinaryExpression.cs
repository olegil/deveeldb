﻿// 
//  Copyright 2010-2016 Deveel
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
//


using System;
using System.Runtime.Serialization;

namespace Deveel.Data.Sql.Expressions {
	[Serializable]
	public sealed class SqlBinaryExpression : SqlExpression {
		private readonly SqlExpressionType expressionType;

		internal SqlBinaryExpression(SqlExpression left, SqlExpressionType expressionType, SqlExpression right) {
			if (left == null)
				throw new ArgumentNullException("left");
			if (right == null)
				throw new ArgumentNullException("right");

			this.expressionType = expressionType;

			Left = left;
			Right = right;
		}

		private SqlBinaryExpression(SerializationInfo info, StreamingContext context)
			: base(info, context) {
			Left = (SqlExpression)info.GetValue("Left", typeof(SqlExpression));
			Right = (SqlExpression)info.GetValue("Right", typeof(SqlExpression));
			expressionType = (SqlExpressionType) info.GetInt32("ExpressionType");
		}

		public SqlExpression Left { get; private set; }

		public SqlExpression Right { get; private set; }

		public override bool CanEvaluate {
			get { return true; }
		}

		public override SqlExpressionType ExpressionType {
			get { return expressionType; }
		}

		protected override void GetData(SerializationInfo info, StreamingContext context) {
			info.AddValue("Left", Left, typeof(SqlExpression));
			info.AddValue("Right", Right, typeof(SqlExpression));
			info.AddValue("ExpressionType", (int)expressionType);
		}

		internal override void AppendTo(SqlStringBuilder builder) {
			Left.AppendTo(builder);

			var binaryOpString = GetBinaryOperatorString(ExpressionType);
			builder.AppendFormat(" {0} ", binaryOpString);

			Right.AppendTo(builder);
		}

		private static string GetBinaryOperatorString(SqlExpressionType expressionType) {
			switch (expressionType) {
				case SqlExpressionType.Add:
					return "+";
				case SqlExpressionType.Subtract:
					return "-";
				case SqlExpressionType.Divide:
					return "/";
				case SqlExpressionType.Multiply:
					return "*";
				case SqlExpressionType.Modulo:
					return "%";
				case SqlExpressionType.Equal:
					return "=";
				case SqlExpressionType.NotEqual:
					return "<>";
				case SqlExpressionType.GreaterThan:
					return ">";
				case SqlExpressionType.GreaterOrEqualThan:
					return ">=";
				case SqlExpressionType.SmallerThan:
					return "<";
				case SqlExpressionType.SmallerOrEqualThan:
					return "<=";
				case SqlExpressionType.Is:
					return "IS";
				case SqlExpressionType.IsNot:
					return "IS NOT";
				case SqlExpressionType.Like:
					return "LIKE";
				case SqlExpressionType.NotLike:
					return "NOT LIKE";
				case SqlExpressionType.Or:
					return "OR";
				case SqlExpressionType.And:
					return "AND";
				case SqlExpressionType.XOr:
					return "XOR";
				default:
					throw new NotSupportedException();
			}
		}
	}
}