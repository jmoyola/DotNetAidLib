using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database.DAO.Core
{

    public class DaoInfo:DaoLoadInfo
    {
        private DaoSession session;

        public DaoInfo (DaoSession session)
            : this (session, new DaoLoadInfo(session.Context.DefaultEntityLoadType)) {}

        public DaoInfo(DaoSession session, DaoLoadInfo daoLoadInfo)
            :base(daoLoadInfo) 
        {
            Assert.NotNull( session, nameof(session));

            this.session = session;
        }

        public DaoSession Session { get => session; set => session=value; }
    }

    public interface IDaoInstance
    {
        DaoInfo __DaoInfo { get;}
    }

    public class DaoInstance : IDaoInstance
    {
        private DaoInfo daoInfo;

        public DaoInstance(DaoSession daoSession)
        {
            this.daoInfo = new DaoInfo(daoSession);
        }

        public DaoInfo __DaoInfo { get => daoInfo;}
    }
}
