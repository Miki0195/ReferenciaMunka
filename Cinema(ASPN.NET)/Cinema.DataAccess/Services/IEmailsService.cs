namespace ELTE.Cinema.DataAccess.Services;

public interface IEmailsService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}