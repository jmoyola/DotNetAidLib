using System.Threading;

namespace DotNetAidLib.Logger.Core
{
    public delegate void LogEntryArrivatedEventHandler(object sender, LogEntryArrivatedEventArgs args);

    public abstract class LoggerServer
    {
        protected bool _Started;

        public AsyncExceptionHandler AsyncExceptionHandler { get; set; } = null;

        public bool Started => _Started;

        public event LogEntryArrivatedEventHandler LogEntryArrivated;


        public void StartServer()
        {
            if (!_Started)
            {
                _Started = true;
                var StartServer_Thread = new Thread(StartServer_ThreadStart);
                StartServer_Thread.Start();
            }
        }

        public void StopServer()
        {
            if (_Started) _Started = false;
        }

        protected abstract void StartServer_ThreadStart();

        protected void OnLogEntryArrivated(object sender, LogEntryArrivatedEventArgs args)
        {
            if (LogEntryArrivated != null) LogEntryArrivated(sender, args);
        }
    }
}