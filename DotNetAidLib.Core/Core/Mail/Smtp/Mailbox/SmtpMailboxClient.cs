using System;
using System.IO;
using System.Threading;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Mail.Core;

namespace DotNetAidLib.Core.Mail.Smtp.Mailbox
{
    public delegate void MailBoxClientSendEventHandler(object sender, MailBoxClientSendEventArgs args);

    public class MailBoxClientSendEventArgs : EventArgs
    {
        public MailBoxClientSendEventArgs(Email email)
            : this(email, null)
        {
        }

        public MailBoxClientSendEventArgs(Email email, Exception exception)
        {
            Email = email;
            Exception = exception;
        }

        public Email Email { get; }

        public Exception Exception { get; }
    }

    public class SmtpMailboxClient
    {
        private readonly string m_EmailFileSufix = ".eml";

        public SmtpMailboxClient(DirectoryInfo MailboxFolder)
        {
            Assert.Exists(MailboxFolder, nameof(MailboxFolder));

            this.MailboxFolder = MailboxFolder;
        }

        public DirectoryInfo MailboxFolder { get; }

        public event MailBoxClientSendEventHandler Sended;

        public void SendEmail(Email email)
        {
            var EncolarEmailParaRenvioThread = new Thread(SendEmailStart);
            EncolarEmailParaRenvioThread.Start(email);
            //SendEmailStart (email);
        }

        protected void OnSended(MailBoxClientSendEventArgs args)
        {
            if (Sended != null)
                Sended(this, args);
        }

        private void SendEmailStart(object o)
        {
            Email email = null;
            Exception error = null;

            try
            {
                email = (Email) o;
                email.ToEMLFile(new FileInfo(MailboxFolder.FullName
                                             + Path.DirectorySeparatorChar
                                             + email.Id + m_EmailFileSufix));
            }
            catch (Exception ex)
            {
                error = ex;
            }

            OnSended(new MailBoxClientSendEventArgs(email, error));
        }
    }
}