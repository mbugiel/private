
namespace ManagemateAPI.Mail
{
    public interface IMailService
    {
        Task<string> SendMailAsync(MailData mailData);
    }
}
