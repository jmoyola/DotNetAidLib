using System;
using System.Data;
using DotNetAidLib.Core.Develop;


namespace DotNetAidLib.Database.SQL.Core
{
    public abstract class SQLUploaderFile:IDisposable
    {
        private IDbConnection cnx;
        private SourceType sourceType;
        private String sourcePath;

        public SQLUploaderFile(IDbConnection cnx, SourceType sourceType, String sourcePath)
        {
            Assert.NotNull( cnx, nameof(cnx));
            Assert.IsValidPath(nameof(sourcePath), sourcePath);

            this.cnx = cnx;
            this.sourceType = sourceType;
            this.sourcePath = sourcePath;
        }

        public IDbConnection Cnx
        {
            get
            {
                return cnx;
            }
        }

        public SourceType SourceType
        {
            get
            {
                return sourceType;
            }
        }

        public string SourcePath
        {
            get
            {
                return sourcePath;
            }
        }

        protected abstract void OnDispose();

        public void Dispose()
        {
            this.OnDispose();
        }

        public static SQLUploaderFile Instance(IDbConnection cnx, SourceType fileSource, String sourcePath) {
            return SQLParserFactory.Instance(cnx).GetContentFromFile(cnx, fileSource, sourcePath);
        }
    }
}
