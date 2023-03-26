using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DotNetAidLib.Core
{
    public class FileChangeInfo
    {
        public FileChangeInfo(FileInfo file, WatcherChangeTypes changeType)
        {
            File = file;
            ChangeType = changeType;
        }

        public FileInfo File { get; }

        public WatcherChangeTypes ChangeType { get; }
    }

    public class DirectoryChangeEventArgs : EventArgs
    {
        public DirectoryChangeEventArgs(IList<FileChangeInfo> fileChangeInfoList)
        {
            FileChangeInfoList = fileChangeInfoList;
        }

        public IList<FileChangeInfo> FileChangeInfoList { get; }
    }

    public class FileInfoEqualityComparerForPath : EqualityComparer<FileInfo>
    {
        public override bool Equals(FileInfo x, FileInfo y)
        {
            return x.FullName.Equals(y.FullName);
        }

        public override int GetHashCode(FileInfo obj)
        {
            return obj.FullName.GetHashCode();
        }
    }

    public class FileInfoEqualityComparerForLastWriteTime : EqualityComparer<FileInfo>
    {
        public override bool Equals(FileInfo x, FileInfo y)
        {
            return x.LastWriteTime.Equals(y.LastWriteTime);
        }

        public override int GetHashCode(FileInfo obj)
        {
            return obj.LastWriteTime.GetHashCode();
        }
    }

    public delegate void DirectoryChangeEventHandler(object sender, DirectoryChangeEventArgs args);

    public class DirectoryChangeListener
    {
        private List<FileInfo> m_Content = new List<FileInfo>();
        private DirectoryInfo m_Directory;
        private int m_Interval = 100;

        public DirectoryChangeListener()
        {
        }

        public DirectoryChangeListener(DirectoryInfo directory)
        {
            Directory = directory;
        }

        public DirectoryChangeListener(DirectoryInfo directory, bool includeInitialContent)
        {
            Directory = directory;
            IncludeInitialContent = includeInitialContent;
        }

        public int Interval
        {
            get => m_Interval;
            set
            {
                if (value >= 100)
                    m_Interval = value;
                else
                    throw new Exception(
                        "Invalid value for Interval property. Only equal or greatest of 100 are valid.");
            }
        }

        public string Filter { get; set; } = "*.*";

        public DirectoryInfo Directory
        {
            get => m_Directory;
            set
            {
                if (!(value == null))
                    m_Directory = value;
                else
                    throw new Exception("Directory property can't be null.");
            }
        }

        public bool IncludeInitialContent { get; set; } = true;

        public bool Started { get; private set; }

        public event DirectoryChangeEventHandler DirectoryChanged;

        public void StartListener()
        {
            if (!Started)
            {
                if (IncludeInitialContent)
                    m_Content = m_Directory.GetFiles(Filter).ToList();
                else
                    m_Content = new List<FileInfo>();

                var th = new Thread(Count);
                Started = true;
                th.Start();
            }
        }

        public void StopListener()
        {
            if (Started)
                Started = false;
        }

        private void Count()
        {
            while (Started)
            {
                Thread.Sleep(m_Interval);
                var actual = m_Directory.GetFiles(Filter).ToList();
                var diff_minus = m_Content.Except(actual, new FileInfoEqualityComparerForPath()).ToList();
                var diff_plus = actual.Except(m_Content, new FileInfoEqualityComparerForPath()).ToList();
                var diff_changed = actual.Except(m_Content, new FileInfoEqualityComparerForLastWriteTime()).ToList();
                var ch = new List<FileChangeInfo>();
                foreach (var dp in diff_plus)
                    ch.Add(new FileChangeInfo(dp, WatcherChangeTypes.Created));
                foreach (var dp in diff_minus)
                    ch.Add(new FileChangeInfo(dp, WatcherChangeTypes.Deleted));
                foreach (var dp in diff_changed)
                    if (!diff_plus.Contains(dp))
                        ch.Add(new FileChangeInfo(dp, WatcherChangeTypes.Changed));

                if (ch.Count > 0)
                    DirectoryChanged(this, new DirectoryChangeEventArgs(ch));

                m_Content = actual;
            }
        }
    }
}