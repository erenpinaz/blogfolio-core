using System.Threading.Tasks;

namespace Blogfolio_CORE.Common.Services.Captcha
{
    public interface ICaptchaService
    {
        Task<ReCaptchaResponse> ValidateAsync(string secret, string response);
    }
}