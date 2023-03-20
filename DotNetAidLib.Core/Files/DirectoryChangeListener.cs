using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DotNetAidLib.Core{
    public class FileChangeInfo{
        private FileInfo m_File;
        private WatcherChangeTypes m_ChangeType;

        public FileChangeInfo(FileInfo file, WatcherChangeTypes changeType){
            this.m_File = file;
            this.m_ChangeType = changeType;
        }

        public FileInfo File{
            get{
                return m_File;
            }
        }

        public WatcherChangeTypes ChangeType{
            get{
                return m_ChangeType;
            }
        }
    }

    public class DirectoryChangeEventArgs : EventArgs{
        private IList<FileChangeInfo> m_FileChangeInfoList;

        public DirectoryChangeEventArgs(IList<FileChangeInfo> fileChangeInfoList){
            this.m_FileChangeInfoList = fileChangeInfoList;
        }

        public IList<FileChangeInfo> FileChangeInfoList{
            get{
                return m_FileChangeInfoList;
            }
        }
    }

    public class FileInfoEqualityComparerForPath : EqualityComparer<FileInfo>{
        public override bool Equals(FileInfo x, FileInfo y){
            return x.FullName.Equals(y.FullName);
        }

        public override int GetHashCode(FileInfo obj){
            return obj.FullName.GetHashCode();
        }
    }

    public class FileInfoEqualityComparerForLastWriteTime : EqualityComparer<FileInfo>{
        public override bool Equals(FileInfo x, FileInfo y){
            return x.LastWriteTime.Equals(y.LastWriteTime);
        }

        public override int GetHashCode(System.IO.FileInfo obj){
            return obj.LastWriteTime.GetHashCode();
        }
    }

    public delegate void DirectoryChangeEventHandler(object sender, DirectoryChangeEventArgs args);
    public class DirectoryChangeListener{
        private DirectoryInfo m_Directory;
        private bool m_IncludeInitialContent = true;
        private List<FileInfo> m_Content = new List<FileInfo>();
        private string m_Filter = "*.*";
        private bool m_Started;
        private int m_Interval = 100;
        public DirectoryChangeListener(){}

        public DirectoryChangeListener(DirectoryInfo directory){
            this.Directory = directory;
        }

        public DirectoryChangeListener(DirectoryInfo directory, bool includeInitialContent)
        {
            this.Directory = directory;
            this.IncludeInitialContent = includeInitialContent;
        }

        public event DirectoryChangeEventHandler DirectoryChanged;

        public int Interval{
            get{
                return m_Interval;
            }
            set{
                if ((value >= 100))
                    m_Interval = value;
                else
                    throw new Exception("Invalid value for Interval property. Only equal or greatest of 100 are valid.");
            }
        }

        public string Filter{
            get{
                return m_Filter;
            }
            set{
                m_Filter = value;
            }
        }

        public DirectoryInfo Directory{
            get{
                return m_Directory;
            }
            set{
                if (!(value == null))
                    m_Directory = value;
                else
                    throw new Exception("Directory property can't be null.");
            }
        }

        public bool IncludeInitialContent{
            get{
                return m_IncludeInitialContent;
            }
            set{
                m_IncludeInitialContent = value;
            }
        }

        public bool Started{
            get{
                return m_Started;
            }
        }

        public void StartListener()
        {
            if (!m_Started){
                if (this.IncludeInitialContent)
                    m_Content = m_Directory.GetFiles(m_Filter).ToList();
                else
                    m_Content = new List<FileInfo>();

                Thread th = new Thread(new ThreadStart(Count));
                m_Started = true;
                th.Start();
            }
        }

        public void StopListener(){
            if (m_Started)
                m_Started = false;
        }

        private void Count(){
            while (m_Started){
                Thread.Sleep(m_Interval);
                List<FileInfo> actual = m_Directory.GetFiles(m_Filter).ToList();
                List<FileInfo> diff_minus = m_Content.Except(actual, new FileInfoEqualityComparerForPath()).ToList();
                List<FileInfo> diff_plus = actual.Except(m_Content, new FileInfoEqualityComparerForPath()).ToList();
                List<FileInfo> diff_changed = actual.Except(m_Content, new FileInfoEqualityComparerForLastWriteTime()).ToList();
                List<FileChangeInfo> ch = new List<FileChangeInfo>();
                foreach (FileInfo dp in diff_plus)
                    ch.Add(new FileChangeInfo(dp, WatcherChangeTypes.Created));
                foreach (FileInfo dp in diff_minus)
                    ch.Add(new FileChangeInfo(dp, WatcherChangeTypes.Deleted));
                foreach (FileInfo dp in diff_changed){
                    if (!diff_plus.Contains(dp))
                        ch.Add(new FileChangeInfo(dp, WatcherChangeTypes.Changed));
                }

                if ((ch.Count > 0))
                    DirectoryChanged(this, new DirectoryChangeEventArgs(ch));

                m_Content = actual;
            }
        }
    }
}
