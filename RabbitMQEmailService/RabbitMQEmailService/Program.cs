using Microsoft.Extensions.Configuration;
using RabbitMQEmailService;
using System.Net.Mail;
using System.Net;
using System;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

internal class Program
{
    static async Task Main(string[] args)
    {


        Email email = new Email()
        {
            BCC = "jayesh@silmac.com",
            Body = "test mail",
            CC = "Jayesh@silmac.com",
            From = "jpatanvadiya123@gmail.com",
            Subject = "test subject",
            To = "Jayesh@silmac.com"
        };
        


        //await SendEmailMessage(email);
        await ReceiveEmailMessage("email");
        Thread.Sleep(60000);
        await SendFailedEmail(email);
        await ReceiveEmailFail("FailedEmailModel");
    }

    public static async Task SendEmailMessage(Email email)
    {
       

        RabitMQService rabitMQService = new RabitMQService();
        await rabitMQService.SendEmailMessage(email, "email", 86400000);
    }

    public static async Task<List<Email>> ReceiveEmailMessage(string routingKeyName)
    {
        try
        {
            RabitMQService rabitMQService = new RabitMQService();
            var result = await rabitMQService.ReceiveEmailMessage<Email>(routingKeyName);
            foreach (var mailSetting in result)
            {
                await SendEmail(mailSetting);

            }
            return result;

        }
        catch (Exception ex)
        {
            Console.WriteLine(" [x] error {0}", ex.Message);
            return new List<Email>();
        }
    }
    public static async Task SendFailedEmail(Email email)
    {
       
        RabitMQService rabitMQ = new RabitMQService();

        // RabitMQ Implementation
        var result = await rabitMQ.SendFailEmailMessage<Email>(email, "FailedEmailModel", 86400000);

    }
    public static async Task ReceiveEmailFail(string route)
    {

        RabitMQService rabitMQ = new RabitMQService();

        // RabitMQ Implementation
        var result = await rabitMQ.ReceiveSendFailEmailMessage<Email>(route);
        foreach (var mailSetting in result)
        {
            await SendEmail(mailSetting);
        }
    }
    static async Task SendEmail(Email email)
    {
        try
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment}.json", true, true)
                .AddEnvironmentVariables();
            var config = builder.Build();

            var port = config.GetSection("EmailSMTP:Port").Value;//Getting Port from appsettings
            string host = config.GetSection("EmailSMTP:Host").Value;
            string Username = config.GetSection("EmailSMTP:Username").Value;
            string Password = config.GetSection("EmailSMTP:Password").Value;

            Console.WriteLine($"Environment is: {environment}");
            Console.WriteLine($"Mail sent to: {email.To}");

            var portCast = Convert.ToInt32(port);


            MailMessage mail = new MailMessage();
            mail.To.Add(email.To);
            mail.From = new MailAddress(email.From);
            mail.CC.Add(email.CC);
            mail.Bcc.Add(email.BCC);
            mail.Subject = email.Subject;
            mail.Body = email.Body;
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient(host, portCast);
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential(Username, Password);
            smtp.Send(mail);

        }
        catch (Exception ex)
        {
            await SendFailedEmail(email);
            Console.WriteLine(ex.Message);
        }

    }

}