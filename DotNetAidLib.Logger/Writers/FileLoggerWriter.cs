using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using DotNetAidLib.Logger.Core;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.IO.Archive.Core;
using DotNetAidLib.Core.IO.Archive.Zip;

namespace DotNetAidLib.Logger.Writers{

	public class FileLoggerWriter : LoggerWriter
	{

		private IArchiveFactory _ArchiveFactory=null;
		private static Dictionary<string, FileLoggerWriter> m_Instances = new Dictionary<string, FileLoggerWriter>();
		private System.IO.FileInfo m_LogFile;
		private long _MaxLogSize = 1 * 1024 * 1024;
		private int _ArchivedLogsExpireDays = 30;

		private Timer m_TimerClearOldLogs;
		private FileLoggerWriter(System.IO.FileInfo logFile)
		{
			_ArchiveFactory = ZipArchiveFactory.Instance ();

			if ((logFile == null)) {
				throw new NullReferenceException("logfile parameter can't be null.");
			}

			m_LogFile = logFile;

			// Si no existe la carpeta base, la creamos (y todo el arbol hasta ella)
			if ((!logFile.Directory.Exists)) {
				logFile.Directory.CreateAll();
			}

			// Creamos e inicializamos el timer de clearoldlogs
			TimerCallback tcb = new TimerCallback(this.TimerClearOldLogs);
			m_TimerClearOldLogs = new Timer(tcb, null, 0, 1000 * 60 * 5);
		}

		private void TimerClearOldLogs(object state)
		{
			// Si los dias de caducidad son 0, no caducan nunca
			if ((this._ArchivedLogsExpireDays > 0)) {
				//Borramos archivedlogs caducados
				this.CleanOldLogs(this.m_LogFile, this._ArchivedLogsExpireDays);
			}
		}

		public FileInfo LogFile {
			get { return m_LogFile; }
		}

		public long MaxLogSize {
			get { return _MaxLogSize; }
			set { _MaxLogSize = value; }
		}

		public int ArchivedLogsExpireDays {
			get { return _ArchivedLogsExpireDays; }
			set { _ArchivedLogsExpireDays = value; }
		}

		public override void InitConfiguration(IApplicationConfigGroup configGroup)
		{
			base.InitConfiguration(configGroup);

			if ((configGroup.ConfigurationExist("MaxLogSize"))) {
				_MaxLogSize = configGroup.GetConfiguration<long>("MaxLogSize").Value;
			} else {
				configGroup.AddConfiguration<long>("MaxLogSize", _MaxLogSize, true);
			}

			if ((configGroup.ConfigurationExist("ArchivedLogsExpireDays"))) {
				_ArchivedLogsExpireDays = configGroup.GetConfiguration<int>("ArchivedLogsExpireDays").Value;
			} else {
				configGroup.AddConfiguration<int>("ArchivedLogsExpireDays", _ArchivedLogsExpireDays, true);
			}

		}

		public override void SaveConfiguration(IApplicationConfigGroup configGroup)
		{
			base.SaveConfiguration(configGroup);


			configGroup.AddConfiguration<long>("MaxLogSize", _MaxLogSize, true);

			configGroup.AddConfiguration<int>("ArchivedLogsExpireDays", _ArchivedLogsExpireDays, true);

		}

		private object oWriteLog = new object();
		public override void WriteLog(LogEntry logEntry)
		{
			lock (oWriteLog) {
				WriteInLogFile(this.m_LogFile, logEntry);
			}
		}

		~FileLoggerWriter()
		{
			m_TimerClearOldLogs.Dispose();
		}



		private void WriteInLogFile(System.IO.FileInfo logFile, LogEntry logEntry)
		{

			try {
				// Restauramos el puntero, ya que se pierde y apunta al anterior archivo renombrado
				// dando datos erroneos de existe y tamaño
				logFile.Refresh();


				// Verificamos si hay que archivar el log actual


				if ((logFile.Exists && (logFile.Length > _MaxLogSize))) {
					// Archivamos el log

					String sLogFile=logFile.FullName;

                    System.IO.FileInfo archiveLogFile = new System.IO.FileInfo(
                        logFile.Directory.FullName + Path.DirectorySeparatorChar
                        + Path.GetFileNameWithoutExtension(logFile.FullName)
                        + "-" + DateTime.Now.ToString("yyyyMMdd_HHmmssfff")
                        + Path.GetExtension(logFile.FullName)
                    );

					File.Move(logFile.FullName, archiveLogFile.FullName);

					logFile=new System.IO.FileInfo(sLogFile);
					// Llamamos al roll del archivo de log en un  hilo aparte
					Thread hilo = new Thread(new ParameterizedThreadStart(CompressAndDeleteSourceInZip_ThreadStart));
					hilo.Start(archiveLogFile);

				}

				// Escribimos el log
				System.IO.File.AppendAllText(logFile.FullName, "\r\n" + logEntry.ToString());

			} catch (Exception ex) {
				throw new LoggerException("Error writing log to file", ex, logEntry);
			}
		}

