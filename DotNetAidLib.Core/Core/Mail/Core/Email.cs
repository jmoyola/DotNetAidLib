using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using DotNetAidLib.Core.Enums;
using MimeKit;

namespace DotNetAidLib.Core.Mail.Core
{
    public enum EmailImportance
    {
        Normal,
        High,
        Low
    }

    public class Email
    {
        private static readonly string ExpresionRegularEmail =
            @"([_a-zA-Z0-9-]+(\.[_a-zA-Z0-9-]+)*@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*(\.[a-zA-Z]{2,5}))";

        private readonly List<FileContent> m_Attachments = new List<FileContent>();
        private string m_Sender;

        public Email()
        {
            DateOfCreation = DateTime.Now;
            Id = Guid.NewGuid().ToString();
        }

        public Email(string sender, string receivers, string subject, string body)
            : this()
        {
            if (!(sender == null)
                && !(receivers == null))
            {
                m_Sender = sender;
                RepplyTo = sender;
                foreach (var Dest in receivers.Split(',', ';'))
                    Receivers.Add(Dest.Trim());

                Subject = subject;
                Body = body;
            }
        }

        public string Id { get; set; }

        public EmailImportance EmailImportance { get; set; } = EmailImportance.Normal;

        public DateTime DateOfCreation { get; set; }

        public string Sender
        {
            get => m_Sender;
            set
            {
                m_Sender = value;
                RepplyTo = Sender;
            }
        }

        public string RepplyTo { get; set; }

        public IList<string> Receivers { get; } = new List<string>();

        public IList<string> CC { get; } = new List<string>();

        public IList<string> CCo { get; } = new List<string>();

        public string Subject { get; set; }

        public string Body { get; set; }

        public IList<FileContent> Attachments => m_Attachments;

        public static string ExpresionRegularEmailValido => @"^" + ExpresionRegularEmail + "$";

        public static string ExpresionRegularListaEmailsSeparadaPorComaValida =>
            @"^" + ExpresionRegularEmail + @"(\s*[,;]\s*" + ExpresionRegularEmail + ")*$";

        public static bool TryAndNormalizeEmailAddress(ref string address)
        {
            var ret = false;
            try
            {
                //  Quitamos espacios
                address = address.Trim();
                //  Si termina en una coma, la quitamos
                if (address.EndsWith(",", StringComparison.InvariantCulture)
                    || address.EndsWith(";", StringComparison.InvariantCulture))
                    address = address.Substring(0, address.Length - 1);

                var re = new Regex(ExpresionRegularEmailValido);
                ret = re.IsMatch(address);
            }
            catch
            {
                ret = false;
            }

            return ret;
        }

        public static bool TryAndNormalizeEmailListAddress(ref string addressCommaSeparatedList)
        {
            var ret = false;
            try
            {
                //  Quitamos espacios
                addressCommaSeparatedList.Replace(" ", "");
                var re = new Regex(ExpresionRegularListaEmailsSeparadaPorComaValida);
                ret = re.IsMatch(addressCommaSeparatedList);
            }
            catch
            {
                ret = false;
            }

            return ret;
        }

        public void ToEMLFile(FileInfo emlOutputFile)
        {
            try
            {
                var m = ToMIMEMessage();
                m.WriteTo(emlOutputFile.FullName);
            }
            catch (Exception ex)
            {
                throw new EmailException("Error saving email to eml file '" + emlOutputFile.FullName + "'", ex);
            }
        }

        public MimeMessage ToMIMEMessage()
        {
            MimeMessage msg = null;
            try
            {
                //  Creamos un email nuevo
                msg = new MimeMessage();
                msg.MessageId = Id;
                msg.Importance = EmailImportance.ToString().ToEnum<MessageImportance>(true);
                msg.ReplyTo.Add(InternetAddress.Parse(RepplyTo));
                msg.Date = DateOfCreation;
                msg.From.AddRange(InternetAddressList.Parse(Sender));
                foreach (var sTo in Receivers)
                    msg.To.Add(InternetAddress.Parse(sTo));

                foreach (var sCc in CC)
                    msg.Cc.Add(InternetAddress.Parse(sCc));

                foreach (var sCco in CCo)
                    msg.Bcc.Add(InternetAddress.Parse(sCco));

                msg.Subject = Subject;
                var bodyBuilder = new BodyBuilder();
                //  Cuerpo
                var cuerpoEnHtml = Body.IndexOf("<html>", StringComparison.InvariantCultureIgnoreCase) > -1;
                if (!cuerpoEnHtml)
                    bodyBuilder.TextBody = Body;
                else
                    bodyBuilder.HtmlBody = Body;

                // Adjuntos
                foreach (var attachment in Attachments
                             .Where(v => v.ContentType == ContentType.Linked))
                    bodyBuilder.LinkedResources.Add(attachment.Name, attachment.Stream).ContentId =
                        HttpUtility.UrlEncode(attachment.Name);

                foreach (var attachment in Attachments
                             .Where(v => v.ContentType == ContentType.Embedded))
                    bodyBuilder.Attachments.Add(attachment.Name, attachment.Stream).ContentId =
                        HttpUtility.UrlEncode(attachment.Name);

                msg.Body = bodyBuilder.ToMessageBody();


                return msg;
            }
            catch (Exception ex)
            {
                throw new EmailException("Error creating mime message from email.", ex);
            }
        }
    }
}