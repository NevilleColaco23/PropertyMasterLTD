using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mime;

namespace MyWarehouse.Infrastructure.Services
{
    public interface IMailService
    {
        /// <summary>
        /// Function to send emails
        /// Additional To / CC/ BCC email address can be specified in MailSettings.xml
        /// Will lookup for templateName.cshtml file in the folder specified in MailSettings.xml
        /// If templateName.txt file is also found, will also create a plain text version of the mail
        /// </summary>
        /// <param name="model">Any typed / anonymous class from which to pick up values </param>
        /// <param name="templateName"> The template file name (.cshtml), without extension. Cannot be null</param>
        /// <param name="mailTo">The To email address, can have multiple comma separated email addresses, can be null</param>
        /// <param name="mailCc">The CC email address, can have multiple comma separated email addresses, can be null</param>
        /// <param name="mailBcc">The BCC email address, can have multiple comma separated email addresses, can be null</param>
        /// <param name="subject">The Subject of the email</param>
        /// <param name="mailConfigName">The name of the section in the MailSettings from which to pick up server and other details. If null, will use 'DefaultMailSettings'</param>
        /// <param name="attachments"></param>
        /// pass filename and stream for multiple attachments
        /// <returns>Void if mail sent successfully, else will raise exception on error.</returns>
        void SendMail<T>(T model, string templateName, string mailTo, string mailCc, string mailBcc, string subject,
            string mailConfigName = null, IEnumerable<MailAttachment> attachments = null);
    }

    public class MailAttachment
    {
        public MailAttachment(string contentName, Stream contentStream, ContentType contentType, bool isInline = false)
        {
            ContentName = contentName;
            ContentStream = contentStream;
            ContentType = contentType;
            IsInline = isInline;
        }

        public MailAttachment(string contentName, string fileNameWithPath, ContentType contentType, bool isInline = false)
        {
            ContentName = contentName;
            FileName = fileNameWithPath;
            ContentType = contentType;
            IsInline = isInline;
        }

        public string ContentName { get; }
        public ContentType ContentType { get; }
        public Stream ContentStream { get; }
        public string FileName { get; }
        public bool IsInline { get; }
    }

    [Table("MailLog")]
    public class MailLog<T>
    {
        public string Id { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }

        public T Model { get; set; }

        public string Template { get; set; }
        public string Subject { get; set; }
        public string Config { get; set; }
        public DateTime CreatedDateTime { get; set; }

        public string Attachments { get; set; }
    }
}
