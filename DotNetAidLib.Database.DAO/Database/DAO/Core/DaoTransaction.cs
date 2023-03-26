using System;
using System.Data;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database.DAO.Core
{
    public class DaoTransaction : IDisposable
    {
        private readonly IDbTransaction dbTransaction;

        private bool disposed;

        public DaoTransaction(DaoSession daoSession, IDbTransaction dbTransaction)
        {
            Assert.NotNull(daoSession, nameof(daoSession));
            Assert.NotNull(dbTransaction, nameof(dbTransaction));

            Session = daoSession;
            this.dbTransaction = dbTransaction;
        }

        public DaoSession Session { get; }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Rollback()
        {
            dbTransaction.Rollback();
            Dispose();
        }

        public void Commit()
        {
            dbTransaction.Commit();
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing) dbTransaction.Dispose();

                disposed = true;
            }
        }
    }
}