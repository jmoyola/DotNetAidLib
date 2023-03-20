using System;
using System.ComponentModel;

namespace DotNetAidLib.Core.Proc.Process
{
    public enum BackgroundProcessStatus{
        Initialiced,
        Started,
        Finish,
        Cancelled,
        Error
    }

    public delegate void BackgroundProcessEventHandler(Object sender, BackgroundProcessEventArgs args);
    public class BackgroundProcessEventArgs{
        private BackgroundProcessStatus status;

        public BackgroundProcessEventArgs(BackgroundProcessStatus status){
            this.status = status;
        }

        public BackgroundProcessStatus Status
        {
            get
            {
                return status;
            }
        }
    }

    public class BackgroundProcess
    {
        public BackgroundProcessEventHandler BackgroundProcessEvent;
        private BackgroundWorker backgroundWorker;
        private Action<Object> action;
        private Func<Object, Object> function;
        private BackgroundProcessStatus status=BackgroundProcessStatus.Initialiced;
        private Object result;
        private Exception exception;

        public BackgroundProcess(Action<Object> action){
            this.action = action;
            this.Init();
        }

        public BackgroundProcess(Func<Object, Object> function)
        {
            this.function = function;
            this.Init();
        }

        private void Init(){
            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.DoWork += Bg_DoWork;
            this.backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            this.backgroundWorker.WorkerSupportsCancellation = true;
        }

        public void Start(Object parameter){
            if (this.backgroundWorker.IsBusy)
                throw new Exception("Process is running.");
            this.exception = null;
            this.result = null;
            this.status = BackgroundProcessStatus.Started;
            this.OnBackgroundProcessEvent(new BackgroundProcessEventArgs(this.status));
            this.backgroundWorker.RunWorkerAsync(parameter);
        }

        public void Cancel(){
            if (!this.backgroundWorker.IsBusy)
                throw new Exception("Process is not running.");
            this.backgroundWorker.CancelAsync();
            this.status = BackgroundProcessStatus.Cancelled;
            this.OnBackgroundProcessEvent(new BackgroundProcessEventArgs(this.status));
        }

        public BackgroundProcessStatus Status
        {
            get
            {
                return status;
            }
        }

        public Object Result
        {
            get
            {
                return result;
            }
        }

        public Exception Exception
        {
            get
            {
                return exception;
            }
        }

        private void Bg_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if(function!=null)
                    this.result = this.function.Invoke(e.Argument);

                if (action != null)
                    this.action.Invoke(e.Argument);

            }
            catch (Exception ex){
                this.status = BackgroundProcessStatus.Error;
                this.exception = ex;
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.status = BackgroundProcessStatus.Finish;
            this.OnBackgroundProcessEvent(new BackgroundProcessEventArgs(this.status));
        }

        protected void OnBackgroundProcessEvent(BackgroundProcessEventArgs args)
        {
            if (this.BackgroundProcessEvent != null)
                this.BackgroundProcessEvent(this, args);
        }

        public static BackgroundProcess NewProcess(Func<Object, Object> action)
        {
            return new BackgroundProcess(action);
        }

        public static BackgroundProcess NewProcess(Action<Object> action)
        {
            return new BackgroundProcess(action);
        }
    }
}
