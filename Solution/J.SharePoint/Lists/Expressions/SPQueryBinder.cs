using J.SharePoint.Lists.Attributes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace J.SharePoint.Lists.Expressions
{
    internal class SPQueryBinder : ExpressionVisitor
    {
        public Expression Bind(Expression expression)
        {
            return Visit(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Expression source = Visit(node.Arguments[0]);
            Expression exp = null;
            if( node.Arguments.Count > 1 )
                exp = StripQuotes(node.Arguments[1]);
            switch (node.Method.Name)
            {
                case "OrderBy":
                case "ThenBy":
                    return new OrderByExpression(source, Visit((LambdaExpression)exp), OrderByType.Ascending);
                case "OrderByDescending":
                case "ThenByDescending":
                    return new OrderByExpression(source, Visit((LambdaExpression)exp), OrderByType.Descending);
                case "Where":
                    return new WhereExpression(source, (new WhereBinder()).Bind((LambdaExpression)exp));
                case "Take":
                    return new RowLimitExpression(source, (ConstantExpression)exp);
                case "ViewScope":
                    return new ViewScopeExpression(source, (ConstantExpression)exp);
                case "Folder":
                    return new FolderExpression(source, (ConstantExpression)exp);
                default:
                    throw new NotSupportedException();
            }
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return Visit(node.Body);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            SPFieldMetadata md = SPFieldMetadata.Get((PropertyInfo)node.Member);
            return Expression.Constant(md.InternalName);
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }
    }

    internal class WhereBinder : ExpressionVisitor
    {
        private bool _withinExpression;

        public Expression Bind(Expression expression)
        {
            return Visit(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            _withinExpression = true;
            CamlComparisonExpression comparison = null;
            switch (node.Method.Name)
            {
                case "Contains":
                    comparison = new CamlComparisonExpression(SPQueryNodeType.Contains, Visit(node.Object), Visit(node.Arguments[0]));
                    break;
                case "Equals":
                    comparison = new CamlComparisonExpression(SPQueryNodeType.Equals, Visit(node.Object), Visit(node.Arguments[0]));
                    break;
                case "QueryEq":
                    comparison = new CamlComparisonExpression(SPQueryNodeType.Equals, Visit(node.Arguments[0]), Visit(node.Arguments[1]), (bool)((ConstantExpression)node.Arguments[2]).Value);
                    break;
                case "QueryLt":
                    comparison = new CamlComparisonExpression(SPQueryNodeType.LessThan, Visit(node.Arguments[0]), Visit(node.Arguments[1]), (bool)((ConstantExpression)node.Arguments[2]).Value);
                    break;
                case "QueryGt":
                    comparison = new CamlComparisonExpression(SPQueryNodeType.GreaterThan, Visit(node.Arguments[0]), Visit(node.Arguments[1]), (bool)((ConstantExpression)node.Arguments[2]).Value);
                    break;
                case "QueryLeq":
                    comparison = new CamlComparisonExpression(SPQueryNodeType.LessThanOrEqual, Visit(node.Arguments[0]), Visit(node.Arguments[1]), (bool)((ConstantExpression)node.Arguments[2]).Value);
                    break;
                case "QueryGeq":
                    comparison = new CamlComparisonExpression(SPQueryNodeType.GreaterThanOrEqual, Visit(node.Arguments[0]), Visit(node.Arguments[1]), (bool)((ConstantExpression)node.Arguments[2]).Value);
                    break;
                default:
                    throw new NotSupportedException();
            }
            _withinExpression = false;
            return comparison;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return BindAndOr(node, SPQueryNodeType.And);
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return BindAndOr(node, SPQueryNodeType.Or);
                case ExpressionType.Equal:
                    return BindComparison(node, SPQueryNodeType.Equals);
                case ExpressionType.GreaterThan:
                    return BindComparison(node, SPQueryNodeType.GreaterThan);
                case ExpressionType.LessThan:
                    return BindComparison(node, SPQueryNodeType.LessThan);
                case ExpressionType.LessThanOrEqual:
                    return BindComparison(node, SPQueryNodeType.LessThanOrEqual);
                case ExpressionType.GreaterThanOrEqual:
                    return BindComparison(node, SPQueryNodeType.GreaterThanOrEqual);
                default:
                    throw new NotSupportedException();
            }
        }

        private Expression BindAndOr(BinaryExpression node, SPQueryNodeType nodeType)
        {
            Expression left = Visit(node.Left);
            Expression right = Visit(node.Right);
            return new CamlAndOrExpression(nodeType, left, right);
        }

        private Expression BindComparison(BinaryExpression node, SPQueryNodeType nodeType)
        {
            _withinExpression = true;
            Expression left = Visit(node.Left);
            Expression right = Visit(node.Right);
            _withinExpression = false;
            return new CamlComparisonExpression(nodeType, left, right);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not && node.Operand.Type.Equals(typeof(bool)))
            {
                _withinExpression = true;
                CamlComparisonExpression exp = new CamlComparisonExpression(SPQueryNodeType.Equals, Visit(node.Operand), new CamlValueExpression(false, node.Type));
                _withinExpression = false;
                return exp;
            }
            throw new NotSupportedException();
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            return new CamlValueExpression(node.Value, node.Type);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            SPFieldMetadata md = SPFieldMetadata.Get((PropertyInfo)node.Member);
            CamlFieldExpression fieldExp = new CamlFieldExpression(md.InternalName, md.Type);
            if (!_withinExpression)
            {
                if (node.Type.Equals(typeof(bool)))
                {
                    return new CamlComparisonExpression(SPQueryNodeType.Equals, fieldExp, new CamlValueExpression(true, node.Type));
                }

                throw new NotSupportedException();
            }

            return fieldExp;
        }
    }
}
