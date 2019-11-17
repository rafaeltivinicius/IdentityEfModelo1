using IdentityProjeto01.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using System.Data.Entity;
using Microsoft.AspNet.Identity.Owin; //adicionar manualmente - Get
using IdentityProjeto01.App_Start.Identity;

[assembly: OwinStartup(typeof(IdentityProjeto01.Startup))]
namespace IdentityProjeto01
{
    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            //Criação DbContext Owin
            builder.CreatePerOwinContext<DbContext>(() =>
                new IdentityDbContext<UsuarioAplicacao>("DefaultConnection"));

            //utilizamos a interface IUserStore para disfarçar que dependemos do Entity Framework
            builder.CreatePerOwinContext<IUserStore<UsuarioAplicacao>>(
                (opcoes, contextoOwin) =>
                {
                    var dbContext = contextoOwin.Get<DbContext>();
                    return new UserStore<UsuarioAplicacao>(dbContext);
                });

            builder.CreatePerOwinContext<UserManager<UsuarioAplicacao>>(
                 (opcoes, contextoOwin) =>
                 {
                     var userStore = contextoOwin.Get<IUserStore<UsuarioAplicacao>>();
                     var userManager = new UserManager<UsuarioAplicacao>(userStore);

                     //valida Email nativo, somente (@ e .)
                     var userValidator = new UserValidator<UsuarioAplicacao>(userManager);
                     userValidator.RequireUniqueEmail = true;
                     //Setando o user Validator
                     userManager.UserValidator = userValidator;
                     // validator da senha
                     userManager.PasswordValidator = new SenhaValidador()
                     {
                         TamanhoRequerido = 6,
                         ObrigatorioCaracteresEspeciais = true,
                         ObrigatorioDigitos = true,
                         ObrigatorioLowerCase = true,
                         ObrigatorioUpperCase = true
                     };
                     //Envio de Email para o Cliente (verificando validação)
                     userManager.EmailService = new EmailServico();

                     var dataProtectionProvider = opcoes.DataProtectionProvider;
                     var dataProtectionProviderCreated =  dataProtectionProvider.Create("IdentityProjeto01");
                     userManager.UserTokenProvider = new DataProtectorTokenProvider<UsuarioAplicacao>(dataProtectionProviderCreated);

                     return userManager;
                 });


        }
    }
}