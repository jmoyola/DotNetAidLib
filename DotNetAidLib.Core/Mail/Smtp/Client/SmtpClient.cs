using System;
using MailKit.Net.Smtp;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Authentication;
using DotNetAidLib.Core.MIME;
using MimeKit;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNetAidLib.Core.Mail.Core;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Enums;
using MailKit.Security;
using MailKit;

namespace DotNetAidLib.Core.Mail.Smtp.Client
{
    public enum SmtpSecureOptions
    {
        None = 0,
        Auto = 1,
        SslOnConnect = 2,
        StartTls = 3,
        StartTlsWhenAvailable = 4
    }

    [Flags]
    public enum SmtpValidSecureOptions
    {
        Default = Tls|Ssl3,
        All=Ssl2|Ssl3|Tls|Tls11|Tls12,
        None = 0,
        Ssl2 = 12,
        Ssl3 = 48,
        Tls = 192,
        Tls11 = 768,
        Tls12 = 3072,
    }
    
    public class SmtpSecureConfig
    {
        private SmtpSecureOptions m_SecureOptions = SmtpSecureOptions.Auto;
        private SmtpValidSecureOptions m_ValidSecureOptions = SmtpValidSecureOptions.Default;
        private NetworkCredential m_SmtpCredentials = null;
        private RemoteCertificateValidationCallback serverCertificateValidationCallback=(s, c, h, e) => true;
        private bool checkCertificateRevocation = false;
        private X509CertificateCollection clientCertificates = new X509CertificateCollection();
            
        public SmtpSecureOptions SecureOptions
        {
            get{
                return m_SecureOptions;
            }
            set{
                m_SecureOptions = value;
            }
        }

        public SmtpValidSecureOptions ValidSecureOptions
        {
            get{
                return m_ValidSecureOptions;
            }
            set{
                m_ValidSecureOptions = value;
            }
        }
        public NetworkCredential SmtpCredentials
        {
            get { return m_SmtpCredentials; }
            set { m_SmtpCredentials = value; }
        }

        public RemoteCertificateValidationCallback ServerCertificateValidationCallback
        {
            get => serverCertificateValidationCallback;
            set
            {
                Assert.NotNull( value, nameof(value));
                serverCertificateValidationCallback = value;
            }
        }

        public bool CheckCertificateRevocation
        {
            get => checkCertificateRevocation;
            set => checkCertificateRevocation = value;
        }

        public X509CertificateCollection ClientCertificates
        {
            get => clientCertificates;
        }
    }
    
    //  Delegado para el evento de estado de email
    public delegate void EmailSendEventHandler(object source, EmailSendEventArgs args);
    public class SmtpClient
    {
        

        //  Evento de estado de email en envio as�ncrono
        public event EmailSendEventHandler EmailSend;

        private string host;
        private int port;
        
		private SmtpSecureConfig secureConfig = new SmtpSecureConfig(){SecureOptions=SmtpSecureOptions.Auto};
        private int m_Timeout = 30000;

        public SmtpClient()
        { }

		public SmtpClient(string host, int port)
            : this(host, port, new SmtpSecureConfig(){SecureOptions = SmtpSecureOptions.Auto}) { }
        
		public SmtpClient(string host, int port, SmtpSecureConfig secureConfig)
        {
            this.Host = host;
            this.Port = port;
            this.SecureConfig = secureConfig;
        }

        public string Host{
            get{
                return host;
            }
            set{
                Assert.NotNullOrEmpty( value, nameof(value));
                host = value;
            }
        }

        public int Port{
            get{
                return port;
            }
            set{
                Assert.GreaterThan(value, 0, nameof(value));
                port = value;
            }
        }

        public SmtpSecureConfig SecureConfig{
            get{
                return secureConfig;
            }
            set{
                Assert.NotNull( value, nameof(value));
                secureConfig = value;
            }
        }
        
