using MailKit.Net.Smtp;
using ManagemateAPI.Encryption;
using ManagemateAPI.Management.Shared.Static;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ManagemateAPI.Mail
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> mailSettingsOptions)
        {
            _mailSettings = mailSettingsOptions.Value;

        }



        public async Task<string> SendMailAsync(MailData mailData)
        {

                using (MimeMessage emailMessage = new MimeMessage())
                {
                    MailboxAddress emailFrom = new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail);
                    emailMessage.From.Add(emailFrom);
                    MailboxAddress emailTo = new MailboxAddress(mailData.EmailToName, mailData.EmailToid);
                    emailMessage.To.Add(emailTo);

                    // you can add the CCs and BCCs here.
                    //emailMessage.Cc.Add(new MailboxAddress("Cc Receiver", "cc@example.com"));
                    //emailMessage.Bcc.Add(new MailboxAddress("Bcc Receiver", "bcc@example.com"));

                   

                    string emailTemplateText = string.Empty;

                    try
                    {
                        switch (mailData.EmailTemplate)
                        {
                            case 1:

                                emailMessage.Subject = Info.TWO_STEP_LOGIN_SUBJECT;
                                emailTemplateText = File_Provider.Get_Email_Template_HTML_2();
                            break;

                            default:
                                emailMessage.Subject = Info.ADD_USER_SUBJECT;
                                emailTemplateText = File_Provider.Get_Email_Template_HTML_1();
                                break;
                        }

                    }
                    catch (Exception)
                    {

                        throw new Exception("15");//_15_CONFIRM_CODE_READ_TEMPLATE_ERROR

                    }


                    emailTemplateText = string.Format(emailTemplateText, mailData.EmailToName, mailData.EmailCode, DateTime.UtcNow.ToString("dd-MM-yyyy HH:mm"));

                    BodyBuilder emailBodyBuilder = new BodyBuilder();
                    emailBodyBuilder.HtmlBody = emailTemplateText;
                    emailBodyBuilder.TextBody = Info.MAIL_SidE_TEXT;


                    emailMessage.Body = emailBodyBuilder.ToMessageBody();
                    //this is the SmtpClient from the Mailkit.Net.Smtp namespace, not the System.Net.Mail one
                    using (SmtpClient mailClient = new SmtpClient())
                    {
                        await mailClient.ConnectAsync(_mailSettings.Server, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                        await mailClient.AuthenticateAsync(_mailSettings.SenderEmail, Crypto.GetAppCode());
                        await mailClient.SendAsync(emailMessage);
                        await mailClient.DisconnectAsync(true);
                    }

                    return Info.CONFIRM_CODE_SENT;
                }


        }




    }

}
