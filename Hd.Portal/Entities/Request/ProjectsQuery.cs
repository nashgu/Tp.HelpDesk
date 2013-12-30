using Hd.QueryExtensions;

namespace Hd.Portal
{
    public class ProjectsQuery : BusinessQuery
    {
        public override SelectQuery InitialQuery
        {
            get
            {
                var query = RequestQueryFactory.CreateQuery().InitialQuery;
                query.Columns = new SelectColumnCollection(new SelectColumn[] { new SelectColumn("Project.Name") });
                query.Distinct = true;
                return query;
            }
        }
    }
}