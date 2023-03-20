using System;
using System.IO;
using System.Threading;
using DotNetAidLib.Core.Mail.Core;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using Org.BouncyCastle.X509.Extension;

namespace DotNetAidLib.Core.Mail.Smtp.Mailbox
{
    public delegate void MailBoxClientSendEventHandler(Object sender, MailBoxClientSendEventArgs args);
    public class MailBoxClientSendEventArgs : EventArgs{
        private Email email;
        private Exception exception=null;
        public MailBoxClientSendEventArgs(Email email)
            :this(email, null) {}

        public MailBoxClientSendEventArgs(Email email, Exception exception) {
            this.email = email;
            this.exception = exception;
        }
        public Email Email { get => this.email; }
        public Exception Exception { get => this.exception; }
    }
    
    public class SmtpMailboxClient
    {
        private DirectoryInfo m_MailboxFolder = null;
        private string m_EmailFileSufix = ".eml";
        public event MailBoxClientSendEventHandler Sended;
        
        public SmtpMailboxClient(DirectoryInfo MailboxFolder){
            Assert.Exists( MailboxFolder, nameof(MailboxFolder));

            this.m_MailboxFolder = MailboxFolder;
        }

        public DirectoryInfo MailboxFolder{
            get{
                return m_MailboxFolder;
            }
        }

        public void SendEmail(Email email)
        {
            Thread EncolarEmailParaRenvioThread = new Thread(new ParameterizedThreadStart(this.SendEmailStart));
            EncolarEmailParaRenvioThread.Start(email);
            //SendEmailStart (email);
        }

        protected void OnSended(MailBoxClientSendEventArgs args) {
            if (this.Sended != null)
                this.Sended(this, args);
        }

        private void SendEmailStart(Object o){
            Email email = null;
            Exception error = null;

            try
            {
                email = (Email)o;
                email.ToEMLFile(new FileInfo(m_MailboxFolder.FullName
                                             + Path.DirectorySeparatorChar
                                             + email.Id + m_EmailFileSufix));
            }
            catch (Exception ex)
            {
                error = ex;
            }

            this.OnSended(new MailBoxClientSendEventArgs(email, error));
        }


    }
}
