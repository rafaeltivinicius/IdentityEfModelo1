using Microsoft.AspNet.Identity;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace IdentityProjeto01.App_Start.Identity
{
    public class EmailServico : IIdentityMessageService
    {
        private readonly string EMAIL_ORIGEM = ConfigurationManager.AppSettings["emailServico:email_remetente"];
        private readonly string EMAIL_SENHA = ConfigurationManager.AppSettings["emailServico:email_senha"];

        //Configurar o seu Gmail para permitir acesso da aplicação
        //https://centraldeatendimento.totvs.com/hc/pt-br/articles/360003807872-RM-Frame-Envio-de-E-mail-Utilizando-SMTP-Gmail-

        public async Task SendAsync(IdentityMessage message)
        {
            using (var mensagemDeEmail = new MailMessage())
            {
                mensagemDeEmail.From = new MailAddress(EMAIL_ORIGEM);
                mensagemDeEmail.Subject = message.Subject;
                mensagemDeEmail.To.Add(message.Destination);
                mensagemDeEmail.Body = message.Body;

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(EMAIL_ORIGEM, EMAIL_SENHA);

                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.Host = "smtp.gmail.com";
                    smtpClient.Port = 587;
                    smtpClient.EnableSsl = true;

                    smtpClient.Timeout = 20_000;

                    await smtpClient.SendMailAsync(mensagemDeEmail);
                }
            }
        }
    }
}