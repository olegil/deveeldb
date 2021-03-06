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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

using Deveel.Data.Routines;
using Deveel.Data.Serialization;
using Deveel.Data.Sql.Compile;
using Deveel.Data.Sql.Types;

namespace Deveel.Data.Sql.Expressions {
	/// <summary>
	/// Defines the base class for instances that represent SQL expression tree nodes.
	/// </summary>
	/// <remarks>
	/// The architecture of the SQL Expression domain is to keep the implementation
	/// internal to the project, that means it will be possible to construct expressions
	/// only through this class, calling factory methods (for example <see cref="Binary"/>).
	/// </remarks>
	[Serializable]
	public abstract class SqlExpression : ISerializable, ISqlFormattable {
		private int precedence;

		/// <summary>
		/// Internally constructs the SQL expression, avoiding external implementations
		/// to be allowed to inherit this class.
		/// </summary>
		internal SqlExpression() {
		}

		internal SqlExpression(SerializationInfo info, StreamingContext context) {
		}

		/// <summary>
		/// Gets the type code of this SQL expression.
		/// </summary>
		public abstract SqlExpressionType ExpressionType { get; }

		/// <summary>
		/// Gets a value indicating whether the expression can be evaluated
		/// to another simpler one.
		/// </summary>
		/// <seealso cref="Evaluate(EvaluateContext)"/>
		public virtual bool CanEvaluate {
			get { return false; }
		}

		internal int EvaluatePrecedence {
			get {
				if (precedence > 0)
					return precedence;

				// Primary
				if (ExpressionType == SqlExpressionType.Reference ||
				    ExpressionType == SqlExpressionType.FunctionCall ||
					ExpressionType == SqlExpressionType.Constant)
					return 150;

				// Unary
				if (ExpressionType == SqlExpressionType.UnaryPlus ||
				    ExpressionType == SqlExpressionType.Negate ||
				    ExpressionType == SqlExpressionType.Not)
					return 140;

				// Cast
				if (ExpressionType == SqlExpressionType.Cast)
					return 139;

				// Multiplicative
				if (ExpressionType == SqlExpressionType.Multiply ||
				    ExpressionType == SqlExpressionType.Divide ||
				    ExpressionType == SqlExpressionType.Modulo)
					return 130;

				// Additive
				if (ExpressionType == SqlExpressionType.Add ||
				    ExpressionType == SqlExpressionType.Subtract)
					return 120;

				// Relational
				if (ExpressionType == SqlExpressionType.GreaterThan ||
				    ExpressionType == SqlExpressionType.GreaterOrEqualThan ||
				    ExpressionType == SqlExpressionType.SmallerThan ||
				    ExpressionType == SqlExpressionType.SmallerOrEqualThan ||
					ExpressionType == SqlExpressionType.Is ||
					ExpressionType == SqlExpressionType.IsNot ||
					ExpressionType == SqlExpressionType.Like ||
					ExpressionType == SqlExpressionType.NotLike)
					return 110;

				// Equality
				if (ExpressionType == SqlExpressionType.Equal ||
				    ExpressionType == SqlExpressionType.NotEqual)
					return 100;

				// Logical
				if (ExpressionType == SqlExpressionType.And)
					return 90;
				if (ExpressionType == SqlExpressionType.Or)
					return 89;
				if (ExpressionType == SqlExpressionType.XOr)
					return 88;

				if (ExpressionType == SqlExpressionType.Conditional)
					return 80;

				if (ExpressionType == SqlExpressionType.Assign)
					return 70;

				if (ExpressionType == SqlExpressionType.Tuple)
					return 60;

				return -1;
			}
			set { precedence = value; }
		}

		public virtual SqlExpression Prepare(IExpressionPreparer preparer) {
			var visitor = new PreparerVisitor(preparer);
			return visitor.Visit(this);
		}

		public virtual SqlExpression Accept(SqlExpressionVisitor visitor) {
			return visitor.Visit(this);
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
			GetData(info, context);
		}

		protected virtual void GetData(SerializationInfo info, StreamingContext context) {
		}

