using System;
using Hd.Portal.Components;
using Hd.QueryExtensions;

namespace Hd.Portal
{
    [Serializable]
    public class SearchFilter
    {
        private string _searchPattern;

        public string SearchPattern
        {
            get { return _searchPattern; }
            set
            {
                if (value != null)
                {
                    value = value.Trim();
                }

                if (value == string.Empty)
                {
                    value = null;
                }

                _searchPattern = value;
            }
        }

        public void AppendSimpleSearchPattern(SelectQuery selectQuery, string properties, CompareOperator compareOperator = CompareOperator.Like)
        {
            if (string.IsNullOrEmpty(properties)) return;
            var propArr = properties.Split(',');
            var group = new WhereClause(WhereClauseRelationship.Or);
            string[] words = StringUtils.SmartSplit(SearchPattern);
            foreach (string property in propArr)
            {
                var clause = new WhereClause(WhereClauseRelationship.And);
                AppendExpression(words, selectQuery, clause, property, compareOperator);
                group.SubClauses.Add(clause);
            }
            selectQuery.WherePhrase.SubClauses.Add(group);
        }

        public void AppendSimpleSearchPattern(SelectQuery selectQuery)
        {
            AppendSimpleSearchPattern(selectQuery, "Name,Description");
        }

        private void AppendExpression(string[] words, SelectQuery selectQuery, WhereClause group, string columnName, CompareOperator compareOperator)
        {
            foreach (string word in words)
            {
                if (word.Length == 0)
                {
                    continue;
                }
                var paramString = compareOperator == CompareOperator.Like
                    ? "%" + word + "%"
                    : word;
                var parameter = new Parameter(paramString);
                group.Terms.Add(
                    WhereTerm.CreateCompare(SqlExpression.Field(columnName, selectQuery.FromClause.BaseTable),
                                            SqlExpression.Parameter(), compareOperator));

                selectQuery.Parameters.Add(parameter);
            }
        }
    }
}