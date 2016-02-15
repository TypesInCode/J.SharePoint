using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace J.SharePoint.Lists.Expressions
{
    internal enum SPQueryExpressionType
    {
        Where = 1000, // make sure these don't overlap with ExpressionType
        OrderBy,
        CamlAndOr,
        CamlComparison,
        CamlField,
        CamlValue,
        RowLimit,
        ViewScope,
        Folder
    }

    internal enum SPQueryNodeType
    {
        And,
        Or,
        Contains,
        Equals,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual
    }

    internal enum OrderByType
    {
        Ascending,
        Descending
    }

    internal class WhereExpression : Expression
    {
        public WhereExpression(Expression source, Expression expression)
        {
            Expression = expression;
        }

        public override ExpressionType NodeType
        { get { return (ExpressionType)SPQueryExpressionType.Where; } }

        public override Type Type
        { get { return typeof(bool); } }

        public Expression Source
        { get; private set; }

        public Expression Expression
        { get; private set; }
    }

    internal class CamlAndOrExpression : Expression
    {
        public override ExpressionType NodeType
        { get { return (ExpressionType)SPQueryExpressionType.CamlAndOr; } }

        public override Type Type
        { get { return typeof(bool); } }

        public Expression First
        { get; private set; }

        public Expression Second
        { get; private set; }

        public SPQueryNodeType SPNodeType
        { get; private set; }

        public CamlAndOrExpression(SPQueryNodeType spNodeType, Expression first, Expression second)
        {
            First = first;
            Second = second;
            SPNodeType = spNodeType;
        }
    }

    internal class CamlComparisonExpression : Expression
    {
        public override ExpressionType NodeType
        { get { return (ExpressionType)SPQueryExpressionType.CamlComparison; } }

        public override Type Type
        { get { return typeof(bool); } }

        public SPQueryNodeType SPNodeType
        { get; private set; }

        public bool IncludeTimeValue
        { get; private set; }

        public Expression Field
        { get; private set; }

        public Expression Value
        { get; private set; }

        public CamlComparisonExpression(SPQueryNodeType spNodeType, Expression field, Expression value)
        {
            SPNodeType = spNodeType;
            Field = field;
            Value = value;
        }

        public CamlComparisonExpression(SPQueryNodeType spNodeType, Expression field, Expression value, bool includeTimeValue)
            : this(spNodeType, field, value)
        {
            IncludeTimeValue = includeTimeValue;
        }
    }

    internal class CamlValueExpression : Expression
    {
        public override ExpressionType NodeType
        { get { return (ExpressionType)SPQueryExpressionType.CamlValue; } }

        public override Type Type
        { get { return typeof(string); } }

        public object Value
        { get; private set; }

        public Type ValueType
        { get; private set; }

        public string ValueString
        {
            get
            {
                if (ValueType.Equals(typeof(bool)))
                    return (bool)Value ? "1" : "0";
                if (ValueType.Equals(typeof(DateTime)))
                    return SPUtility.CreateISO8601DateTimeFromSystemDateTime((DateTime)Value);

                return Value.ToString();
            }
        }

        public CamlValueExpression(object value, Type valueType)
        {
            Value = value;
            ValueType = valueType;
        }
    }

    internal class CamlFieldExpression : Expression
    {
        public override ExpressionType NodeType
        { get { return (ExpressionType)SPQueryExpressionType.CamlField; } }

        public override Type Type
        { get { return typeof(string); } }

        public string Name
        { get; private set; }

        public SPFieldType FieldType
        { get; private set; }

        public CamlFieldExpression(string name, SPFieldType type)
        {
            Name = name;
            FieldType = type;
        }
    }

    internal class OrderByExpression : Expression
    {
        public OrderByExpression(Expression source, Expression fieldName, OrderByType orderType)
        {
            Source = source;
            FieldName = fieldName;
            OrderType = orderType;
        }

        public override ExpressionType NodeType
        { get { return (ExpressionType)SPQueryExpressionType.OrderBy; } }

        public override Type Type
        { get { return typeof(string); } }

        public Expression Source
        { get; private set; }

        public Expression FieldName
        { get; private set; }

        public OrderByType OrderType
        { get; private set; }
    }

    internal class RowLimitExpression : Expression
    {
        public override ExpressionType NodeType
        { get { return (ExpressionType)SPQueryExpressionType.RowLimit; } }

        public override Type Type
        { get { return typeof(int); } }

        public Expression Source
        { get; private set; }

        public int RowLimit
        { get; private set; }

        public RowLimitExpression(Expression source, ConstantExpression expression)
        {
            Source = source;
            RowLimit = (int)expression.Value;
        }
    }

    internal class ViewScopeExpression : Expression
    {
        public override ExpressionType NodeType
        { get { return (ExpressionType)SPQueryExpressionType.ViewScope; } }

        public Expression Source
        { get; private set; }

        public SPViewScope Scope
        { get; private set; }

        public ViewScopeExpression(Expression source, ConstantExpression expression)
        {
            Source = source;
            Scope = (SPViewScope)expression.Value;
        }
    }

    internal class FolderExpression : Expression
    {
        public override ExpressionType NodeType
        { get { return (ExpressionType)SPQueryExpressionType.Folder; } }

        public Expression Source
        { get; private set; }

        public SPFolder Folder
        { get; private set; }

        public FolderExpression(Expression source, ConstantExpression expression)
        {
            Source = source;
            Folder = (SPFolder)expression.Value;
        }
    }
}
