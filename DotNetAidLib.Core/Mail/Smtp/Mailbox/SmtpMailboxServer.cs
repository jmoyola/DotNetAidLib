using System;
using System.IO;
using System.Threading;
using System.Net.Mail;
using DotNetAidLib.Core.Develop;
using MimeKit;
using System.Collections.Generic;
using DotNetAidLib.Core.Mail.Smtp.Client;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Files;
using SmtpClient = DotNetAidLib.Core.Mail.Smtp.Client.SmtpClient;
using SmtpException = DotNetAidLib.Core.Mail.Smtp.Client.SmtpException;

namespace DotNetAidLib.Core.Mail.Smtp.Mailbox
{

    public class SmtpMailboxServer
    {

        private SmtpClient m_SmtpClient = null;
        private DirectoryInfo m_MailboxFolder = null;
        private string m_EmailFileSufix = ".eml";
        private int m_MaxEmailsToSendPerDay = 400;
        private int m_MaxConcurrentEmailsToSend = 5;
        private int m_ConcurrentEmails = 0;
        private KeyValue<int, int> m_EmailsInDay =new KeyValue<int, int>(0,0);
        private bool m_Started = false;
        private DirectoryChangeListener dcl = new DirectoryChangeListener ();
        public event EmailSendEventHandler EmailSend;

        public SmtpMailboxServer(DirectoryInfo MailboxFolder, SmtpClient SmtpClient){
            Assert.Exists( MailboxFolder, nameof(MailboxFolder));
            Assert.NotNull( SmtpClient, nameof(SmtpClient));

            this.m_MailboxFolder = MailboxFolder;
            this.m_SmtpClient = SmtpClient;
            m_SmtpClient.EmailSend += this.SmtpClient_EmailSend;
        }

        public DirectoryInfo MailboxFolder{
            get{
                return m_MailboxFolder;
            }
        }

        public SmtpClient SmtpClient
        {
            get{
                return m_SmtpClient;
            }
        }

        public bool Started{
            get{
                return m_Started;
            }
        }

        public int MaxConcurrentEmailsToSend{
            get{
                return m_MaxConcurrentEmailsToSend;
            }
            set{
                Assert.BetweenOrEqual(value, 1, 50, nameof(value));
                m_MaxConcurrentEmailsToSend = value;
            }
        }
        public int MaxEmailsToSendPerDay {
            get {
                return m_MaxEmailsToSendPerDay;
            }
            set {
                Assert.BetweenOrEqual (value, 1, 50, nameof (value));
                m_MaxEmailsToSendPerDay = value;
            }
        }

        private void SmtpClient_EmailSend(object sender, EmailSendEventArgs args)
        {
            //  Si un email se envi�, se elimina como archivo de la carpeta
			if (args.Status.Equals(EmailSendStatus.Sending)){
				m_ConcurrentEmails = m_ConcurrentEmails--;
                m_EmailsInDay.Value--;                
            }
            if (args.Status.Equals(EmailSendStatus.Sended)){
                this.EliminarArchivoEmailDeCarpetaMailbox(args.EmailId);
				m_ConcurrentEmails = m_ConcurrentEmails++;
                m_EmailsInDay.Value++;
            }
			if (args.Status.Equals(EmailSendStatus.SendingError))
            {
				m_ConcurrentEmails = m_ConcurrentEmails--;
                m_EmailsInDay.Value--;
            }
            this.OnEmailSend (args);
        }

        private void CambioEnCarpetaMailboxStart(Object o){
            FileInfo fi = null;
            try {
                fi = (FileInfo)o;
                fi.WaitUntilUnlock();
                this.SendEmail (fi);
            } catch {
                fi.Delete();
            }
        }

        private void SendEmail(FileInfo emlFile)
        {

            //  Esperamos a que haya correos libres en el día para envíar
            do {
                Thread.Sleep (500);

                // Si cambiamos de día, se reinicia el contador
                if (m_EmailsInDay.Key != DateTime.Now.Day)
                    m_EmailsInDay = new KeyValue<int, int> (DateTime.Now.Day, 0);
            } while (this.m_Started && m_EmailsInDay.Value >= m_MaxEmailsToSendPerDay);
                
            //  Esperamos a que haya un pool libre para enviar
            while (this.m_Started && m_ConcurrentEmails >= m_MaxConcurrentEmailsToSend) {
                Thread.Sleep (500);
            }   

            try
            {
                if (m_SmtpClient != null && this.m_Started){
                    m_SmtpClient.SendEMLEmail(emlFile, true);
                    
                }

            }
            catch (Exception ex){
				SmtpClient_EmailSend(this,
				                     new EmailSendEventArgs(emlFile.FullName.Substring(0,emlFile.FullName.Length-4), EmailSendStatus.SendingError, new SmtpException("Error sending email: " + ex.Message, ex)));
                
            }

        }

        private void EliminarArchivoEmailDeCarpetaMailbox(String emailId)
        {
            Thread EliminarArchivoEmailDeCarpetaMailboxThread = new Thread(new ParameterizedThreadStart(this.EliminarArchivoEmailDeCarpetaMailboxStart));
            EliminarArchivoEmailDeCarpetaMailboxThread.Start(emailId);
        }

        private void EliminarArchivoEmailDeCarpetaMailboxStart(Object o)
        {
            String emailId = (String)o;
            FileInfo fEmail = new FileInfo(this.m_MailboxFolder.FullName
                            + Path.DirectorySeparatorChar
                            + emailId + m_EmailFileSufix);
            fEmail.WaitUntilUnlock();
            fEmail.Delete();
        }

        public void Start(){
            if (m_Started)
                throw new SmtpMailboxException ("Already started.");

            m_Started = true;
            if (dcl != null && dcl.Started){
                dcl.StopListener();
            }

            // -
            dcl.Directory = this.MailboxFolder;
            // -
            dcl.Filter = ("*" + m_EmailFileSufix);
            // -
            dcl.IncludeInitialContent = false;
            dcl.Interval = 1000;
            dcl.DirectoryChanged += this.MailboxDirectoryChanged;
            // -
            dcl.StartListener();
        }

        //  Evento que sucede en el filesystemwatcher al cambiar el estado de los archivos contenidos en la carpeta de mailbox
        private void MailboxDirectoryChanged(object sender, DirectoryChangeEventArgs args)
        {
            foreach (FileChangeInfo fci in args.FileChangeInfoList)
            {
                if ((fci.ChangeType == WatcherChangeTypes.Created))
                {
                    Thread CambioEnCarpetaMailboxStartThread = new Thread(new ParameterizedThreadStart(this.CambioEnCarpetaMailboxStart));
                    CambioEnCarpetaMailboxStartThread.Start(fci.File);
                }

            }

        }

        public void Stop(){
            if (!m_Started)
                throw new SmtpMailboxException ("Already stopped.");
            this.m_Started = false;
            dcl.StopListener();

        }

        protected void OnEmailSend (EmailSendEventArgs args){
            if(this.EmailSend!=null)
                this.EmailSend (this, args);
        }
    }
}
