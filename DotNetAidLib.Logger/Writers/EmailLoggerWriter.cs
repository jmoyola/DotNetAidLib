using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

using System.Net;
using System.Text;
using System.Threading;
using System.Linq;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Logger.Core;
using DotNetAidLib.Logger.LogEntryInfo;
using DotNetAidLib.Core.Mail.Smtp.Client;
using DotNetAidLib.Core.Network.Client.Core;

namespace DotNetAidLib.Logger.Writers{
	public class EmailLoggerWriter : LoggerWriter
	{


		private List<LogEntry> LogEntriesQueue = new List<LogEntry>();

		private int _LogEntriesPerEmail = 10;
		private bool _DequeueLogEntriesOnSendMailError = false;
		private string _SmtpServer = null;
		private int _SmtpPort = 0;
		private NetworkCredential _SmtpCredentials = null;
		private SmtpSecureOptions _SmtpSsl = SmtpSecureOptions.Auto;
		private string _MailFromAddress = null;
		private string _MailToAddress = null;

		private string _MailSubject = "Log report from client '%clientInfo%' (%publicIp%/%privateIp%)";

		private Timer _SendMailQeueTimer = null;
		public EmailLoggerWriter(string smtpServer, int smtpPort, string smtpUser, string smtpPassword, SmtpSecureOptions smtpSsl, string mailFromAddress, string mailToAddress, string mailSubject)
		{
			_SmtpServer = smtpServer;
			_SmtpPort = smtpPort;
			_SmtpCredentials = new NetworkCredential(smtpUser, smtpPassword);
			_SmtpSsl = smtpSsl;
			_MailFromAddress = mailFromAddress;
			_MailToAddress = mailToAddress;

			_MailSubject = mailSubject;

			this.MaxLogPriorityVerboseInDebugTime = LogPriorityLevels._ERROR;
			this.MaxLogPriorityVerboseInRunTime = LogPriorityLevels._ERROR;

			// Creamos e inicializamos el timer de sendMailQeue
			TimerCallback tcb = new TimerCallback(this.SendMailQeueLapse);
			_SendMailQeueTimer = new Timer(tcb, null, 0, 1000 * 60);
		}
		public EmailLoggerWriter(string smtpServer, int smtpPort, string smtpUser, string smtpPassword, SmtpSecureOptions smtpSsl)
			: this(smtpServer, smtpPort, smtpUser, smtpPassword, smtpSsl, null, null, "Log report from client '%clientInfo%' (%publicIp%/%privateIp%)")
		{
		}

		public EmailLoggerWriter(string smtpServer, int smtpPort, string smtpUser, string smtpPassword, SmtpSecureOptions smtpSsl, string mailFromAddress, string mailToAddress)
			: this(smtpServer, smtpPort, smtpUser, smtpPassword, smtpSsl, mailFromAddress, mailToAddress, "Log report from client '%clientInfo%' (%publicIp%/%privateIp%)")
		{
		}

		public int LogEntriesPerEmail {
			get { return _LogEntriesPerEmail; }
			set { _LogEntriesPerEmail = value; }
		}

		public string SmtpServer {
			get { return _SmtpServer; }
			set { _SmtpServer = value; }
		}

		public int SmtpPort {
			get { return _SmtpPort; }
			set { _SmtpPort = value; }
		}

		public NetworkCredential SmtpCredentials {
			get { return _SmtpCredentials; }
			set { _SmtpCredentials = value; }
		}

		public SmtpSecureOptions SmtpSsl {
			get { return _SmtpSsl; }
			set { _SmtpSsl = value; }
		}

		public string MailFromAddress {
			get { return _MailFromAddress; }
			set { _MailFromAddress = value; }
		}

		public string MailToAddress {
			get { return _MailToAddress; }
			set { _MailToAddress = value; }
		}

		public string MailSubject {
			get { return _MailSubject; }
			set { _MailSubject = value; }
		}

		public bool DequeueLogEntriesOnSendMailError {
			get { return _DequeueLogEntriesOnSendMailError; }
			set { _DequeueLogEntriesOnSendMailError = value; }
		}

