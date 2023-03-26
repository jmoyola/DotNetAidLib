using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Database.DAO.Core
{
    public class DaoLoadInfo
    {
        public DaoLoadInfo(DaoLoadInfo daoLoadInfo)
        {
            LoadType = daoLoadInfo.LoadType;
            ChildrenNavigators = daoLoadInfo.ChildrenNavigators;
        }

        public DaoLoadInfo(DaoEntityLoadType loadType, string[] childrenNavigators = null)
        {
            LoadType = loadType;
            ChildrenNavigators = childrenNavigators;
        }

        public DaoEntityLoadType LoadType { get; set; }

        public string[] ChildrenNavigators { get; set; }

        public override string ToString()
        {
            return LoadType + (ChildrenNavigators == null ? "" : ChildrenNavigators.ToStringJoin(", "));
        }
    }
}