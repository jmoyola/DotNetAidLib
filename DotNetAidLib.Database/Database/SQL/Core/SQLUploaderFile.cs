using System;
using System.Data;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database.SQL.Core
{
    public abstract class SQLUploaderFile : IDisposable
    {
        public SQLUploaderFile(IDbConnection cnx, SourceType sourceType, string sourcePath)
        {
            Assert.NotNull(cnx, nameof(cnx));
            Assert.IsValidPath(nameof(sourcePath), sourcePath);

            Cnx = cnx;
            SourceType = sourceType;
            SourcePath = sourcePath;
        }

        public IDbConnection Cnx { get; }

        public SourceType SourceType { get; }

        public string SourcePath { get; }

        public void Dispose()
        {
            OnDispose();
        }

        protected abstract void OnDispose();

        public static SQLUploaderFile Instance(IDbConnection cnx, SourceType fileSource, string sourcePath)
        {
            return SQLParserFactory.Instance(cnx).GetContentFromFile(cnx, fileSource, sourcePath);
        }
    }
}