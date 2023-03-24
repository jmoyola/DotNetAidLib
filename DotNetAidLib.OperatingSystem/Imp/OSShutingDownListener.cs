using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;

namespace Library.OperatingSystem.Imp
{
    public class ShuttingDownEventArgs : EventArgs
    {
        private bool isShuttingDown = false;
        public ShuttingDownEventArgs(bool isShuttingDown) {
            this.isShuttingDown = isShuttingDown;
        }
        public bool IsShuttingDown => this.isShuttingDown;
    }

    public delegate void ShuttingDownEventHandler(Object sender, ShuttingDownEventArgs args);

    public class OSShutingDownListener:IStartable
    {
        private static OSShutingDownListener instance=null;

        private BackgroundWorker t = null;
        private bool started = false;
        private bool isShuttingDown = false;
        public event ShuttingDownEventHandler ShuttingDown;

        private FileInfo f = null;

        protected OSShutingDownListener()
        {
            f = new FileInfo("/tmp/shutdown");
        }

        public bool Started => this.started;

        private static Object oIsShuttingDown = new object();
        public bool IsShuttingDown
        {
            get
            {
                lock (oIsShuttingDown)
                {
                    return this.isShuttingDown;
                }
            }
        }

        public void Start()
        {
            if (this.started)
                throw new Exception("Already started.");
            t = new BackgroundWorker();
            t.DoWork += start_thread; ;
            t.WorkerReportsProgress = false;
            t.WorkerSupportsCancellation = false;
            this.started = true;
            t.RunWorkerAsync();
        }

        public void Stop()
        {
            if (!this.started)
                throw new Exception("Already stopped.");
            this.started = false;
        }

        private void start_thread(object sender, DoWorkEventArgs e)
        {
            while (this.started) {
                Thread.Sleep(1);

                bool auxIsShuttingDown = f.RefreshFluent().Exists;
                if (auxIsShuttingDown != this.isShuttingDown){
                    this.isShuttingDown = auxIsShuttingDown;
                    this.OnShuttingDown(new ShuttingDownEventArgs(this.isShuttingDown));
                }
            }
        }

        protected void OnShuttingDown(ShuttingDownEventArgs args) {
            if (this.ShuttingDown != null)
                this.ShuttingDown(this, args);
        }

        public static OSShutingDownListener Instance() {
            if (instance == null)
                instance = new OSShutingDownListener();
            return instance;
        }
    }
}
