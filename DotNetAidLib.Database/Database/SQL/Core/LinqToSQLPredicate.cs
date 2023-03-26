using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DotNetAidLib.Database.SQL.Core
{
    public static class Evaluator
    {
	    /// <summary>
	    ///     Performs evaluation & replacement of independent sub-trees
	    /// </summary>
	    /// <param name="expression">The root of the expression tree.</param>
	    /// <param name="fnCanBeEvaluated">
	    ///     A function that decides whether a given expression node can be part of the local
	    ///     function.
	    /// </param>
	    /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
	    public static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
        {
            return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
        }

	    /// <summary>
	    ///     Performs evaluation & replacement of independent sub-trees
	    /// </summary>
	    /// <param name="expression">The root of the expression tree.</param>
	    /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
	    public static Expression PartialEval(Expression expression)
        {
            return PartialEval(expression, CanBeEvaluatedLocally);
        }

        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter;
        }

        /// <summary>
        ///     Evaluates & replaces sub-trees when first candidate is reached (top-down)
        /// </summary>
        private class SubtreeEvaluator : ExpressionVisitor
        {
            private readonly HashSet<Expression> candidates;

            internal SubtreeEvaluator(HashSet<Expression> candidates)
            {
                this.candidates = candidates;
            }

            internal Expression Eval(Expression exp)
            {
                return Visit(exp);
            }

            public override Expression Visit(Expression exp)
            {
                if (exp == null) return null;

                if (candidates.Contains(exp)) return Evaluate(exp);

                return base.Visit(exp);
            }

            private Expression Evaluate(Expression e)
            {
                if (e.NodeType == ExpressionType.Constant) return e;

                var lambda = Expression.Lambda(e);
                var fn = lambda.Compile();
                return Expression.Constant(fn.DynamicInvoke(null), e.Type);
            }
        }

        /// <summary>
        ///     Performs bottom-up analysis to determine which nodes can possibly
        ///     be part of an evaluated sub-tree.
        /// </summary>
        private class Nominator : ExpressionVisitor
        {
            private HashSet<Expression> candidates;
            private bool cannotBeEvaluated;
            private readonly Func<Expression, bool> fnCanBeEvaluated;

            internal Nominator(Func<Expression, bool> fnCanBeEvaluated)
            {
                this.fnCanBeEvaluated = fnCanBeEvaluated;
            }

            internal HashSet<Expression> Nominate(Expression expression)
            {
                candidates = new HashSet<Expression>();
                Visit(expression);
                return candidates;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    var saveCannotBeEvaluated = cannotBeEvaluated;
                    cannotBeEvaluated = false;
                    base.Visit(expression);
                    if (!cannotBeEvaluated)
                    {
                        if (fnCanBeEvaluated(expression))
                            candidates.Add(expression);
                        else
                            cannotBeEvaluated = true;
                    }

                    cannotBeEvaluated |= saveCannotBeEvaluated;
                }

                return expression;
            }
        }
    }

    public class LinqToSQLPredicate : ExpressionVisitor
    {
        private readonly Func<MemberInfo, string> _MemberParser;
        private readonly bool assignationExpression;
        private StringBuilder sb;

        public LinqToSQLPredicate()
        {
        }

        public LinqToSQLPredicate(Func<MemberInfo, string> memberParser, bool assignationExpression = false)
        {
            _MemberParser = memberParser;
            this.assignationExpression = assignationExpression;
        }

        public bool Count { get; private set; }

        public int? Skip { get; private set; }

        public int? Take { get; private set; }

        public string OrderBy { get; private set; } = string.Empty;

        public string WhereClause { get; private set; } = string.Empty;

        public string TranslateLambda<T>(Expression<Func<T, bool>> predicate)
        {
            return TranslateExpression(predicate);
        }

        public string TranslateLambda<T, S>(Expression<Func<T, S>> predicate)
        {
            return TranslateExpression(predicate);
        }

        public string TranslateLambda<T>(Expression<Action<T>> predicate)
        {
            return TranslateExpression(predicate);
        }

        public string TranslateExpression(Expression expression)
        {
            sb = new StringBuilder();
            Visit(expression);
            WhereClause = sb.ToString();
            return WhereClause;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote) e = ((UnaryExpression) e).Operand;
            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                Visit(m.Arguments[0]);
                var lambda = (LambdaExpression) StripQuotes(m.Arguments[1]);
                Visit(lambda.Body);
                return m;
            }

            if (m.Method.Name == "Count")
            {
                if (ParseCountExpression(m))
                {
                    var nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "Take")
            {
                if (ParseTakeExpression(m))
                {
                    var nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "Skip")
            {
                if (ParseSkipExpression(m))
                {
                    var nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "OrderBy")
            {
                if (ParseOrderByExpression(m, "ASC"))
                {
                    var nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "OrderByDescending")
            {
                if (ParseOrderByExpression(m, "DESC"))
                {
                    var nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            else
            {
                sb.Append(Expression.Lambda(m).Compile().DynamicInvoke().ToSQLValue());
            }

            return m;
            //throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    sb.Append(" NOT ");
                    Visit(u.Operand);
                    break;
                case ExpressionType.Convert:
                    Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported",
                        u.NodeType));
            }

            return u;
        }

        /// <summary>
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression b)
        {
            sb.Append("(");
            Visit(b.Left);

            switch (b.NodeType)
            {
                case ExpressionType.And:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.AndAlso:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.Or:
                    sb.Append(" OR ");
                    break;

                case ExpressionType.OrElse:
                    sb.Append(" OR ");
                    break;

                case ExpressionType.Equal:
                    if (!assignationExpression && IsNullConstant(b.Right))
                        sb.Append(" IS ");
                    else
                        sb.Append(" = ");
                    break;

                case ExpressionType.NotEqual:
                    if (IsNullConstant(b.Right))
                        sb.Append(" IS NOT ");
                    else
                        sb.Append(" <> ");
                    break;

                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;

                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;

                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported",
                        b.NodeType));
            }

            Visit(b.Right);
            sb.Append(")");
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            var q = c.Value as IQueryable;

            if (q == null) sb.Append(c.Value.ToSQLValue());

            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null)
            {
                if (m.Expression.NodeType == ExpressionType.Parameter)
                {
                    if (_MemberParser == null)
                        sb.Append(m.Member.Name);
                    else
                        sb.Append(_MemberParser.Invoke(m.Member));
                }
                else
                {
                    sb.Append(Expression.Lambda(m).Compile().DynamicInvoke().ToSQLValue());
                }
            }

            return m;
        }

        protected bool IsNullConstant(Expression exp)
        {
            return exp.NodeType == ExpressionType.Constant && ((ConstantExpression) exp).Value == null;
        }

        private bool ParseOrderByExpression(MethodCallExpression expression, string order)
        {
            var unary = (UnaryExpression) expression.Arguments[1];
            var lambdaExpression = (LambdaExpression) unary.Operand;

            lambdaExpression = (LambdaExpression) Evaluator.PartialEval(lambdaExpression);

            var body = lambdaExpression.Body as MemberExpression;
            if (body != null)
            {
                if (string.IsNullOrEmpty(OrderBy))
                    OrderBy = string.Format("{0} {1}", body.Member.Name, order);
                else
                    OrderBy = string.Format("{0}, {1} {2}", OrderBy, body.Member.Name, order);

                return true;
            }

            return false;
        }

        private bool ParseTakeExpression(MethodCallExpression expression)
        {
            var sizeExpression = (ConstantExpression) expression.Arguments[1];

            int size;
            if (int.TryParse(sizeExpression.Value.ToString(), out size))
            {
                Take = size;
                return true;
            }

            return false;
        }

        private bool ParseSkipExpression(MethodCallExpression expression)
        {
            var sizeExpression = (ConstantExpression) expression.Arguments[1];

            int size;
            if (int.TryParse(sizeExpression.Value.ToString(), out size))
            {
                Skip = size;
                return true;
            }

            return false;
        }

        private bool ParseCountExpression(MethodCallExpression expression)
        {
            Count = true;
            return true;
        }
    }
}