using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

/* Credits: https://github.com/andrewdavey/postal */

namespace Blogfolio_CORE.Common.Services.Email
{
    /// <summary>
    /// Implementation that uses <see cref="SmtpClient" />
    /// </summary>
    public class EmailService : IEmailService
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
        public void Send(MailMessage email, string host, int port, string userName, string password, bool enableSSL)
        {
            using (var client = new SmtpClient(host, port))
            {
                client.Credentials = new NetworkCredential(userName, password);
                client.EnableSsl = enableSSL;
                client.Timeout = 10000;

                client.Send(email);
            }
        }

        /// <summary>
        /// Asynchronously sends a <see cref="MailMessage" />
        /// </summary>
        /// <param name="email"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="enableSSL"></param>
        /// <returns>A <see cref="Task" /></returns>
        public Task SendAsync(MailMessage email, string host, int port, string userName, string password, bool enableSSL)
        {
            var client = new SmtpClient(host, port);
            try
            {
                var taskCompletionSource = new TaskCompletionSource<object>();

                client.Credentials = new NetworkCredential(userName, password);
                client.EnableSsl = enableSSL;

                client.SendCompleted += (o, e) =>
                {
                    client.Dispose();
                    email.Dispose();

                    if (e.Error != null)
                    {
                        taskCompletionSource.TrySetException(e.Error);
                    }
                    else if (e.Cancelled)
                    {
                        taskCompletionSource.TrySetCanceled();
                    }
                    else
                    {
                        taskCompletionSource.TrySetResult(null);
                    }
                };

                client.Timeout = 10000;
                client.SendAsync(email, null);
                return taskCompletionSource.Task;
            }
            catch
            {
                client.Dispose();
                email.Dispose();
                throw;
            }
        }
    }
}