using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database.DAO.Core
{
    public class DaoInfo : DaoLoadInfo
    {
        public DaoInfo(DaoSession session)
            : this(session, new DaoLoadInfo(session.Context.DefaultEntityLoadType))
        {
        }

        public DaoInfo(DaoSession session, DaoLoadInfo daoLoadInfo)
            : base(daoLoadInfo)
        {
            Assert.NotNull(session, nameof(session));

            Session = session;
        }

        public DaoSession Session { get; set; }
    }

    public interface IDaoInstance
    {
        DaoInfo __DaoInfo { get; }
    }

    public class DaoInstance : IDaoInstance
    {
        public DaoInstance(DaoSession daoSession)
        {
            __DaoInfo = new DaoInfo(daoSession);
        }

        public DaoInfo __DaoInfo { get; }
    }
}