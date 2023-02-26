using System.Collections.Generic;
using Newtonsoft.Json;

namespace Blogfolio_CORE.Common.Services.Captcha
{
    public class ReCaptchaResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error-codes")]
        public List<string> ErrorCodes { get; set; }
    }
}