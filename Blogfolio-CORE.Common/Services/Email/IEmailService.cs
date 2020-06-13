using System.Net.Mail;
using System.Threading.Tasks;

/* Credits: https://github.com/andrewdavey/postal */

namespace Blogfolio_CORE.Common.Services.Email
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends a <see cref="MailMessage" />
        /// </summary>
        /// <param name="email"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="enableSSL"></param>
        void Send(MailMessage email, string host, int port, string userName, string password, bool enableSSL);

        /// <summary>
        /// Asynchronously sends a <see cref="MailMessage" />
        /// </summary>
        /// <param name="email"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="enableSSL"></param>
        Task SendAsync(MailMessage email, string host, int port, string userName, string password, bool enableSSL);
    }
}