using System;
using System.ComponentModel;

namespace DotNetAidLib.Core.Process
{
    public enum BackgroundProcessStatus
    {
        Initialiced,
        Started,
        Finish,
        Cancelled,
        Error
    }

    public delegate void BackgroundProcessEventHandler(object sender, BackgroundProcessEventArgs args);

    public class BackgroundProcessEventArgs
    {
        public BackgroundProcessEventArgs(BackgroundProcessStatus status)
        {
            Status = status;
        }

        public BackgroundProcessStatus Status { get; }
    }

    public class BackgroundProcess
    {
        private readonly Action<object> action;
        public BackgroundProcessEventHandler BackgroundProcessEvent;
        private BackgroundWorker backgroundWorker;
        private readonly Func<object, object> function;

        public BackgroundProcess(Action<object> action)
        {
            this.action = action;
            Init();
        }

        public BackgroundProcess(Func<object, object> function)
        {
            this.function = function;
            Init();
        }

        public BackgroundProcessStatus Status { get; private set; } = BackgroundProcessStatus.Initialiced;

        public object Result { get; private set; }

        public Exception Exception { get; private set; }

        private void Init()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += Bg_DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            backgroundWorker.WorkerSupportsCancellation = true;
        }

        public void Start(object parameter)
        {
            if (backgroundWorker.IsBusy)
                throw new Exception("Process is running.");
            Exception = null;
            Result = null;
            Status = BackgroundProcessStatus.Started;
            OnBackgroundProcessEvent(new BackgroundProcessEventArgs(Status));
            backgroundWorker.RunWorkerAsync(parameter);
        }

        public void Cancel()
        {
            if (!backgroundWorker.IsBusy)
                throw new Exception("Process is not running.");
            backgroundWorker.CancelAsync();
            Status = BackgroundProcessStatus.Cancelled;
            OnBackgroundProcessEvent(new BackgroundProcessEventArgs(Status));
        }

        private void Bg_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (function != null)
                    Result = function.Invoke(e.Argument);

                if (action != null)
                    action.Invoke(e.Argument);
            }
            catch (Exception ex)
            {
                Status = BackgroundProcessStatus.Error;
                Exception = ex;
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Status = BackgroundProcessStatus.Finish;
            OnBackgroundProcessEvent(new BackgroundProcessEventArgs(Status));
        }

        protected void OnBackgroundProcessEvent(BackgroundProcessEventArgs args)
        {
            if (BackgroundProcessEvent != null)
                BackgroundProcessEvent(this, args);
        }

        public static BackgroundProcess NewProcess(Func<object, object> action)
        {
            return new BackgroundProcess(action);
        }

        public static BackgroundProcess NewProcess(Action<object> action)
        {
            return new BackgroundProcess(action);
        }
    }
}