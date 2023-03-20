using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Files;

namespace DotNetAidLib.Core.Files.Temp
{
    public class TempDirectory: IDisposable
    {
        public static event TypedEventHandler<TempDirectory> Created;
        public static event TypedCancellableEventHandler<TempDirectory>  Disposing;
        public static event TypedEventHandler<TempDirectory> Disposed;

        protected static IDictionary<String, TempDirectory> instances = new Dictionary<string, TempDirectory>();

        private bool disposed = false;
        private bool autoDispose = false;
        private readonly DirectoryInfo baseDirectory;
        private DateTime creationDate;

        protected TempDirectory(DirectoryInfo baseDirectory)
            :this(baseDirectory, DateTime.Now){}

        protected TempDirectory(DirectoryInfo baseDirectory, DateTime creationDate)
        {
            Assert.NotNull( baseDirectory, nameof(baseDirectory));

            this.baseDirectory = baseDirectory;

            if (!this.baseDirectory.RefreshFluent().Exists)
                this.baseDirectory.Create();

            this.baseDirectory.Refresh();
            this.creationDate = creationDate;

            this.OnCreated(new TypedEventArgs<TempDirectory>(this));
        }

        public static IEnumerable<TempDirectory> Instances
        {
            get { return instances.Values; }
        }

        public DirectoryInfo BaseDirectory {
            get { return this.baseDirectory; }
        }

        public DateTime CreationDate
        {
            get { return creationDate; }
        }

        public bool AutoDispose
        {
            get{return autoDispose;}

            set{autoDispose = value;}
        }

        public DirectoryInfo AddDirectoryInfo(String name)
        {
            return new DirectoryInfo(this.baseDirectory.FullName + Path.DirectorySeparatorChar +  name);
        }

        public FileInfo AddFileInfo(String name)
        {
            return new System.IO.FileInfo(this.baseDirectory.FullName + Path.DirectorySeparatorChar + name);
        }

        public void Clean() {
            this.baseDirectory.GetFiles().ToList().ForEach(v=>v.Delete());
            this.baseDirectory.GetDirectories().ToList().ForEach(v => v.Delete(true));
        }

        public override string ToString()
        {
            return this.baseDirectory.ToString();
        }

        public override int GetHashCode()
        {
            return this.baseDirectory.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && typeof(TempDirectory).IsAssignableFrom(obj.GetType()))
                return this.baseDirectory.Equals(((TempDirectory)obj).BaseDirectory);
            else if (obj != null && typeof(DirectoryInfo).IsAssignableFrom(obj.GetType()))
                return this.baseDirectory.Equals(obj);
            else
                return false;
        }

        public static void Expire(long maxSeconds)
        {
            DateTime now = DateTime.Now;
            foreach(String key in instances.Keys.ToArray())
            {
                if (now.Subtract(instances[key].creationDate).TotalSeconds > maxSeconds)
                    instances[key].Dispose();
            }
        }

        public static void DisposeAll()
        {
            foreach (String key in instances.Keys.ToArray()){
                    instances[key].Dispose();
            }
        }

        public static implicit operator DirectoryInfo(TempDirectory v)
        {
            return v.BaseDirectory;
        }

        ~TempDirectory()
        {
            if(this.autoDispose)
                this.Dispose();
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                // Evento Disposing (Cancelable)
                if (Disposing != null)
                {
                    TypedCancellableEventArgs<TempDirectory> args = new TypedCancellableEventArgs<TempDirectory>(this);
                    this.OnDisposing(args);
                    if (args.Cancel)
                        return;
                }

                baseDirectory.Refresh();
                // Free any other managed objects here.
                if (baseDirectory.Exists)
                    baseDirectory.Delete(true);
                instances.Remove(baseDirectory.FullName);

                this.OnDisposed(new TypedEventArgs<TempDirectory>(this));

                this.disposed = true;
            }
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
            Assert.NotNull( baseDirectory, nameof(baseDirectory));

            if (!instances.ContainsKey(baseDirectory.FullName))
                instances.Add(baseDirectory.FullName, new TempDirectory(baseDirectory));

            return instances[baseDirectory.FullName];
        }

        public static TempDirectory New()
        {
            String randomGUIDPath = Path.GetTempPath() + GuidGenerator.GenerateTimeBasedGuid().ToString() + ".tmp";
            return New(new DirectoryInfo(randomGUIDPath));
        }

        static TempDirectory()
        {
            String tempNamePattern = @"([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\.tmp";
            foreach (DirectoryInfo di in new DirectoryInfo(Path.GetTempPath())
                                            .GetDirectories().Where(v => v.Name.RegexIsMatch(tempNamePattern)))
            {
                Guid guid = new Guid(di.Name.RegexGroupsMatches(tempNamePattern)[1]);
                DateTime creationDate = DateTime.Now;
                if (GuidGenerator.GetUuidVersion(guid) == GuidVersion.TimeBased)
                    creationDate = GuidGenerator.GetDateTime(guid).ToLocalTime();
                TempDirectory tempDirectory = new TempDirectory(di, creationDate);
                instances.Add(di.FullName, tempDirectory);
            }
        }
    }
}
