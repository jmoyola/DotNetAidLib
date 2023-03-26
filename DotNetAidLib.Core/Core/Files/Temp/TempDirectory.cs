using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Files.Temp
{
    public class TempDirectory : IDisposable
    {
        protected static IDictionary<string, TempDirectory> instances = new Dictionary<string, TempDirectory>();

        private bool disposed;

        static TempDirectory()
        {
            var tempNamePattern = @"([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\.tmp";
            foreach (var di in new DirectoryInfo(Path.GetTempPath())
                         .GetDirectories().Where(v => v.Name.RegexIsMatch(tempNamePattern)))
            {
                var guid = new Guid(di.Name.RegexGroupsMatches(tempNamePattern)[1]);
                var creationDate = DateTime.Now;
                if (guid.GetUuidVersion() == GuidVersion.TimeBased)
                    creationDate = GuidGenerator.GetDateTime(guid).ToLocalTime();
                var tempDirectory = new TempDirectory(di, creationDate);
                instances.Add(di.FullName, tempDirectory);
            }
        }

        protected TempDirectory(DirectoryInfo baseDirectory)
            : this(baseDirectory, DateTime.Now)
        {
        }

        protected TempDirectory(DirectoryInfo baseDirectory, DateTime creationDate)
        {
            Assert.NotNull(baseDirectory, nameof(baseDirectory));

            BaseDirectory = baseDirectory;

            if (!BaseDirectory.RefreshFluent().Exists)
                BaseDirectory.Create();

            BaseDirectory.Refresh();
            CreationDate = creationDate;

            OnCreated(new TypedEventArgs<TempDirectory>(this));
        }

        public static IEnumerable<TempDirectory> Instances => instances.Values;

        public DirectoryInfo BaseDirectory { get; }

        public DateTime CreationDate { get; }

        public bool AutoDispose { get; set; } = false;

        public void Dispose()
        {
            if (!disposed)
            {
                // Evento Disposing (Cancelable)
                if (Disposing != null)
                {
                    var args = new TypedCancellableEventArgs<TempDirectory>(this);
                    OnDisposing(args);
                    if (args.Cancel)
                        return;
                }

                BaseDirectory.Refresh();
                // Free any other managed objects here.
                if (BaseDirectory.Exists)
                    BaseDirectory.Delete(true);
                instances.Remove(BaseDirectory.FullName);

                OnDisposed(new TypedEventArgs<TempDirectory>(this));

                disposed = true;
            }
        }

        public static event TypedEventHandler<TempDirectory> Created;
        public static event TypedCancellableEventHandler<TempDirectory> Disposing;
        public static event TypedEventHandler<TempDirectory> Disposed;

        public DirectoryInfo AddDirectoryInfo(string name)
        {
            return new DirectoryInfo(BaseDirectory.FullName + Path.DirectorySeparatorChar + name);
        }

        public FileInfo AddFileInfo(string name)
        {
            return new FileInfo(BaseDirectory.FullName + Path.DirectorySeparatorChar + name);
        }

        public void Clean()
        {
            BaseDirectory.GetFiles().ToList().ForEach(v => v.Delete());
            BaseDirectory.GetDirectories().ToList().ForEach(v => v.Delete(true));
        }

        public override string ToString()
        {
            return BaseDirectory.ToString();
        }

        public override int GetHashCode()
        {
            return BaseDirectory.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && typeof(TempDirectory).IsAssignableFrom(obj.GetType()))
                return BaseDirectory.Equals(((TempDirectory) obj).BaseDirectory);
            if (obj != null && typeof(DirectoryInfo).IsAssignableFrom(obj.GetType()))
                return BaseDirectory.Equals(obj);
            return false;
        }

        public static void Expire(long maxSeconds)
        {
            var now = DateTime.Now;
            foreach (var key in instances.Keys.ToArray())
                if (now.Subtract(instances[key].CreationDate).TotalSeconds > maxSeconds)
                    instances[key].Dispose();
        }

        public static void DisposeAll()
        {
            foreach (var key in instances.Keys.ToArray()) instances[key].Dispose();
        }

        public static implicit operator DirectoryInfo(TempDirectory v)
        {
            return v.BaseDirectory;
        }

        ~TempDirectory()
        {
            if (AutoDispose)
                Dispose();
        }


        protected void OnCreated(TypedEventArgs<TempDirectory> args)
        {
            // Evento Disposed
            if (Created != null)
                Created(this, args);
        }

        protected void OnDisposing(TypedCancellableEventArgs<TempDirectory> args)
        {
            // Evento Disposing
            if (Disposing != null)
                Disposing(this, args);
        }

        protected void OnDisposed(TypedEventArgs<TempDirectory> args)
        {
            // Evento Disposed
            if (Disposed != null)
                Disposed(this, args);
        }

        public static TempDirectory New(DirectoryInfo baseDirectory)
        {
            Assert.NotNull(baseDirectory, nameof(baseDirectory));

            if (!instances.ContainsKey(baseDirectory.FullName))
                instances.Add(baseDirectory.FullName, new TempDirectory(baseDirectory));

            return instances[baseDirectory.FullName];
        }

        public static TempDirectory New()
        {
            var randomGUIDPath = Path.GetTempPath() + GuidGenerator.GenerateTimeBasedGuid() + ".tmp";
            return New(new DirectoryInfo(randomGUIDPath));
        }
    }
}