		public override void InitConfiguration(IApplicationConfigGroup configGroup)
		{
			base.InitConfiguration(configGroup);

			if ((configGroup.ConfigurationExist("SmtpServer"))) {
				_SmtpServer = configGroup.GetConfiguration<string>("SmtpServer").Value;
			} else {
				configGroup.AddConfiguration<string>("SmtpServer", _SmtpServer, true);
			}

			if ((configGroup.ConfigurationExist("SmtpPort"))) {
				_SmtpPort = configGroup.GetConfiguration<int>("SmtpPort").Value;
			} else {
				configGroup.AddConfiguration<int>("SmtpPort", _SmtpPort, true);
			}

			if ((configGroup.ConfigurationExist("SmtpSsl"))) {
				try{
					_SmtpSsl = configGroup.GetConfiguration<SmtpSecureOptions>("SmtpSsl").Value;
				}
				catch{
					configGroup.AddConfiguration<SmtpSecureOptions>("SmtpSsl", _SmtpSsl, true);
				}
			} else {
				configGroup.AddConfiguration<SmtpSecureOptions>("SmtpSsl", _SmtpSsl, true);
			}

			if ((configGroup.ConfigurationExist("SmtpCredentials"))) {
				_SmtpCredentials = configGroup.GetConfiguration<NetworkCredential>("SmtpCredentials").Value;
			} else {
				configGroup.AddConfiguration<NetworkCredential>("SmtpCredentials", _SmtpCredentials, true);
			}

			if ((configGroup.ConfigurationExist("MailFromAddress"))) {
				_MailFromAddress = configGroup.GetConfiguration<string>("MailFromAddress").Value;
			} else {
				configGroup.AddConfiguration<string>("MailFromAddress", _MailFromAddress, true);
			}

			if ((configGroup.ConfigurationExist("MailToAddress"))) {
				_MailToAddress = configGroup.GetConfiguration<string>("MailToAddress").Value;
			} else {
				configGroup.AddConfiguration<string>("MailToAddress", _MailToAddress, true);
			}

			if ((configGroup.ConfigurationExist("MailSubject"))) {
				_MailSubject = configGroup.GetConfiguration<string>("MailSubject").Value;
			} else {
				configGroup.AddConfiguration<string>("MailSubject", _MailSubject, true);
			}

			if ((configGroup.ConfigurationExist("LogEntriesPerEmail"))) {
				_LogEntriesPerEmail = configGroup.GetConfiguration<int>("LogEntriesPerEmail").Value;
			} else {
				configGroup.AddConfiguration<int>("LogEntriesPerEmail", _LogEntriesPerEmail, true);
			}

			if ((configGroup.ConfigurationExist("DequeueLogEntriesOnSendMailError"))) {
				_DequeueLogEntriesOnSendMailError = configGroup.GetConfiguration<bool>("DequeueLogEntriesOnSendMailError").Value;
			} else {
				configGroup.AddConfiguration<bool>("DequeueLogEntriesOnSendMailError", _DequeueLogEntriesOnSendMailError, true);
			}
		}

		public override void SaveConfiguration(IApplicationConfigGroup configGroup)
		{
			base.SaveConfiguration(configGroup);

			configGroup.AddConfiguration<string>("SmtpServer", _SmtpServer, true);

			configGroup.AddConfiguration<int>("SmtpPort", _SmtpPort, true);

			configGroup.AddConfiguration<SmtpSecureOptions>("SmtpSsl", _SmtpSsl, true);

			configGroup.AddConfiguration<NetworkCredential>("SmtpCredentials", _SmtpCredentials, true);

			configGroup.AddConfiguration<string>("MailFromAddress", _MailFromAddress, true);

			configGroup.AddConfiguration<string>("MailToAddress", _MailToAddress, true);

			configGroup.AddConfiguration<string>("MailSubject", _MailSubject, true);

			configGroup.AddConfiguration<int>("LogEntriesPerEmail", _LogEntriesPerEmail, true);

			configGroup.AddConfiguration<bool>("DequeueLogEntriesOnSendMailError", _DequeueLogEntriesOnSendMailError, true);
		}


		public override void WriteLog(LogEntry logEntry)
		{
			try {
				LogEntriesQueue.Add(logEntry);
			} catch (Exception ex) {
				throw new LoggerException("Error writing log to email", ex, logEntry);
			}
		}

