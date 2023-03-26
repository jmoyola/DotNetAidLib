using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Core.Mail.Core;
using DotNetAidLib.Core.Mail.Smtp.Client;
using DotNetAidLib.Core.Network.Client.Core;
using DotNetAidLib.Logger.Core;
using DotNetAidLib.Logger.LogEntryInfo;

namespace DotNetAidLib.Logger.Writers
{
    public class EmailLoggerWriter : LoggerWriter
    {
        private static readonly object oSendLogEntriesQueue = new object();

        private readonly Timer _SendMailQeueTimer;


        private readonly List<LogEntry> LogEntriesQueue = new List<LogEntry>();

        public EmailLoggerWriter(string smtpServer, int smtpPort, string smtpUser, string smtpPassword,
            SmtpSecureOptions smtpSsl, string mailFromAddress, string mailToAddress, string mailSubject)
        {
            SmtpServer = smtpServer;
            SmtpPort = smtpPort;
            SmtpCredentials = new NetworkCredential(smtpUser, smtpPassword);
            SmtpSsl = smtpSsl;
            MailFromAddress = mailFromAddress;
            MailToAddress = mailToAddress;

            MailSubject = mailSubject;

            MaxLogPriorityVerboseInDebugTime = LogPriorityLevels._ERROR;
            MaxLogPriorityVerboseInRunTime = LogPriorityLevels._ERROR;

            // Creamos e inicializamos el timer de sendMailQeue
            TimerCallback tcb = SendMailQeueLapse;
            _SendMailQeueTimer = new Timer(tcb, null, 0, 1000 * 60);
        }

        public EmailLoggerWriter(string smtpServer, int smtpPort, string smtpUser, string smtpPassword,
            SmtpSecureOptions smtpSsl)
            : this(smtpServer, smtpPort, smtpUser, smtpPassword, smtpSsl, null, null,
                "Log report from client '%clientInfo%' (%publicIp%/%privateIp%)")
        {
        }

        public EmailLoggerWriter(string smtpServer, int smtpPort, string smtpUser, string smtpPassword,
            SmtpSecureOptions smtpSsl, string mailFromAddress, string mailToAddress)
            : this(smtpServer, smtpPort, smtpUser, smtpPassword, smtpSsl, mailFromAddress, mailToAddress,
                "Log report from client '%clientInfo%' (%publicIp%/%privateIp%)")
        {
        }

        public int LogEntriesPerEmail { get; set; } = 10;

        public string SmtpServer { get; set; }

        public int SmtpPort { get; set; }

        public NetworkCredential SmtpCredentials { get; set; }

        public SmtpSecureOptions SmtpSsl { get; set; } = SmtpSecureOptions.Auto;

        public string MailFromAddress { get; set; }

        public string MailToAddress { get; set; }

        public string MailSubject { get; set; } = "Log report from client '%clientInfo%' (%publicIp%/%privateIp%)";

        public bool DequeueLogEntriesOnSendMailError { get; set; }

        public override void InitConfiguration(IApplicationConfigGroup configGroup)
        {
            base.InitConfiguration(configGroup);

            if (configGroup.ConfigurationExist("SmtpServer"))
                SmtpServer = configGroup.GetConfiguration<string>("SmtpServer").Value;
            else
                configGroup.AddConfiguration("SmtpServer", SmtpServer, true);

            if (configGroup.ConfigurationExist("SmtpPort"))
                SmtpPort = configGroup.GetConfiguration<int>("SmtpPort").Value;
            else
                configGroup.AddConfiguration("SmtpPort", SmtpPort, true);

            if (configGroup.ConfigurationExist("SmtpSsl"))
                try
                {
                    SmtpSsl = configGroup.GetConfiguration<SmtpSecureOptions>("SmtpSsl").Value;
                }
                catch
                {
                    configGroup.AddConfiguration("SmtpSsl", SmtpSsl, true);
                }
            else
                configGroup.AddConfiguration("SmtpSsl", SmtpSsl, true);

            if (configGroup.ConfigurationExist("SmtpCredentials"))
                SmtpCredentials = configGroup.GetConfiguration<NetworkCredential>("SmtpCredentials").Value;
            else
                configGroup.AddConfiguration("SmtpCredentials", SmtpCredentials, true);

            if (configGroup.ConfigurationExist("MailFromAddress"))
                MailFromAddress = configGroup.GetConfiguration<string>("MailFromAddress").Value;
            else
                configGroup.AddConfiguration("MailFromAddress", MailFromAddress, true);

            if (configGroup.ConfigurationExist("MailToAddress"))
                MailToAddress = configGroup.GetConfiguration<string>("MailToAddress").Value;
            else
                configGroup.AddConfiguration("MailToAddress", MailToAddress, true);

            if (configGroup.ConfigurationExist("MailSubject"))
                MailSubject = configGroup.GetConfiguration<string>("MailSubject").Value;
            else
                configGroup.AddConfiguration("MailSubject", MailSubject, true);

            if (configGroup.ConfigurationExist("LogEntriesPerEmail"))
                LogEntriesPerEmail = configGroup.GetConfiguration<int>("LogEntriesPerEmail").Value;
            else
                configGroup.AddConfiguration("LogEntriesPerEmail", LogEntriesPerEmail, true);

            if (configGroup.ConfigurationExist("DequeueLogEntriesOnSendMailError"))
                DequeueLogEntriesOnSendMailError =
                    configGroup.GetConfiguration<bool>("DequeueLogEntriesOnSendMailError").Value;
            else
                configGroup.AddConfiguration("DequeueLogEntriesOnSendMailError", DequeueLogEntriesOnSendMailError,
                    true);
        }