		/// <summary>
		/// When overridden by a derived class, this method evaluates the expression
		/// within the provided context.
		/// </summary>
		/// <param name="context">The context for the evaluation of the expression, providing
		/// access to the system or to the execution context.</param>
		/// <remarks>
		/// <para>
		/// This method is only executed is <see cref="CanEvaluate"/> is <c>true</c>, and the
		/// override method can reduce this expression to a simpler form.
		/// </para>
		/// </remarks>
		/// <returns>
		/// Returns a new <seealso cref="SqlExpression"/> that is the result of the
		/// evaluation of this expression, within the context given.
		/// </returns>
		/// <exception cref="ExpressionEvaluateException">
		/// If any error occurred while evaluating the expression.
		/// </exception>
		public virtual SqlExpression Evaluate(EvaluateContext context) {
			var visitor = new ExpressionEvaluatorVisitor(context);
			return visitor.Visit(this);
		}


		/// <summary>
		/// Statically evaluates the expression, outside any context.
		/// </summary>
		/// <para>
		/// This overload of the <c>Evaluate</c> logic provides an empty context
		/// to <see cref="Evaluate(EvaluateContext)"/>, so that dynamic resolutions
		/// (eg. function calls, states assessments, etc.) will throw an exception.
		/// </para>
		/// <para>
		/// Care must be taken when calling this method, that the expression tree
		/// represented does not contain any reference to dynamically resolved
		/// expressions (<see cref="SqlFunctionCallExpression"/> for example), otherwise
		/// its evaluation will result in an exception state.
		/// </para>
		/// <returns>
		/// Returns a new <seealso cref="SqlExpression"/> that is the result of the
		/// static evaluation of this expression.
		/// </returns>
		/// <exception cref="ExpressionEvaluateException">
		/// If any error occurred while evaluating the expression.
		/// </exception>
		public SqlExpression Evaluate() {
			return Evaluate(null, null);
		}

		public SqlExpression Evaluate(IRequest context, IVariableResolver variables) {
			return Evaluate(context, variables, null);
		}

		public SqlExpression Evaluate(IRequest context, IVariableResolver variables, IGroupResolver group) {
			return Evaluate(new EvaluateContext(context, variables, group));
		}

		public override string ToString() {
			var builder = new SqlStringBuilder();
			AppendTo(builder);
			return builder.ToString();
		}

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			AppendTo(builder);
		}

		internal virtual void AppendTo(SqlStringBuilder builder) {
			
		}

		/// <summary>
		/// Parses the given SQL string to an expression that can be evaluated.
		/// </summary>
		/// <param name="s">The string to parse.</param>
		/// <returns>
		/// Returns an instance of <seealso cref="SqlExpression"/> that represents
		/// the given SQL string parsed.
		/// </returns>
		public static SqlExpression Parse(string s) {
			return Parse(s, new ExpressionParser());
		}

		/// <summary>
		/// Parses the given SQL string to an expression that can be evaluated.
		/// </summary>
		/// <param name="s">The string to parse.</param>
		/// <param name="context"></param>
		/// <returns>
		/// Returns an instance of <seealso cref="SqlExpression"/> that represents
		/// the given SQL string parsed.
		/// </returns>
		public static SqlExpression Parse(string s, IContext context) {
			// TODO: Get the expression compiler from the context
			var parser = context.ResolveService<IExpressionParser>();

			if (parser == null)
				parser = new ExpressionParser();

			return Parse(s, parser);
		}

		private static SqlExpression Parse(string s, IExpressionParser parser) {
			Exception error;
			SqlExpression expression;
			if (!TryParse(s, parser, out expression, out error)) {
				throw new ExpressionFormatException("Unable to parse the exception.", error);
			}

			return expression;
		}

		public static bool TryParse(string s, IExpressionParser parser, out SqlExpression expression) {
			Exception error;
			return TryParse(s, parser, out expression, out error);
		}

		private static bool TryParse(string s, IExpressionParser parser, out SqlExpression expression, out Exception error) {
			if (parser == null) {
				expression = null;
				error = new ArgumentNullException("parser");
				return false;
			}

			try {
				var result = parser.Parse(s);
				if (!result.IsValid) {
					var errors = result.Errors;
					if (errors.Length == 1) {
						error = new FormatException(errors[0]);
					} else {
						// TODO: aggregate the errors ...
						error = new FormatException(String.Join(", ", errors));
					}

					expression = null;
					return false;
				}

				expression = result.Expression;
				error = null;
				return true;
			} catch (Exception ex) {
				error = ex;
				expression = null;
				return false;
			}
		}

