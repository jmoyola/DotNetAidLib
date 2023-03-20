using System;
using System.Data;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database.DAO.Core
{
    public class DaoTransaction:IDisposable
    {
        private IDbTransaction dbTransaction;
        private DaoSession daoSession;

        public DaoTransaction(DaoSession daoSession, IDbTransaction dbTransaction)
        {
            Assert.NotNull( daoSession, nameof(daoSession));
            Assert.NotNull( dbTransaction, nameof(dbTransaction));

            this.daoSession = daoSession;
            this.dbTransaction = dbTransaction;
        }

        public DaoSession Session { get => this.daoSession; }

        public void Rollback(){
            this.dbTransaction.Rollback();
            this.Dispose();
        }

        public void Commit(){
            this.dbTransaction.Commit();
            this.Dispose();
        }

        private bool disposed = false; 
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    this.dbTransaction.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

    }
}
