using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Enums;
using DotNetAidLib.Core.Mail.Core;
using MailKit;
using MailKit.Security;
using MimeKit;

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
        Default = Tls | Ssl3,
        All = Ssl2 | Ssl3 | Tls | Tls11 | Tls12,
        None = 0,
        Ssl2 = 12,
        Ssl3 = 48,
        Tls = 192,
        Tls11 = 768,
        Tls12 = 3072
    }

    public class SmtpSecureConfig
    {
        private RemoteCertificateValidationCallback serverCertificateValidationCallback = (s, c, h, e) => true;

        public SmtpSecureOptions SecureOptions { get; set; } = SmtpSecureOptions.Auto;

        public SmtpValidSecureOptions ValidSecureOptions { get; set; } = SmtpValidSecureOptions.Default;

        public NetworkCredential SmtpCredentials { get; set; } = null;

        public RemoteCertificateValidationCallback ServerCertificateValidationCallback
        {
            get => serverCertificateValidationCallback;
            set
            {
                Assert.NotNull(value, nameof(value));
                serverCertificateValidationCallback = value;
            }
        }

        public bool CheckCertificateRevocation { get; set; } = false;

        public X509CertificateCollection ClientCertificates { get; } = new X509CertificateCollection();
    }

    //  Delegado para el evento de estado de email
    public delegate void EmailSendEventHandler(object source, EmailSendEventArgs args);

    public class SmtpClient
    {
        private string host;
        private int port;

        private SmtpSecureConfig secureConfig = new SmtpSecureConfig {SecureOptions = SmtpSecureOptions.Auto};

        public SmtpClient()
        {
        }

        public SmtpClient(string host, int port)
            : this(host, port, new SmtpSecureConfig {SecureOptions = SmtpSecureOptions.Auto})
        {
        }

        public SmtpClient(string host, int port, SmtpSecureConfig secureConfig)
        {
            Host = host;
            Port = port;
            SecureConfig = secureConfig;
        }

        public string Host
        {
            get => host;
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                host = value;
            }
        }

        public int Port
        {
            get => port;
            set
            {
                Assert.GreaterThan(value, 0, nameof(value));
                port = value;
            }
        }

        public SmtpSecureConfig SecureConfig
        {
            get => secureConfig;
            set
            {
                Assert.NotNull(value, nameof(value));
                secureConfig = value;
            }
        }

        public int Timeout { get; set; } = 30000;


        //  Evento de estado de email en envio as�ncrono
        public event EmailSendEventHandler EmailSend;

        public void SendEmail(Email email)
        {
            SendEmail(email, false);
        }

        public void SendEmail(Email email, bool async)
        {
            try
            {
                var smtpClient = NewSmtpClient();
                var msg = email.ToMIMEMessage();
                if (async)
                    smtpClient.SendAsync(msg);
                else
                    smtpClient.Send(msg);

                OnEmailSend(new EmailSendEventArgs(msg.MessageId, EmailSendStatus.Sending, null));
            }
            catch (Exception ex)
            {
                var smtpException = new SmtpException("Error sending message '" + email.Id + "': " + ex.Message, ex);

                if (async)
                    OnEmailSend(new EmailSendEventArgs(email.Id, EmailSendStatus.SendingError, smtpException));
                else
                    throw smtpException;
            }
        }

        public void SendEMLEmail(FileInfo emlEmail, bool async)
        {
            MailKit.Net.Smtp.SmtpClient smtpClient = null;
            MimeMessage msg = null;
            try
            {
                smtpClient = NewSmtpClient();
                msg = MimeMessage.Load(emlEmail.FullName);
                OnEmailSend(new EmailSendEventArgs(msg.MessageId, EmailSendStatus.Sending, null));
                if (async)
                {
                    smtpClient.SendAsync(msg);
                }
                else
                {
                    smtpClient.Send(msg);
                    OnEmailSend(new EmailSendEventArgs(msg.MessageId, EmailSendStatus.Sended, null));
                }
            }
            catch (Exception ex)
            {
                var smtpException =
                    new SmtpException("Error sending message '" + msg.MessageId + "': " + ex.Message, ex);

                if (async)
                    OnEmailSend(new EmailSendEventArgs(msg.MessageId, EmailSendStatus.SendingError,
                        smtpException));
                else
                    throw smtpException;
            }
        }

        private MailKit.Net.Smtp.SmtpClient NewSmtpClient()
        {
            var coreStmpClient = new MailKit.Net.Smtp.SmtpClient();
            coreStmpClient.ClientCertificates = secureConfig.ClientCertificates;
            coreStmpClient.ServerCertificateValidationCallback = secureConfig.ServerCertificateValidationCallback;
            coreStmpClient.CheckCertificateRevocation = secureConfig.CheckCertificateRevocation;
            coreStmpClient.SslProtocols = (SslProtocols) secureConfig.ValidSecureOptions;

            coreStmpClient.Connect(Host, Port, secureConfig.SecureOptions.ToString().ToEnum<SecureSocketOptions>(true));

            //  Necesario para la admisi�n de certificadoawait client.SendAsync(emailMessage);s en ssl
            // ServicePointManager.ServerCertificateValidationCallback = AddressOf FDevuelveOk
            if (secureConfig.SmtpCredentials != null) coreStmpClient.Authenticate(secureConfig.SmtpCredentials);

            coreStmpClient.Timeout = Timeout;
            coreStmpClient.MessageSent += MessageSendEvent;
            return coreStmpClient;
        }

        private void MessageSendEvent(object sender, MessageSentEventArgs e)
        {
            var smtpClient = (MailKit.Net.Smtp.SmtpClient) sender;
            OnEmailSend(new EmailSendEventArgs(e.Message.MessageId, EmailSendStatus.Sended, null));
            smtpClient.Disconnect(true);
            smtpClient.Dispose();
        }

        protected void OnEmailSend(EmailSendEventArgs args)
        {
            if (EmailSend != null)
                EmailSend(this, args);
        }
    }
}