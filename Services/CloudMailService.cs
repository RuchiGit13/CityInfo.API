namespace CityInfo.API.Services
{
    public class CloudMailService : IMailService
    {
        public string toemail;
        public string fromemail;
        public string subject;
        public string body;

        public CloudMailService(IConfiguration configuration)
        {
            this.toemail = configuration["mailSettings:toemail"];
            this.fromemail = configuration["mailSettings:fromemail"]; 
        }

        public void sendEmail(string subject, string body)
        {
            Console.WriteLine("Email ids " + fromemail + ":" + toemail);
            Console.WriteLine("From Cloud Service Mail subject is " + subject);
            Console.WriteLine(" Mail body is " + body);
        }

    }
}