        public override void SaveConfiguration(IApplicationConfigGroup configGroup)
        {
            base.SaveConfiguration(configGroup);

            configGroup.AddConfiguration("SmtpServer", SmtpServer, true);

            configGroup.AddConfiguration("SmtpPort", SmtpPort, true);

            configGroup.AddConfiguration("SmtpSsl", SmtpSsl, true);

            configGroup.AddConfiguration("SmtpCredentials", SmtpCredentials, true);

            configGroup.AddConfiguration("MailFromAddress", MailFromAddress, true);

            configGroup.AddConfiguration("MailToAddress", MailToAddress, true);

            configGroup.AddConfiguration("MailSubject", MailSubject, true);

            configGroup.AddConfiguration("LogEntriesPerEmail", LogEntriesPerEmail, true);

            configGroup.AddConfiguration("DequeueLogEntriesOnSendMailError", DequeueLogEntriesOnSendMailError, true);
        }


        public override void WriteLog(LogEntry logEntry)
        {
            try
            {
                LogEntriesQueue.Add(logEntry);
            }
            catch (Exception ex)
            {
                throw new LoggerException("Error writing log to email", ex, logEntry);
            }
        }

        ~EmailLoggerWriter()
        {
            // Si se cierra la aplicación

            // Finalizamos el timer
            _SendMailQeueTimer.Dispose();

            // Se envian todos los de la cola
            while (LogEntriesQueue.Count > 0) SendLogEntriesQueue(-1);
        }

        private void SendMailQeueLapse(object state)
        {
            if (LogEntriesQueue.Count >= LogEntriesPerEmail)
            {
                var SendLogEntriesQueue_Thread = new Thread(SendLogEntriesQueue);
                SendLogEntriesQueue_Thread.Start(LogEntriesPerEmail);
            }
        }

        private void SendLogEntriesQueue(object logEntriesPerEmail)
        {
            lock (oSendLogEntriesQueue)
            {
                var iLogEntriesPerEmail = (int) logEntriesPerEmail;
                IEnumerable<LogEntry> logsEntriesToSend = null;
                var sendError = false;
                try
                {
                    // Si hay alguna entrada de log en la cola
                    if (LogEntriesQueue.Count > 0)
                    {
                        // Se crea el cuerpo del email en base a los mensajes a mandar
                        var body = new StringBuilder();

                        // Se envia toda la cola pendiente
                        if (iLogEntriesPerEmail == -1)
                            logsEntriesToSend = LogEntriesQueue;
                        // Se envia lo estipulado por email
                        else
                            logsEntriesToSend = LogEntriesQueue.Take(iLogEntriesPerEmail);

                        foreach (var logsEntryToSend in logsEntriesToSend.OrderBy(v => v.Instant))
                            body.Append("\r\n" + logsEntryToSend);

                        var privateIp = IPProviderFactory.Instance().GetPrivate();
                        var publicIp = IPProviderFactory.Instance().GetPublicCached();

                        var subject = MailSubject;
                        var clientLogEntryInfo = (ClientLogEntryInfo) LogEntriesQueue[0].LogEntryInformation
                            .FirstOrDefault(v => v is ClientLogEntryInfo);

                        if (clientLogEntryInfo != null)
                            subject = subject.Replace("%clientInfo%", clientLogEntryInfo.ClientInfo);
                        subject = subject.Replace("%publicIp%", publicIp == null ? "UNKNOW" : publicIp.ToString());
                        subject = subject.Replace("%privateIp%", privateIp == null ? "UNKNOW" : privateIp.ToString());

                        SendMail(subject, body.ToString());
                    }
                }
                catch (Exception ex)
                {
                    if (AsyncExceptionHandler != null)
                        AsyncExceptionHandler.Invoke(this, new LoggerException("Error sending mail queue", ex));
                    sendError = true;
                }
                finally
                {
                    // Eliminamos las entradas de logs enviadas de la cola desde el último al primero si procede
                    if (!sendError || DequeueLogEntriesOnSendMailError & sendError)
                        // Si se mandó el email, eliminamos las entradas de logs enviadas de la cola desde el último al primero
                        foreach (var logsEntryToSend in logsEntriesToSend.Reverse())
                            LogEntriesQueue.Remove(logsEntryToSend);
                }
            }
        }

        private void SendMail(string subject, string body)
        {
            var coreStmpClient = new SmtpClient(SmtpServer, SmtpPort);
            coreStmpClient.SecureConfig.SecureOptions = SmtpSsl;
            coreStmpClient.SecureConfig.SmtpCredentials = SmtpCredentials;


            var mail = new Email();
            mail.Sender = MailFromAddress;
            mail.Receivers.Add(MailToAddress);
            mail.Subject = subject;
            mail.Body = body;

            coreStmpClient.SendEmail(mail);
        }

        public override string ToString()
        {
            return GetType().Name + ": " + SmtpServer + ":" + SmtpPort + " ssl (" + SmtpSsl + ") > " + MailToAddress;
        }
    }
}