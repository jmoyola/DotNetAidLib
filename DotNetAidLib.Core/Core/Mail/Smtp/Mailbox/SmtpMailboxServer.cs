using System;
using System.IO;
using System.Threading;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Mail.Smtp.Client;

namespace DotNetAidLib.Core.Mail.Smtp.Mailbox
{
    public class SmtpMailboxServer
    {
        private readonly DirectoryChangeListener dcl = new DirectoryChangeListener();
        private int m_ConcurrentEmails;
        private readonly string m_EmailFileSufix = ".eml";
        private KeyValue<int, int> m_EmailsInDay = new KeyValue<int, int>(0, 0);
        private int m_MaxConcurrentEmailsToSend = 5;
        private int m_MaxEmailsToSendPerDay = 400;

        public SmtpMailboxServer(DirectoryInfo MailboxFolder, SmtpClient SmtpClient)
        {
            Assert.Exists(MailboxFolder, nameof(MailboxFolder));
            Assert.NotNull(SmtpClient, nameof(SmtpClient));

            this.MailboxFolder = MailboxFolder;
            this.SmtpClient = SmtpClient;
            this.SmtpClient.EmailSend += SmtpClient_EmailSend;
        }

        public DirectoryInfo MailboxFolder { get; }

        public SmtpClient SmtpClient { get; }

        public bool Started { get; private set; }

        public int MaxConcurrentEmailsToSend
        {
            get => m_MaxConcurrentEmailsToSend;
            set
            {
                Assert.BetweenOrEqual(value, 1, 50, nameof(value));
                m_MaxConcurrentEmailsToSend = value;
            }
        }

        public int MaxEmailsToSendPerDay
        {
            get => m_MaxEmailsToSendPerDay;
            set
            {
                Assert.BetweenOrEqual(value, 1, 50, nameof(value));
                m_MaxEmailsToSendPerDay = value;
            }
        }

        public event EmailSendEventHandler EmailSend;

        private void SmtpClient_EmailSend(object sender, EmailSendEventArgs args)
        {
            //  Si un email se envi�, se elimina como archivo de la carpeta
            if (args.Status.Equals(EmailSendStatus.Sending))
            {
                m_ConcurrentEmails = m_ConcurrentEmails--;
                m_EmailsInDay.Value--;
            }

            if (args.Status.Equals(EmailSendStatus.Sended))
            {
                EliminarArchivoEmailDeCarpetaMailbox(args.EmailId);
                m_ConcurrentEmails = m_ConcurrentEmails++;
                m_EmailsInDay.Value++;
            }

            if (args.Status.Equals(EmailSendStatus.SendingError))
            {
                m_ConcurrentEmails = m_ConcurrentEmails--;
                m_EmailsInDay.Value--;
            }

            OnEmailSend(args);
        }

        private void CambioEnCarpetaMailboxStart(object o)
        {
            FileInfo fi = null;
            try
            {
                fi = (FileInfo) o;
                fi.WaitUntilUnlock();
                SendEmail(fi);
            }
            catch
            {
                fi.Delete();
            }
        }

        private void SendEmail(FileInfo emlFile)
        {
            //  Esperamos a que haya correos libres en el día para envíar
            do
            {
                Thread.Sleep(500);

                // Si cambiamos de día, se reinicia el contador
                if (m_EmailsInDay.Key != DateTime.Now.Day)
                    m_EmailsInDay = new KeyValue<int, int>(DateTime.Now.Day, 0);
            } while (Started && m_EmailsInDay.Value >= m_MaxEmailsToSendPerDay);

            //  Esperamos a que haya un pool libre para enviar
            while (Started && m_ConcurrentEmails >= m_MaxConcurrentEmailsToSend) Thread.Sleep(500);

            try
            {
                if (SmtpClient != null && Started) SmtpClient.SendEMLEmail(emlFile, true);
            }
            catch (Exception ex)
            {
                SmtpClient_EmailSend(this,
                    new EmailSendEventArgs(emlFile.FullName.Substring(0, emlFile.FullName.Length - 4),
                        EmailSendStatus.SendingError, new SmtpException("Error sending email: " + ex.Message, ex)));
            }
        }

        private void EliminarArchivoEmailDeCarpetaMailbox(string emailId)
        {
            var EliminarArchivoEmailDeCarpetaMailboxThread = new Thread(EliminarArchivoEmailDeCarpetaMailboxStart);
            EliminarArchivoEmailDeCarpetaMailboxThread.Start(emailId);
        }

        private void EliminarArchivoEmailDeCarpetaMailboxStart(object o)
        {
            var emailId = (string) o;
            var fEmail = new FileInfo(MailboxFolder.FullName
                                      + Path.DirectorySeparatorChar
                                      + emailId + m_EmailFileSufix);
            fEmail.WaitUntilUnlock();
            fEmail.Delete();
        }

        public void Start()
        {
            if (Started)
                throw new SmtpMailboxException("Already started.");

            Started = true;
            if (dcl != null && dcl.Started) dcl.StopListener();

            // -
            dcl.Directory = MailboxFolder;
            // -
            dcl.Filter = "*" + m_EmailFileSufix;
            // -
            dcl.IncludeInitialContent = false;
            dcl.Interval = 1000;
            dcl.DirectoryChanged += MailboxDirectoryChanged;
            // -
            dcl.StartListener();
        }

        //  Evento que sucede en el filesystemwatcher al cambiar el estado de los archivos contenidos en la carpeta de mailbox
        private void MailboxDirectoryChanged(object sender, DirectoryChangeEventArgs args)
        {
            foreach (var fci in args.FileChangeInfoList)
                if (fci.ChangeType == WatcherChangeTypes.Created)
                {
                    var CambioEnCarpetaMailboxStartThread = new Thread(CambioEnCarpetaMailboxStart);
                    CambioEnCarpetaMailboxStartThread.Start(fci.File);
                }
        }

        public void Stop()
        {
            if (!Started)
                throw new SmtpMailboxException("Already stopped.");
            Started = false;
            dcl.StopListener();
        }

        protected void OnEmailSend(EmailSendEventArgs args)
        {
            if (EmailSend != null)
                EmailSend(this, args);
        }
    }
}