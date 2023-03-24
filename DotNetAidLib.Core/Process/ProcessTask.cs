using System;
using System.Threading;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace DotNetAidLib.Core.Proc
{
    public delegate void ProcessTaskExceptionEventHandler(Object sender, ProcessTaskExceptionEventArgs args);
    public class ProcessTaskExceptionEventArgs : EventArgs
    {
        private Exception exception;

        public ProcessTaskExceptionEventArgs(Exception exception){
            this.exception = exception;
        }

        public Exception Exception {
            get { return this.exception; }
        }
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
        private System.Diagnostics.Process process;
        private Action<System.Diagnostics.Process> endTaskAction;

        public ProcessTask(System.Diagnostics.Process process, Action<System.Diagnostics.Process> endTaskAction)
        {
            if (process == null)
                throw new ProcessTaskException("process can't be null.");
            if (endTaskAction == null)
                throw new ProcessTaskException("cancelAction can't be null.");
            
            this.process = process;
            this.process.Exited += Exited_process;
            this.endTaskAction = endTaskAction;
        }

        public event DataReceivedEventHandler OutputDataReceived;
        protected void OnDataReceivedEvent(DataReceivedEventArgs args)
        {
            if (this.OutputDataReceived != null)
                this.OutputDataReceived(this, args);
        }

        private void Exited_process(Object sender, EventArgs args) {
            this.OnEndTaskEvent(args);
        }

        public event EventHandler EndTask;
        protected void OnEndTaskEvent(EventArgs args)
        {
            if (this.EndTask != null)
                this.EndTask(this, args);
        }

        public Action<System.Diagnostics.Process> CancelAction {
            get { return endTaskAction; }
            set { endTaskAction = value; }
        }

        public void Start(){
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            if (this.OutputDataReceived != null)
            {
                process.OutputDataReceived += DataReceivedEvent_process;
            }
            process.Start();
            if (this.OutputDataReceived != null)
                process.BeginOutputReadLine();
        }

        private void DataReceivedEvent_process(object sender, DataReceivedEventArgs e){
            this.OnDataReceivedEvent(e);
        }

        public void End(bool waitUtilHasExited){
            this.endTaskAction.Invoke(process);

            if (waitUtilHasExited){
                while (!this.HasExited())
                {
                    Thread.Sleep(1);
                }
            }
        }

        public bool HasExited(){
            return process.HasExited;
        }

        public bool HasError(){
            return process.ExitCode != 0;
        }

        public Exception GetError(){

            if (this.HasError())
                return new ProcessTaskException("Error procesing process: " + process.StandardError.ReadToEnd());
            else
                return null;
        }

        public string StandardOutput() {
            if(!this.HasExited())
                throw new ProcessTaskException("Process has not exited.");
            
            return process.StandardOutput.ReadToEnd();
        }

    }
}