		#region Factory Methods 

		#region Primary

		public static SqlConstantExpression Constant(object value) {
			return Constant(Field.Create(value));
		}

		public static SqlConstantExpression Constant(Field value) {
			return new SqlConstantExpression(value);
		}

		public static SqlCastExpression Cast(SqlExpression value, SqlType destType) {
			return new SqlCastExpression(value, destType);
		}

		public static SqlFunctionCallExpression FunctionCall(ObjectName functionName, InvokeArgument[] args) {
			return new SqlFunctionCallExpression(functionName, args);
		}

		public static SqlFunctionCallExpression FunctionCall(string functionName, InvokeArgument[] args) {
			return FunctionCall(ObjectName.Parse(functionName), args);
		}

		public static SqlFunctionCallExpression FunctionCall(ObjectName functionName) {
			return FunctionCall(functionName, new InvokeArgument[0]);
		}

		public static SqlFunctionCallExpression FunctionCall(ObjectName functionName, SqlExpression[] args) {
			var invokeArgs = args != null && args.Length > 0 ? args.Select(x => new InvokeArgument(x)).ToArray() : new InvokeArgument[0];
			return FunctionCall(functionName, invokeArgs);
		}

		public static SqlFunctionCallExpression FunctionCall(string functionName) {
			return FunctionCall(functionName, new SqlExpression[0]);
		}

		public static SqlFunctionCallExpression FunctionCall(string functionName, SqlExpression[] args) {
			return FunctionCall(ObjectName.Parse(functionName), args);
		}

		public static SqlReferenceExpression Reference(ObjectName objectName) {
			return new SqlReferenceExpression(objectName);
		}

		public static SqlVariableReferenceExpression VariableReference(string varName) {
			return new SqlVariableReferenceExpression(varName);
		}
 
		#endregion

		public static SqlConditionalExpression Conditional(SqlExpression testExpression, SqlExpression ifTrue) {
			return Conditional(testExpression, ifTrue, null);
		}

		public static SqlConditionalExpression Conditional(SqlExpression testExpression, SqlExpression ifTrue, SqlExpression ifFalse) {
			return new SqlConditionalExpression(testExpression, ifTrue, ifFalse);
		}

		#region Binary Expressions

		public static SqlBinaryExpression Binary(SqlExpression left, SqlExpressionType expressionType, SqlExpression right) {
			if (expressionType == SqlExpressionType.Add)
				return Add(left, right);
			if (expressionType == SqlExpressionType.Subtract)
				return Subtract(left, right);
			if (expressionType == SqlExpressionType.Multiply)
				return Multiply(left, right);
			if (expressionType == SqlExpressionType.Divide)
				return Divide(left, right);
			if (expressionType == SqlExpressionType.Modulo)
				return Modulo(left, right);

			if (expressionType == SqlExpressionType.Equal)
				return Equal(left, right);
			if (expressionType == SqlExpressionType.NotEqual)
				return NotEqual(left, right);
			if (expressionType == SqlExpressionType.Is)
				return Is(left, right);
			if (expressionType == SqlExpressionType.IsNot)
				return IsNot(left, right);
			if (expressionType == SqlExpressionType.GreaterThan)
				return GreaterThan(left, right);
			if (expressionType == SqlExpressionType.GreaterOrEqualThan)
				return GreaterOrEqualThan(left, right);
			if (expressionType == SqlExpressionType.SmallerThan)
				return SmallerThan(left, right);
			if (expressionType == SqlExpressionType.SmallerOrEqualThan)
				return SmallerOrEqualThan(left, right);

			if (expressionType == SqlExpressionType.Like)
				return Like(left, right);
			if (expressionType == SqlExpressionType.NotLike)
				return NotLike(left, right);

			if (expressionType == SqlExpressionType.And)
				return And(left, right);
			if (expressionType == SqlExpressionType.Or)
				return Or(left, right);
			if (expressionType == SqlExpressionType.XOr)
				return XOr(left, right);

			throw new ArgumentException(String.Format("Expression type {0} is not a Binary", expressionType));
		}

