using Cinema.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace Cinema.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config=config;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult About()
        {
            return View();
        }

        [HttpPost]
        public IActionResult About(string name, string email, string message)
        {
            try
            {
                // чете настройките директно от appsettings.json
                var host = _config["Smtp:Host"];
                var port = int.Parse(_config["Smtp:Port"]);
                var enableSsl = bool.Parse(_config["Smtp:EnableSsl"]);
                var userName = _config["Smtp:UserName"];
                var password = _config["Smtp:Password"];
                var from = _config["Smtp:From"];
                var to = _config["Smtp:To"];
                var subject = _config["Smtp:Subject"];

                var body = $"Hello Support Team,\n\n" +
           $"A new message has been submitted through the 'About Us' form.\n\n" +
           $"Details:\n\n" +
           $"Name: {name}\n" +
           $"Email: {email}\n" +
           $"Message:\n{message}\n\n" +
           $"Please follow up with the user as needed.\n\n" +
           $"Best regards,\n" +
           $"The Starluxe Cinema Team";

                using (var client = new SmtpClient(host, port))
                {
                    client.Credentials = new NetworkCredential(userName, password);
                    client.EnableSsl = enableSsl;

                    var mail = new MailMessage(from, to, subject, body);

                    // Reply-To, за да може support да отговаря директно на потребителя
                    mail.ReplyToList.Add(new MailAddress(email));

                    client.Send(mail);
                }

                ViewBag.Message = "Message has been sent successfully!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "There was an error when sending: " + ex.Message;
            }

            return View();
        }
    }
}