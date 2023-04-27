using Microsoft.Extensions.Configuration;
using RabbitMQEmailService;
using System.Net.Mail;
using System.Net;
using System;

internal class Program
{
    static void Main(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", true, true)
            .AddJsonFile($"appsettings.{environment}.json", true, true)
            .AddEnvironmentVariables();
        var config = builder.Build();

        string connectionString = config["ConnectionString"];//Getting Connectionstring from appsettings

        Console.WriteLine($"Environment is: {environment}");
        Console.WriteLine($"Connection String is:{connectionString}");

        string smtpServer = "smtp-mail.outlook.com";
        SendEmail(smtpServer);
    }

   

    static void SendEmail(string smtpServer)
    {
        try
        {
            MailMessage mail = new MailMessage();
            mail.To.Add("jpatanvadiya1@gmail.com");
            mail.From = new MailAddress("Jayesh@silmac.biz");
            mail.Subject = "WebSite was down - please test";
            mail.Body = "WebSite was down - please test";
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("smtp-mail.outlook.com", 587);
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential("Jayesh@silmac.biz", "J@y5142J@y");
            smtp.Send(mail);
            //Send teh High priority Email  
            EmailManager mailMan = new EmailManager(smtpServer);

            EmailSendConfigure myConfig = new EmailSendConfigure();
            // replace with your email userName  
            myConfig.ClientCredentialUserName = "Jayesh@silmac.biz";
            // replace with your email account password
            myConfig.ClientCredentialPassword = "J@y5142J@y";
            myConfig.TOs = new string[] { "jpatanvadiya1@gmail.com" };
            myConfig.CCs = new string[] { "jpatanvadiya1@gmail.com" };
            myConfig.From = "Jayesh@silmac.biz";
            myConfig.FromDisplayName = "Jayesh";
            myConfig.Priority = System.Net.Mail.MailPriority.Normal;
            myConfig.Subject = "WebSite was down - please test";

            EmailContent myContent = new EmailContent();
            myContent.Content = "The following URLs were down - 1. Foo, 2. bar";

            mailMan.SendMail(myConfig, myContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }

}