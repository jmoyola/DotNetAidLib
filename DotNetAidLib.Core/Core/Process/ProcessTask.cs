using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;

namespace DotNetAidLib.Core.Proc
{
    public delegate void ProcessTaskExceptionEventHandler(object sender, ProcessTaskExceptionEventArgs args);

    public class ProcessTaskExceptionEventArgs : EventArgs
    {
        public ProcessTaskExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }

    public class ProcessTaskException : Exception
    {
        public ProcessTaskException()
        {
        }

        public ProcessTaskException(string message) : base(message)
        {
        }

        public ProcessTaskException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ProcessTaskException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class ProcessTask
    {
        private readonly System.Diagnostics.Process process;

        public ProcessTask(System.Diagnostics.Process process, Action<System.Diagnostics.Process> endTaskAction)
        {
            if (process == null)
                throw new ProcessTaskException("process can't be null.");
            if (endTaskAction == null)
                throw new ProcessTaskException("cancelAction can't be null.");

            this.process = process;
            this.process.Exited += Exited_process;
            CancelAction = endTaskAction;
        }

        public Action<System.Diagnostics.Process> CancelAction { get; set; }

        public event DataReceivedEventHandler OutputDataReceived;

        protected void OnDataReceivedEvent(DataReceivedEventArgs args)
        {
            if (OutputDataReceived != null)
                OutputDataReceived(this, args);
        }

        private void Exited_process(object sender, EventArgs args)
        {
            OnEndTaskEvent(args);
        }

        public event EventHandler EndTask;

        protected void OnEndTaskEvent(EventArgs args)
        {
            if (EndTask != null)
                EndTask(this, args);
        }

        public void Start()
        {
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            if (OutputDataReceived != null) process.OutputDataReceived += DataReceivedEvent_process;
            process.Start();
            if (OutputDataReceived != null)
                process.BeginOutputReadLine();
        }

        private void DataReceivedEvent_process(object sender, DataReceivedEventArgs e)
        {
            OnDataReceivedEvent(e);
        }

        public void End(bool waitUtilHasExited)
        {
            CancelAction.Invoke(process);

            if (waitUtilHasExited)
                while (!HasExited())
                    Thread.Sleep(1);
        }

        public bool HasExited()
        {
            return process.HasExited;
        }

        public bool HasError()
        {
            return process.ExitCode != 0;
        }

        public Exception GetError()
        {
            if (HasError())
                return new ProcessTaskException("Error procesing process: " + process.StandardError.ReadToEnd());
            return null;
        }

        public string StandardOutput()
        {
            if (!HasExited())
                throw new ProcessTaskException("Process has not exited.");

            return process.StandardOutput.ReadToEnd();
        }
    }
}