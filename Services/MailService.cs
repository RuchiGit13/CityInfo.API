namespace CityInfo.API.Services
{
    public class MailService : IMailService
    {
        public string toemail;
        public string fromemail;
        public string subject;
        public string body;
        public MailService(IConfiguration configuration)
        {
            this.toemail = configuration["mailSettings:toemail"];
            this.fromemail = configuration["mailSettings:fromemail"];
        }

        public void sendEmail(string subject, string body)
        {
            Console.WriteLine("Email ids are " + fromemail + ":" + toemail);
            Console.WriteLine("From MailService Mail subject is " + subject);
            Console.WriteLine(" Mail body is " + body);
        }

    }
}
