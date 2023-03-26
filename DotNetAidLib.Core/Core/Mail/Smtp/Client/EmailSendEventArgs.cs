using System;

namespace DotNetAidLib.Core.Mail.Smtp.Client
{
    public enum EmailSendStatus
    {
        Sending,
        Sended,
        SendingError
    }

    public class EmailSendEventArgs : EventArgs
    {
        public EmailSendEventArgs(string emailId, EmailSendStatus status, SmtpException error)
        {
            EmailId = emailId;
            Status = status;
            Error = error;
        }

        public string EmailId { get; }

        public EmailSendStatus Status { get; }

        public SmtpException Error { get; }
    }
}