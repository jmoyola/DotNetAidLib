using System;
namespace DotNetAidLib.Core.Mail.Smtp.Client
{
    public enum EmailSendStatus{
        Sending,
        Sended,
        SendingError,
    }

    public class EmailSendEventArgs : EventArgs{

        private String m_EmailId = null;
        private EmailSendStatus m_Status;
        private SmtpException m_Error = null;


        public EmailSendEventArgs(String emailId, EmailSendStatus status, SmtpException error){
            this.m_EmailId = emailId;
            this.m_Status = status;
            this.m_Error = error;
        }

        public String EmailId{
            get{
                return m_EmailId;
            }
        }

        public EmailSendStatus Status{
            get{
                return m_Status;
            }
        }

        public SmtpException Error{
            get{
                return m_Error;
            }
        }
    }
}
