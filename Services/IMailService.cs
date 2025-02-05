namespace CityInfo.API.Services
{
    public interface IMailService
    {
        void sendEmail(string subject, string body);
    }
}