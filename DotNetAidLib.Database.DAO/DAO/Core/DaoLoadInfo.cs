using System;
using System.Collections;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Database.DAO.Core
{
    public class DaoLoadInfo
    {
        private DaoEntityLoadType loadType;
        private String [] childrenNavigators = null;

        public DaoLoadInfo (DaoLoadInfo daoLoadInfo) {
            this.loadType =daoLoadInfo.LoadType;
            this.childrenNavigators =daoLoadInfo.ChildrenNavigators;
        }
        public DaoLoadInfo (DaoEntityLoadType loadType, String [] childrenNavigators = null)
        {
            this.loadType = loadType;
            this.childrenNavigators = childrenNavigators;
        }

        public DaoEntityLoadType LoadType { get => loadType; set => loadType = value; }
        public string [] ChildrenNavigators { get => childrenNavigators; set => childrenNavigators = value; }

        public override string ToString ()
        {
            return this.loadType.ToString () + (this.childrenNavigators == null ? "" : childrenNavigators.ToStringJoin(", "));
        }
    }
}