        public int Timeout{
            get{
                return m_Timeout;
            }
            set{
                m_Timeout = value;
            }
        }
        
        public void SendEmail(Email email){
            this.SendEmail(email, false);
        }

        public void SendEmail(Email email, bool async){
            try
            {
                MailKit.Net.Smtp.SmtpClient smtpClient = this.NewSmtpClient();
                MimeMessage msg = email.ToMIMEMessage();
                if (async)
                    smtpClient.SendAsync (msg);
                else
                    smtpClient.Send(msg);
                
                this.OnEmailSend (new EmailSendEventArgs (msg.MessageId, EmailSendStatus.Sending, null));
            }
            catch (Exception ex){
                SmtpException smtpException = new SmtpException("Error sending message '" + email.Id + "': " + ex.Message, ex);

                if (async)
                    this.OnEmailSend(new EmailSendEventArgs(email.Id, EmailSendStatus.SendingError, smtpException));
                else
                    throw smtpException;
            }
        }

        public void SendEMLEmail (FileInfo emlEmail, bool async)
        {
            MailKit.Net.Smtp.SmtpClient smtpClient = null;
            MimeMessage msg = null;
            try
            {
                smtpClient = this.NewSmtpClient();
                msg = MimeMessage.Load(emlEmail.FullName);
                this.OnEmailSend(new EmailSendEventArgs(msg.MessageId, EmailSendStatus.Sending, null));
                if (async)
                    smtpClient.SendAsync(msg);
                else
                {
                    smtpClient.Send(msg);
                    this.OnEmailSend(new EmailSendEventArgs(msg.MessageId, EmailSendStatus.Sended, null));
                }
            }
            catch (Exception ex)
            {
                SmtpException smtpException =
                    new SmtpException("Error sending message '" + msg.MessageId + "': " + ex.Message, ex);

                if (async)
                    this.OnEmailSend(new EmailSendEventArgs(msg.MessageId, EmailSendStatus.SendingError,
                        smtpException));
                else
                    throw smtpException;
            }
        }

        private MailKit.Net.Smtp.SmtpClient NewSmtpClient()
        {
			MailKit.Net.Smtp.SmtpClient coreStmpClient = new MailKit.Net.Smtp.SmtpClient();
            coreStmpClient.ClientCertificates = this.secureConfig.ClientCertificates;
            coreStmpClient.ServerCertificateValidationCallback=this.secureConfig.ServerCertificateValidationCallback;
            coreStmpClient.CheckCertificateRevocation = this.secureConfig.CheckCertificateRevocation;
            coreStmpClient.SslProtocols = (SslProtocols)this.secureConfig.ValidSecureOptions;
            
			coreStmpClient.Connect(this.Host, this.Port, this.secureConfig.SecureOptions.ToString().ToEnum<SecureSocketOptions>(true));

            //  Necesario para la admisi�n de certificadoawait client.SendAsync(emailMessage);s en ssl
            // ServicePointManager.ServerCertificateValidationCallback = AddressOf FDevuelveOk
            if (this.secureConfig.SmtpCredentials!=null){
				coreStmpClient.Authenticate(this.secureConfig.SmtpCredentials);
            }

            coreStmpClient.Timeout = this.Timeout;
            coreStmpClient.MessageSent += MessageSendEvent;
            return coreStmpClient;
        }

        private void MessageSendEvent(object sender, MailKit.MessageSentEventArgs e){
            MailKit.Net.Smtp.SmtpClient smtpClient = (MailKit.Net.Smtp.SmtpClient)sender;
            this.OnEmailSend(new EmailSendEventArgs(e.Message.MessageId, EmailSendStatus.Sended, null));
            smtpClient.Disconnect (true);
            smtpClient.Dispose();  
        }

        protected void OnEmailSend (EmailSendEventArgs args) {
            if(this.EmailSend!=null)
                this.EmailSend (this, args);
        }
    }
}
