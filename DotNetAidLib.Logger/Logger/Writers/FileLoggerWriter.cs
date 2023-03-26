using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.IO.Archive.Core;
using DotNetAidLib.Core.IO.Archive.Zip;
using DotNetAidLib.Logger.Core;

namespace DotNetAidLib.Logger.Writers
{
    public class FileLoggerWriter : LoggerWriter
    {
        private static readonly Dictionary<string, FileLoggerWriter> m_Instances =
            new Dictionary<string, FileLoggerWriter>();

        private static readonly object oInstance = new object();

        private readonly IArchiveFactory _ArchiveFactory;

        private readonly Timer m_TimerClearOldLogs;

        private readonly object oWriteLog = new object();

        private readonly object sLockArchivado = new object();

        private readonly object sLockCleanOldLogs = new object();

        private FileLoggerWriter(FileInfo logFile)
        {
            _ArchiveFactory = ZipArchiveFactory.Instance();

            if (logFile == null) throw new NullReferenceException("logfile parameter can't be null.");

            LogFile = logFile;

            // Si no existe la carpeta base, la creamos (y todo el arbol hasta ella)
            if (!logFile.Directory.Exists) logFile.Directory.CreateAll();

            // Creamos e inicializamos el timer de clearoldlogs
            TimerCallback tcb = TimerClearOldLogs;
            m_TimerClearOldLogs = new Timer(tcb, null, 0, 1000 * 60 * 5);
        }

        public FileInfo LogFile { get; }

        public long MaxLogSize { get; set; } = 1 * 1024 * 1024;

        public int ArchivedLogsExpireDays { get; set; } = 30;

        private void TimerClearOldLogs(object state)
        {
            // Si los dias de caducidad son 0, no caducan nunca
            if (ArchivedLogsExpireDays > 0)
                //Borramos archivedlogs caducados
                CleanOldLogs(LogFile, ArchivedLogsExpireDays);
        }

        public override void InitConfiguration(IApplicationConfigGroup configGroup)
        {
            base.InitConfiguration(configGroup);

            if (configGroup.ConfigurationExist("MaxLogSize"))
                MaxLogSize = configGroup.GetConfiguration<long>("MaxLogSize").Value;
            else
                configGroup.AddConfiguration("MaxLogSize", MaxLogSize, true);

            if (configGroup.ConfigurationExist("ArchivedLogsExpireDays"))
                ArchivedLogsExpireDays = configGroup.GetConfiguration<int>("ArchivedLogsExpireDays").Value;
            else
                configGroup.AddConfiguration("ArchivedLogsExpireDays", ArchivedLogsExpireDays, true);
        }

        public override void SaveConfiguration(IApplicationConfigGroup configGroup)
        {
            base.SaveConfiguration(configGroup);


            configGroup.AddConfiguration("MaxLogSize", MaxLogSize, true);

            configGroup.AddConfiguration("ArchivedLogsExpireDays", ArchivedLogsExpireDays, true);
        }

        public override void WriteLog(LogEntry logEntry)
        {
            lock (oWriteLog)
            {
                WriteInLogFile(LogFile, logEntry);
            }
        }

        ~FileLoggerWriter()
        {
            m_TimerClearOldLogs.Dispose();
        }


        private void WriteInLogFile(FileInfo logFile, LogEntry logEntry)
        {
            try
            {
                // Restauramos el puntero, ya que se pierde y apunta al anterior archivo renombrado
                // dando datos erroneos de existe y tamaño
                logFile.Refresh();


                // Verificamos si hay que archivar el log actual


                if (logFile.Exists && logFile.Length > MaxLogSize)
                {
                    // Archivamos el log

                    var sLogFile = logFile.FullName;

                    var archiveLogFile = new FileInfo(
                        logFile.Directory.FullName + Path.DirectorySeparatorChar
                                                   + Path.GetFileNameWithoutExtension(logFile.FullName)
                                                   + "-" + DateTime.Now.ToString("yyyyMMdd_HHmmssfff")
                                                   + Path.GetExtension(logFile.FullName)
                    );

                    File.Move(logFile.FullName, archiveLogFile.FullName);

                    logFile = new FileInfo(sLogFile);
                    // Llamamos al roll del archivo de log en un  hilo aparte
                    var hilo = new Thread(CompressAndDeleteSourceInZip_ThreadStart);
                    hilo.Start(archiveLogFile);
                }

                // Escribimos el log
                File.AppendAllText(logFile.FullName, "\r\n" + logEntry);
            }
            catch (Exception ex)
            {
                throw new LoggerException("Error writing log to file", ex, logEntry);
            }
        }

        private void CompressAndDeleteSourceInZip_ThreadStart(object oSourceFile)
        {
            lock (sLockArchivado)
            {
                var sourceFile = (FileInfo) oSourceFile;
                try
                {
                    AddFileToZip(sourceFile.FullName + "." + _ArchiveFactory.DefaultExtension, sourceFile.FullName);
                    sourceFile.Delete();
                }
                catch (Exception ex)
                {
                    File.AppendAllText(sourceFile.FullName, ExceptionToString(ex, ExceptionVerbosityLevels._HIGH));
                    if (AsyncExceptionHandler != null)
                        AsyncExceptionHandler.Invoke(this, new Exception("Error in compress and delete log file", ex));
                }
            }
        }

        private void CleanOldLogs(FileInfo logFile, int ExpireDays)
        {
            lock (sLockCleanOldLogs)
            {
                try
                {
                    var archiveLogFile = new FileInfo(logFile.FullName);

                    var ArchivedLogs = new DirectoryInfo(archiveLogFile.Directory.FullName)
                        .GetFiles(Path.GetFileNameWithoutExtension(archiveLogFile.FullName)
                                  + "-????????_?????????"
                                  + Path.GetExtension(archiveLogFile.FullName)
                                  + "." + _ArchiveFactory.DefaultExtension);

                    for (var i = 0; i <= ArchivedLogs.Length - 1; i++)
                        if (DateTime.Now.Subtract(ArchivedLogs[i].CreationTime).TotalDays > ExpireDays)
                        {
                            Debug.WriteLine("Removing expire archivelog '" + ArchivedLogs[i].Name + "'");
                            ArchivedLogs[i].Delete();
                        }
                }
                catch (Exception ex)
                {
                    if (AsyncExceptionHandler != null)
                        AsyncExceptionHandler.Invoke(this, new Exception("Error removing old log files", ex));
                }
            }
        }

        private void AddFileToZip(string zipFilename, string textFileToAdd)
        {
            ArchiveFile af = null;
            try
            {
                af = _ArchiveFactory.NewArchiveInstance(new FileInfo(zipFilename));
                af.Open(ArchiveOpenMode.OpenCreate);
                af.Add(new FileInfo(textFileToAdd));
            }
            catch (Exception ex)
            {
                throw new Exception("Error compressing log file to zip", ex);
            }
            finally
            {
                try
                {
                    af.Close();
                }
                catch
                {
                }
            }
        }

        public static FileLoggerWriter Instance(FileInfo logFile)
        {
            lock (oInstance)
            {
                FileLoggerWriter logRet = null;

                if (m_Instances.ContainsKey(logFile.FullName))
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
            var fileName = Path.GetFileNameWithoutExtension(Helper.GetEntryAssembly().GetName().Name) + ".log";
            return Instance(location, fileName);
        }

        public static FileLoggerWriter Instance(FileLocation location, string fileName)
        {
            var file = new FileInfo(FileLocations.GetLocation(location).FullName
                                    + Path.DirectorySeparatorChar + fileName);
            return Instance(file);
        }
    }
}