		private object sLockArchivado = new object();
		private void CompressAndDeleteSourceInZip_ThreadStart(object oSourceFile)
		{
			lock (sLockArchivado) {
                System.IO.FileInfo sourceFile = (System.IO.FileInfo)oSourceFile;
				try {
					AddFileToZip(sourceFile.FullName + "." + _ArchiveFactory.DefaultExtension, sourceFile.FullName);
					sourceFile.Delete();
				} catch (Exception ex) {
					System.IO.File.AppendAllText(sourceFile.FullName, LoggerWriter.ExceptionToString(ex,ExceptionVerbosityLevels._HIGH));
					if (((this.AsyncExceptionHandler != null))) {
						this.AsyncExceptionHandler.Invoke(this, new Exception("Error in compress and delete log file", ex));
					}
				}

			}
		}

		private object sLockCleanOldLogs = new object();
		private void CleanOldLogs(System.IO.FileInfo logFile, int ExpireDays)
		{
			lock (sLockCleanOldLogs) {
				try {
                    System.IO.FileInfo archiveLogFile = new System.IO.FileInfo(logFile.FullName);

                    System.IO.FileInfo[] ArchivedLogs = new DirectoryInfo(archiveLogFile.Directory.FullName)
                        .GetFiles(Path.GetFileNameWithoutExtension(archiveLogFile.FullName)
                                  + "-????????_?????????"
                                  + Path.GetExtension(archiveLogFile.FullName)
                                  + "." + _ArchiveFactory.DefaultExtension);

					for (int i = 0; i <= ArchivedLogs.Length - 1; i++) {
						if ((DateTime.Now.Subtract(ArchivedLogs[i].CreationTime).TotalDays > ExpireDays)) {
							Debug.WriteLine("Removing expire archivelog '" + ArchivedLogs[i].Name + "'");
							ArchivedLogs[i].Delete();
						}
					}
				} catch (Exception ex) {
					if (((this.AsyncExceptionHandler != null))) {
						this.AsyncExceptionHandler.Invoke(this, new Exception("Error removing old log files", ex));
					}
				}
			}
		}

		private void AddFileToZip(string zipFilename, string textFileToAdd)
		{
			ArchiveFile af = null;
			try {
				af= _ArchiveFactory.NewArchiveInstance(new System.IO.FileInfo(zipFilename));
				af.Open(ArchiveOpenMode.OpenCreate);
				af.Add(new System.IO.FileInfo(textFileToAdd));
			} catch (Exception ex) {
				throw new Exception("Error compressing log file to zip", ex);
			}
			finally{
				try{
					af.Close();
				}catch{
				}
			}
		}

		private static Object oInstance = new object();
		public static FileLoggerWriter Instance(System.IO.FileInfo logFile)
		{
			lock (oInstance)
			{
				FileLoggerWriter logRet = null;

				if ((m_Instances.ContainsKey(logFile.FullName)))
				{
					logRet = m_Instances[logFile.FullName];
				}
				else
				{
					logRet = new FileLoggerWriter(logFile);
					m_Instances.Add(logFile.FullName, logRet);
				}

				return logRet;
			}
		}

		public static FileLoggerWriter Instance()
		{
            return Instance(FileLocation.UserLogDataFolder);
		}

		public static FileLoggerWriter Instance(FileLocation location)
		{
            String fileName =Path.GetFileNameWithoutExtension(Helper.GetEntryAssembly().GetName().Name) + ".log";
            return Instance(location,fileName);
		}

		public static FileLoggerWriter Instance(FileLocation location, string fileName)
		{
            System.IO.FileInfo file = new System.IO.FileInfo(FileLocations.GetLocation(location).FullName
                                                             + Path.DirectorySeparatorChar + fileName);
            return Instance(file);
		}
	}
}