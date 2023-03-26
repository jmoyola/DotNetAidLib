using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.OperatingSystem.Imp
{
    public class ShuttingDownEventArgs : EventArgs
    {
        public ShuttingDownEventArgs(bool isShuttingDown)
        {
            IsShuttingDown = isShuttingDown;
        }

        public bool IsShuttingDown { get; }
    }

    public delegate void ShuttingDownEventHandler(object sender, ShuttingDownEventArgs args);

    public class OSShutingDownListener : IStartable
    {
        private static OSShutingDownListener instance;

        private static readonly object oIsShuttingDown = new object();

        private readonly FileInfo f;
        private bool isShuttingDown;

        private BackgroundWorker t;

        protected OSShutingDownListener()
        {
            f = new FileInfo("/tmp/shutdown");
        }

        public bool IsShuttingDown
        {
            get
            {
                lock (oIsShuttingDown)
                {
                    return isShuttingDown;
                }
            }
        }

        public bool Started { get; private set; }

        public void Start()
        {
            if (Started)
                throw new Exception("Already started.");
            t = new BackgroundWorker();
            t.DoWork += start_thread;
            ;
            t.WorkerReportsProgress = false;
            t.WorkerSupportsCancellation = false;
            Started = true;
            t.RunWorkerAsync();
        }

        public void Stop()
        {
            if (!Started)
                throw new Exception("Already stopped.");
            Started = false;
        }

        public event ShuttingDownEventHandler ShuttingDown;

        private void start_thread(object sender, DoWorkEventArgs e)
        {
            while (Started)
            {
                Thread.Sleep(1);

                var auxIsShuttingDown = f.RefreshFluent().Exists;
                if (auxIsShuttingDown != isShuttingDown)
                {
                    isShuttingDown = auxIsShuttingDown;
                    OnShuttingDown(new ShuttingDownEventArgs(isShuttingDown));
                }
            }
        }

        protected void OnShuttingDown(ShuttingDownEventArgs args)
        {
            if (ShuttingDown != null)
                ShuttingDown(this, args);
        }

        public static OSShutingDownListener Instance()
        {
            if (instance == null)
                instance = new OSShutingDownListener();
            return instance;
        }
    }
}