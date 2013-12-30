﻿//
// Copyright (c) 2005-2009 TargetProcess. All rights reserved.
// TargetProcess proprietary/confidential. Use is subject to license terms. Redistribution of this file is strictly forbidden.
//
using Hd.QueryExtensions;

namespace Hd.Portal
{
    internal class GlobalRequestQuery : BusinessQuery
    {
        public override SelectQuery InitialQuery
        {
            get
            {
                var selectQuery = new SelectQuery(typeof(Request));
                selectQuery.OrderByTerms.Clear();
                selectQuery.AddOrderBy("CreateDate", OrderByDirection.Descending);

                selectQuery.AddCompare("ParentProject.IsProduct", new Parameter(true), CompareOperator.Equal);
                if (Requester.LoggedUserID == null)
                    throw new AccessDeniedException();
                int requesterID = Requester.LoggedUserID.Value;
                WhereTerm term = WhereTerm.CreateIn(SqlExpression.Field("RequestID"),
                                                    string.Format(@"select r.RequestID from Request as r where (r.ParentProject.Company.CompanyID is null or (r.ParentProject.Company.CompanyID in ( select rq.Company.CompanyID from Requester as rq where rq.UserID = {0} ) ) )
														and r.IsPrivate = 0 ", requesterID));

                selectQuery.WherePhrase.Terms.Add(term);
                return selectQuery;
            }
        }
    }
}