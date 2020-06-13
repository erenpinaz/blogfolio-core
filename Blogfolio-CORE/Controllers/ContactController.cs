using System.Net.Mail;
using System.Threading.Tasks;
using Blogfolio_CORE.Common.Services.Captcha;
using Blogfolio_CORE.Common.Services.Email;
using Blogfolio_CORE.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Blogfolio_CORE.Controllers
{
    public class ContactController : BaseController
    {
        private readonly IEmailService _emailService;
        private readonly ICaptchaService _captchaService;

        public ContactController(IEmailService emailService, ICaptchaService captchaService)
        {
            _emailService = emailService;
            _captchaService = captchaService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(new ContactEditModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendMessage(ContactEditModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Please correctly fill all the required fields." });

            if (!string.IsNullOrWhiteSpace(SiteSettings.ReCaptchaKey) && !string.IsNullOrWhiteSpace(SiteSettings.ReCaptchaSecret))
            {
                var captchaResponse = await _captchaService.ValidateAsync(SiteSettings.ReCaptchaSecret, Request.Form["g-recaptcha-response"]);
                if (!captchaResponse.Success)
                {
                    return Json(new { success = false, message = string.Format("Captcha validation failed. ({0})", captchaResponse.ErrorCodes[0]) });
                }
            }

            var from = new MailAddress(model.Email, model.Name);
            var mailMessage = new MailMessage()
            {
                From = from,
                To = { SiteSettings.ContactEmail },
                Subject = model.Subject,
                Body = model.Message
            };
            try
            {
                await _emailService.SendAsync(mailMessage, SiteSettings.SMTPHost, SiteSettings.SMTPPort, SiteSettings.SMTPUserName, SiteSettings.SMTPPassword, SiteSettings.SMTPEnableSSL);
                return Json(new { success = true, message = "Your message has been successfully sent." });
            }
            catch
            {
                return Json(new { success = false, message = "An error occurred while sending your mail." });
            }
            finally
            {
                mailMessage.Dispose();
            }
        }
    }
}