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
    internal class SPQueryTranslator : ExpressionVisitor
    {
        private static Dictionary<SPQueryNodeType, string> SPQueryNodeTypeMapping = new Dictionary<SPQueryNodeType, string> 
        {
            { SPQueryNodeType.And, "And" },
            { SPQueryNodeType.Or, "Or" },
            { SPQueryNodeType.Contains, "Contains" },
            { SPQueryNodeType.Equals, "Eq" },
            { SPQueryNodeType.NotEqual, "Neq" },
            { SPQueryNodeType.GreaterThan, "Gt|Lt" },
            { SPQueryNodeType.LessThan, "Lt|Gt" },
            { SPQueryNodeType.GreaterThanOrEqual, "Geq|Leq" },
            { SPQueryNodeType.LessThanOrEqual, "Leq|Geq" }
        };

        private List<string> _orderByXml;
        private StringBuilder _whereXml;
        private uint? _rowLimit;
        private SPViewScope? _viewScope;
        private SPFolder _folder;

        public SPQueryTranslator()
        {
            _orderByXml = new List<string>();
            _whereXml = new StringBuilder();
        }

        public SPQuery Translate(Expression expression)
        {
            Expression queryExpression = (new SPQueryBinder()).Bind(expression);
            Visit(queryExpression);
            _whereXml.AppendFormat("<OrderBy>{0}</OrderBy>", string.Join(string.Empty, _orderByXml.ToArray()));
            SPQuery query = new SPQuery { Query = _whereXml.ToString() };
            if (_rowLimit.HasValue)
                query.RowLimit = _rowLimit.Value;
            if (_viewScope.HasValue)
                query.ViewAttributes = string.Format("Scope=\"{0}\"", _viewScope.Value);
            if (_folder != null)
                query.Folder = _folder;
            return query;
        }

        public override Expression Visit(Expression node)
        {
            if (node == null) return null;
            switch((SPQueryExpressionType)node.NodeType)
            {
                case SPQueryExpressionType.Where:
                    return VisitWhere((WhereExpression)node);
                case SPQueryExpressionType.OrderBy:
                    return VisitOrderBy((OrderByExpression)node);
                case SPQueryExpressionType.CamlAndOr:
                    return VisitCamlAndOr((CamlAndOrExpression)node);
                case SPQueryExpressionType.CamlComparison:
                    return VisitCamlComparison((CamlComparisonExpression)node);
                case SPQueryExpressionType.RowLimit:
                    return VisitRowLimit((RowLimitExpression)node);
                case SPQueryExpressionType.ViewScope:
                    return VisitViewScope((ViewScopeExpression)node);
                case SPQueryExpressionType.Folder:
                    return VisitFolder((FolderExpression)node);
                default:
                    return base.Visit(node);
            }
        }

        public virtual Expression VisitWhere(WhereExpression node)
        {
            Visit(node.Source);
            _whereXml.Append("<Where>");
            Visit(node.Expression);
            _whereXml.Append("</Where>");
            return node;
        }

        public virtual Expression VisitCamlAndOr(CamlAndOrExpression node)
        {
            _whereXml.AppendFormat("<{0}>", node.SPNodeType.ToString());
            Visit(node.First);
            Visit(node.Second);
            _whereXml.AppendFormat("</{0}>", node.SPNodeType.ToString());
            return node;
        }

        public virtual Expression VisitCamlComparison(CamlComparisonExpression node)
        {
            string[] parts = SPQueryNodeTypeMapping[node.SPNodeType].Split('|');
            string comparison = parts[0];

            CamlValueExpression valueExp;
            CamlFieldExpression fieldExp;
            if( node.Value is CamlValueExpression )
            {
                valueExp = (CamlValueExpression)node.Value;
                fieldExp = (CamlFieldExpression)node.Field;
            }
            else
            {
                valueExp = (CamlValueExpression)node.Field;
                fieldExp = (CamlFieldExpression)node.Value;
                if( parts.Length > 1 )
                    comparison = parts[1];
            }

            _whereXml.AppendFormat("<{0}>", comparison);
            _whereXml.AppendFormat("<FieldRef Name='{0}'/>", fieldExp.Name);
            _whereXml.AppendFormat("<Value Type='{0}'{1}>{2}</Value>", fieldExp.FieldType.ToString(),
                node.IncludeTimeValue ? " IncludeTimeValue='TRUE'" : string.Empty,
                valueExp.ValueString);
            _whereXml.AppendFormat("</{0}>", comparison);
            return node;
        }

        public virtual Expression VisitOrderBy(OrderByExpression node)
        {
            Visit(node.Source);
            if (node.FieldName is ConstantExpression)
            {
                _orderByXml.Add(string.Format("<FieldRef Name='{0}'{1} />", ((ConstantExpression)node.FieldName).Value.ToString(), node.OrderType == OrderByType.Descending ? "Ascending='FALSE'" : string.Empty));
                return node;
            }
            throw new NotSupportedException();
        }

        public virtual Expression VisitRowLimit(RowLimitExpression node)
        {
            Visit(node.Source);
            _rowLimit = (uint)node.RowLimit;
            return node;
        }

        public virtual Expression VisitViewScope(ViewScopeExpression node)
        {
            Visit(node.Source);
            _viewScope = node.Scope;
            return node;
        }

        public virtual Expression VisitFolder(FolderExpression node)
        {
            Visit(node.Source);
            _folder = node.Folder;
            return node;
        }
    }
}
