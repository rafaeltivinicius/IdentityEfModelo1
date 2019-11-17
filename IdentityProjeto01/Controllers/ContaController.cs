using IdentityProjeto01.Models;
using IdentityProjeto01.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace IdentityProjeto01.Controllers
{
    public class ContaController : Controller
    {
        private UserManager<UsuarioAplicacao> _userManager;
        public UserManager<UsuarioAplicacao> UserManager
        {
            get
            {
                if (_userManager == null)
                {
                    var contextOwin = HttpContext.GetOwinContext();
                    _userManager = contextOwin.GetUserManager<UserManager<UsuarioAplicacao>>();
                }
                return _userManager;
            }
            set
            {
                _userManager = value;
            }
        }

        public ActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Registrar(ContaRegistrarViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                //dbCOntext do Identity com EF
                //camada imediata é a de Acesso aos Dados (Data Access Layer) no caso é o Entity Framework
                //mas poderia ser um SQL, um MongoDB entre outros
                //var dbContext = new IdentityDbContext<UsuarioAplicacao>("DefaultConnection");

                //classe que gera essa interface entre o Identity e o banco de dados
                //tilizada para manipular os usuários dentro do Identity Framework
                //var userStore = new UserStore<UsuarioAplicacao>(dbContext);

                //(termo em inglês para "gerenciamento") menos específica e desacopla e pertence ao Identity (não ao EntityFramework)
                //var userManager = new UserManager<UsuarioAplicacao>(userStore); 

                //             ### AGORA TRABALHANDO COM OWIN ####

                if (!ModelState.IsValid)
                    return View(modelo);

                var usuario = await UserManager.FindByEmailAsync(modelo.Email);

                var novoUsuario = new UsuarioAplicacao();

                novoUsuario.Email = modelo.Email;
                novoUsuario.UserName = modelo.UserName;
                novoUsuario.NomeCompleto = modelo.NomeCompleto;

                var UsuarioJaExiste = usuario != null;

                if (UsuarioJaExiste)
                    return View("AguardandoConfirmacao");

                var resultado = await UserManager.CreateAsync(novoUsuario, modelo.Senha);

                if (resultado.Succeeded)
                {
                    // Enviar o email de confirmação
                    await EnviarEmailDeConfirmacaoAsync(novoUsuario);

                    return View("AguardandoConfirmacao");
                }
                else
                    AdicionaErros(resultado);

                //dbContext.Users.Add(novoUsuario);  trabalha com EntityFramework (muito acoplado)
                //dbContext.SaveChanges(); trabalha com EntityFramework (muito acoplado)

                // Podemos incluir o usuario
            }

            // Alguma coisa de errado aconteceu!
            return View(modelo);
        }

        private void AdicionaErros(IdentityResult resultado)
        {
            foreach (var erro in resultado.Errors)
                ModelState.AddModelError("", erro);
        }

        private async Task EnviarEmailDeConfirmacaoAsync(UsuarioAplicacao usuario)
        {
            var token = await UserManager.GenerateEmailConfirmationTokenAsync(usuario.Id);
            var linkDeCallback = Url.Action("ConfirmacaoEmail", "Conta", new { usuarioId = usuario.Id, token = token }, Request.Url.Scheme);

            await UserManager.SendEmailAsync(usuario.Id, "Projeto Teste Identity - Confirmação de Email",
             $"Bem vindo, clique aqui {linkDeCallback} para confirmar seu email!");
        }

        public async Task<ActionResult> ConfirmacaoEmail(string usuarioId, string token)
        {
            if (usuarioId == null || token == null)
                return View("Error");

            var resultado = await UserManager.ConfirmEmailAsync(usuarioId, token);

            if (resultado.Succeeded)
                return RedirectToAction("Index", "Home");
            else
                return View("Error");
        }

        public async Task<ActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(ContaLoginViewModel model)
        {
            if (ModelState.IsValid)
            {

            }

            return View();
        }
    }
}