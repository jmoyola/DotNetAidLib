using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;

namespace DotNetAidLib.Core.Logger.Core{
	public delegate void LogEntryArrivatedEventHandler(object sender, LogEntryArrivatedEventArgs args);

	public abstract class LoggerServer
	{
		protected bool _Started = false;
		private AsyncExceptionHandler _AsyncExceptionHandler = null;
		public event LogEntryArrivatedEventHandler LogEntryArrivated;

		public LoggerServer()
		{
		}

		public AsyncExceptionHandler AsyncExceptionHandler {
			get { return _AsyncExceptionHandler; }
			set { _AsyncExceptionHandler = value; }
		}

		public bool Started {
			get { return _Started; }
		}


		public void StartServer()
		{
			if ((!_Started)) {
				_Started = true;
				Thread StartServer_Thread = new Thread(new ThreadStart(StartServer_ThreadStart));
				StartServer_Thread.Start();
			}
		}

		public void StopServer()
		{
			if ((_Started)) {
				_Started = false;
			}
		}

		protected abstract void StartServer_ThreadStart();

		protected void OnLogEntryArrivated(object sender, LogEntryArrivatedEventArgs args)
		{
			if (LogEntryArrivated != null) {
				LogEntryArrivated(sender, args);
			}
		}
	}
}