		public static SqlBinaryExpression Equal(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.Equal, right);
		}

		public static SqlBinaryExpression NotEqual(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.NotEqual, right);
		}

		public static SqlBinaryExpression Is(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.Is, right);
		}

		public static SqlBinaryExpression IsNot(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.IsNot, right);
		}

		public static SqlBinaryExpression SmallerOrEqualThan(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.SmallerOrEqualThan, right);
		}

		public static SqlBinaryExpression GreaterOrEqualThan(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.GreaterOrEqualThan, right);
		}

		public static SqlBinaryExpression SmallerThan(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.SmallerThan, right);
		}

		public static SqlBinaryExpression GreaterThan(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.GreaterThan, right);
		}

		public static SqlBinaryExpression Like(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.Like, right);
		}

		public static SqlBinaryExpression NotLike(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.NotLike, right);
		}

		public static SqlBinaryExpression And(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.And, right);
		}

		public static SqlBinaryExpression Or(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.Or, right);
		}

		public static SqlBinaryExpression XOr(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.XOr, right);
		}

		public static SqlBinaryExpression Add(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.Add, right);
		}

		public static SqlBinaryExpression Subtract(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.Subtract, right);
		}

		public static SqlBinaryExpression Multiply(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.Multiply, right);
		}

		public static SqlBinaryExpression Divide(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.Divide, right);
		}

		public static SqlBinaryExpression Modulo(SqlExpression left, SqlExpression right) {
			return new SqlBinaryExpression(left, SqlExpressionType.Modulo, right);
		}

		public static SqlQuantifiedExpression Quantified(SqlExpressionType expressionType, SqlExpression value) {
			if (expressionType != SqlExpressionType.Any &&
				expressionType != SqlExpressionType.All)
				throw new ArgumentException("Invalid quantified expression type.");

			return new SqlQuantifiedExpression(expressionType, value);
		}

		public static SqlQuantifiedExpression Any(SqlExpression value) {
			return Quantified(SqlExpressionType.Any, value);
		}

		public static SqlQuantifiedExpression All(SqlExpression value) {
			return Quantified(SqlExpressionType.All, value);
		}

		#endregion

		#region Unary Expressions

		public static SqlUnaryExpression Unary(SqlExpressionType expressionType, SqlExpression operand) {
			if (expressionType == SqlExpressionType.UnaryPlus)
				return UnaryPlus(operand);
			if (expressionType == SqlExpressionType.Negate)
				return Negate(operand);
			if (expressionType == SqlExpressionType.Not)
				return Not(operand);

			throw new ArgumentException(String.Format("Expression Type {0} is not an Unary.", expressionType));
		}

		public static SqlUnaryExpression Not(SqlExpression operand) {
			return new SqlUnaryExpression(SqlExpressionType.Not, operand);
		}

		public static SqlUnaryExpression Negate(SqlExpression operand) {
			return new SqlUnaryExpression(SqlExpressionType.Negate, operand);
		}

		public static SqlUnaryExpression UnaryPlus(SqlExpression operand) {
			return new SqlUnaryExpression(SqlExpressionType.UnaryPlus, operand);
		}

		#endregion

		public static SqlAssignExpression Assign(SqlExpression reference, SqlExpression valueExpression) {
			return new SqlAssignExpression(reference, valueExpression);
		}


		public static SqlTupleExpression Tuple(SqlExpression[] expressions) {
			return new SqlTupleExpression(expressions);
		}

		public static SqlTupleExpression Tuple(SqlExpression expr1, SqlExpression exp2) {
			return Tuple(new[] {expr1, exp2});
		}

		public static SqlTupleExpression Tuple(SqlExpression expr1, SqlExpression expr2, SqlExpression expr3) {
			return Tuple(new[] {expr1, expr2, expr3});
		}

		#endregion

		internal static void Serialize(SqlExpression expression, BinaryWriter writer) {
			var serializer = new BinarySerializer();
			serializer.Serialize(writer, expression);
		}

		internal static SqlExpression Deserialize(BinaryReader reader) {
			var serializer = new BinarySerializer();
			return (SqlExpression) serializer.Deserialize(reader);
		}

		#region ExpressionParser

		class ExpressionParser : IExpressionParser {
			public ExpressionParseResult Parse(string s) {
				return new PlSqlCompiler().ParseExpression(s);
			}
		}

		#endregion
	}
}