		~EmailLoggerWriter()
		{
			// Si se cierra la aplicación

			// Finalizamos el timer
			_SendMailQeueTimer.Dispose();

			// Se envian todos los de la cola
			while ((LogEntriesQueue.Count > 0)) {
				SendLogEntriesQueue(-1);
			}

		}

		private void SendMailQeueLapse(object state)
		{
			if ((LogEntriesQueue.Count >= _LogEntriesPerEmail)) {
				Thread SendLogEntriesQueue_Thread = new Thread(new ParameterizedThreadStart(SendLogEntriesQueue));
				SendLogEntriesQueue_Thread.Start(_LogEntriesPerEmail);
			}
		}

		private static object oSendLogEntriesQueue = new object();
		private void SendLogEntriesQueue(Object logEntriesPerEmail)
		{
			lock (oSendLogEntriesQueue)
			{
				int iLogEntriesPerEmail = (int)logEntriesPerEmail;
				IEnumerable<LogEntry> logsEntriesToSend = null;
				bool sendError = false;
				try
				{
					// Si hay alguna entrada de log en la cola
					if ((LogEntriesQueue.Count > 0))
					{
						// Se crea el cuerpo del email en base a los mensajes a mandar
						StringBuilder body = new StringBuilder();

						// Se envia toda la cola pendiente
						if ((iLogEntriesPerEmail == -1))
						{
							logsEntriesToSend = LogEntriesQueue;
							// Se envia lo estipulado por email
						}
						else
						{
							logsEntriesToSend = LogEntriesQueue.Take(iLogEntriesPerEmail);
						}

						foreach (LogEntry logsEntryToSend in logsEntriesToSend.OrderBy<LogEntry, DateTime>(v => v.Instant))
						{
							body.Append("\r\n" + logsEntryToSend.ToString());
						}

						IPAddress privateIp = IPProviderFactory.Instance().GetPrivate();                 
						IPAddress publicIp = IPProviderFactory.Instance().GetPublicCached();

						string subject = _MailSubject;
                        ClientLogEntryInfo clientLogEntryInfo = (ClientLogEntryInfo)LogEntriesQueue[0].LogEntryInformation.FirstOrDefault(v=>v is ClientLogEntryInfo);

                        if(clientLogEntryInfo!=null)
                            subject = subject.Replace("%clientInfo%", clientLogEntryInfo.ClientInfo);
						subject = subject.Replace("%publicIp%", (publicIp==null?"UNKNOW":publicIp.ToString()));
						subject=subject.Replace("%privateIp%", (privateIp == null ? "UNKNOW" : privateIp.ToString()));

						SendMail(subject, body.ToString());

					}
				}
				catch (Exception ex)
				{
					if (((this.AsyncExceptionHandler != null)))
					{
						this.AsyncExceptionHandler.Invoke(this, new LoggerException("Error sending mail queue", ex));
					}
					sendError = true;

				}
				finally
				{
					// Eliminamos las entradas de logs enviadas de la cola desde el último al primero si procede
					if ((!sendError || (_DequeueLogEntriesOnSendMailError & sendError)))
					{
						// Si se mandó el email, eliminamos las entradas de logs enviadas de la cola desde el último al primero
						foreach (LogEntry logsEntryToSend in logsEntriesToSend.Reverse())
						{
							LogEntriesQueue.Remove(logsEntryToSend);
						}
					}
				}
			}
		}

		private void SendMail(string subject, string body)
		{
            DotNetAidLib.Core.Mail.Smtp.Client.SmtpClient coreStmpClient=new DotNetAidLib.Core.Mail.Smtp.Client.SmtpClient(_SmtpServer, _SmtpPort);
			coreStmpClient.SecureConfig.SecureOptions=SmtpSsl;
			coreStmpClient.SecureConfig.SmtpCredentials = SmtpCredentials;


            DotNetAidLib.Core.Mail.Core.Email mail = new DotNetAidLib.Core.Mail.Core.Email ();
			mail.Sender=_MailFromAddress;
			mail.Receivers.Add(_MailToAddress);
			mail.Subject = subject;
			mail.Body=body;

			coreStmpClient.SendEmail(mail);
		}

		public override string ToString()
		{
			return this.GetType().Name + ": " + this.SmtpServer + ":" + this.SmtpPort + " ssl (" + this.SmtpSsl.ToString()+") > " + this.MailToAddress;
		}

	}
}