using System;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Web;
using DotNetAidLib.Core.Enums;
using DotNetAidLib.Core.Helpers;
using MimeKit;

namespace DotNetAidLib.Core.Mail.Core
{
    public enum EmailImportance {
        Normal, High, Low
    }

    public class Email
    {
        private string m_Id;
        private EmailImportance m_EmailImportance=EmailImportance.Normal;
        private DateTime m_DateOfCreation;
        private string m_Sender;
        private string m_RepplyTo;
        private IList<string> m_Receivers = new List<string>();
        private IList<string> m_CC = new List<string>();
        private IList<string> m_CCo = new List<string>();
        private string m_Subject;
        private string m_Body;
        private List<FileContent> m_Attachments = new List<FileContent>();

        public Email()
        {
            this.m_DateOfCreation = DateTime.Now;
            this.m_Id = Guid.NewGuid().ToString();
        }

        public Email(string sender, string receivers, string subject, string body)
            :this()
        {
            if ((!(sender == null)
                        && !(receivers == null)))
            {
                this.m_Sender = sender;
                this.m_RepplyTo = sender;
                foreach (string Dest in receivers.Split(new char[] { ',' , ';'}))
                    this.m_Receivers.Add(Dest.Trim());

                this.m_Subject = subject;
                this.m_Body = body;
            }

        }

        public string Id{
            get{
                return m_Id;
            }
            set{
                m_Id = value;
            }
        }

        public EmailImportance EmailImportance {
            get { return m_EmailImportance; }
            set { m_EmailImportance = value; }
        }

        public DateTime DateOfCreation{
            get{return m_DateOfCreation;}
            set{m_DateOfCreation = value;}
        }

        public string Sender{
            get{return m_Sender;}
            set{
                m_Sender = value;
                this.RepplyTo = this.Sender;
            }
        }

        public string RepplyTo{
            get{return m_RepplyTo;}
            set{m_RepplyTo = value;}
        }

        public IList<string> Receivers{
            get{return m_Receivers;}
        }

        public IList<string> CC{
            get{return m_CC;}
        }

        public IList<string> CCo{
            get{return m_CCo;}
        }

        public string Subject{
            get{return m_Subject;}
            set{m_Subject = value;}
        }

        public string Body{
            get{return m_Body;}
            set{m_Body = value;}
        }

        public IList<FileContent> Attachments{
            get{return m_Attachments;}
        }

        private static String ExpresionRegularEmail = @"([_a-zA-Z0-9-]+(\.[_a-zA-Z0-9-]+)*@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*(\.[a-zA-Z]{2,5}))";
        public static string ExpresionRegularEmailValido{
            get{
                return @"^" + ExpresionRegularEmail + "$";
            }
        }

        public static string ExpresionRegularListaEmailsSeparadaPorComaValida{
            get{
                return @"^" + ExpresionRegularEmail + @"(\s*[,;]\s*" + ExpresionRegularEmail +")*$";
            }
        }

        public static bool TryAndNormalizeEmailAddress(ref string address)
        {
            bool ret = false;
            try
            {
                //  Quitamos espacios
                address = address.Trim();
                //  Si termina en una coma, la quitamos
                if (address.EndsWith(",", StringComparison.InvariantCulture)
                    || address.EndsWith(";", StringComparison.InvariantCulture)){
                    address = address.Substring(0, (address.Length - 1));
                }

                Regex re = new Regex(ExpresionRegularEmailValido);
                ret = re.IsMatch(address);
            }
            catch{
                ret = false;
            }

            return ret;
        }

        public static bool TryAndNormalizeEmailListAddress(ref string addressCommaSeparatedList)
        {
            bool ret = false;
            try
            {
                //  Quitamos espacios
                addressCommaSeparatedList.Replace(" ", "");
                Regex re = new Regex(ExpresionRegularListaEmailsSeparadaPorComaValida);
                ret = re.IsMatch(addressCommaSeparatedList);
            }
            catch{
                ret = false;
            }

            return ret;
        }

        public void ToEMLFile (FileInfo emlOutputFile)
        {
            try
            {
                MimeMessage m = this.ToMIMEMessage();
                m.WriteTo(emlOutputFile.FullName);
            }
            catch (Exception ex)
            {
                throw new EmailException("Error saving email to eml file '" + emlOutputFile.FullName + "'", ex);
            }
        }

        public MimeMessage ToMIMEMessage ()
        {
            MimeMessage msg = null;
            try {
                //  Creamos un email nuevo
                msg = new MimeMessage();
                msg.MessageId = this.Id;
                msg.Importance = this.EmailImportance.ToString ().ToEnum<MessageImportance> (true);
                msg.ReplyTo.Add (InternetAddress.Parse (this.RepplyTo));
                msg.Date = this.DateOfCreation;
                msg.From.AddRange (InternetAddressList.Parse (this.Sender));
                foreach (string sTo in this.Receivers)
                    msg.To.Add (InternetAddress.Parse (sTo));

                foreach (string sCc in this.CC)
                    msg.Cc.Add (InternetAddress.Parse (sCc));

                foreach (string sCco in this.CCo)
                    msg.Bcc.Add (InternetAddress.Parse (sCco));

                msg.Subject = this.Subject;
                BodyBuilder bodyBuilder = new BodyBuilder ();
                //  Cuerpo
                bool cuerpoEnHtml = this.Body.IndexOf("<html>", StringComparison.InvariantCultureIgnoreCase)>-1;
                if (!cuerpoEnHtml)
                    bodyBuilder.TextBody = this.Body;
                else
                    bodyBuilder.HtmlBody = this.Body;

                // Adjuntos
                foreach (FileContent attachment in this.Attachments
                             .Where(v=>v.ContentType==ContentType.Linked))
                    bodyBuilder.LinkedResources.Add (attachment.Name, attachment.Stream).ContentId= HttpUtility.UrlEncode(attachment.Name);

                foreach (FileContent attachment in this.Attachments
                             .Where(v=>v.ContentType==ContentType.Embedded))
                    bodyBuilder.Attachments.Add (attachment.Name, attachment.Stream).ContentId= HttpUtility.UrlEncode(attachment.Name);
                
                msg.Body = bodyBuilder.ToMessageBody ();


                return msg;
            } catch (Exception ex) {
                throw new EmailException("Error creating mime message from email.", ex);
            }
        }
    }
}
