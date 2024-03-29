﻿using IdentityProjeto01.Models;
using IdentityProjeto01.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
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

        private SignInManager<UsuarioAplicacao, string> _singInManager;
        public SignInManager<UsuarioAplicacao, string> SignInManager
        {
            get
            {
                if (_singInManager == null)
                {
                    var contextOwin = HttpContext.GetOwinContext();
                    _singInManager = contextOwin.GetUserManager<SignInManager<UsuarioAplicacao, string>>();
                }
                return _singInManager;
            }
            set
            {
                _singInManager = value;
            }
        }

        public IAuthenticationManager authenticationManager
        {
            get { var contextoOwin = Request.GetOwinContext(); return contextoOwin.Authentication; }
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
                var usuario = await UserManager.FindByEmailAsync(model.Email);

                if (usuario == null || usuario.EmailConfirmed == false)
                    return SenhaOuUsuarioInvalido();

                var singInResult = await SignInManager.PasswordSignInAsync(
                            usuario.UserName, model.Senha, isPersistent: model.ContinuarLogado, shouldLockout: true);

                switch (singInResult)
                {
                    case SignInStatus.Success: return RedirectToAction("Index", "Home");
                    case SignInStatus.LockedOut:
                        var senhaCorreta = await UserManager.CheckPasswordAsync(usuario, model.Senha);
                        if (senhaCorreta) { ModelState.AddModelError("", "Conta Bloqueada"); break; }
                        ModelState.AddModelError("", "As Credenciais estão invalias!"); break;
                    default: return SenhaOuUsuarioInvalido();
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Logoff()
        {
            authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult EsqueciSenha()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> EsqueciSenha(ContaEsqueciSenhaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByEmailAsync(model.Email);

                if (usuario != null)
                {
                    var token = await UserManager.GeneratePasswordResetTokenAsync(usuario.Id);

                    var linkDeCallback = Url.Action("ConfirmacaoAlteracaoSenha", "Conta", new { usuarioId = usuario.Id, token = token }, Request.Url.Scheme);

                    await UserManager.SendEmailAsync(usuario.Id, "Projeto Teste Identity - Alteração de senha",
                     $"Redefinição de senha, clique aqui {linkDeCallback}!");
                }

                return View("EmailAlteracaoSenhaEnviado");
            }

            return View();
        }

       
        public ActionResult ConfirmacaoAlteracaoSenha(string usuarioId, string token)
        {
            var modelo = new ConfirmacaoAlteracaoSenhaViewModel
            {
                UsuarioId = usuarioId,
                Token = token
            };

            return View(modelo);
        }

        [HttpPost]
        public async Task<ActionResult> ConfirmacaoAlteracaoSenha(ConfirmacaoAlteracaoSenhaViewModel model)
        {
            if (ModelState.IsValid)
            {
               var result = await UserManager.ResetPasswordAsync(model.UsuarioId, model.Token, model.NovaSenha);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                AdicionaErros(result);
            }

            return View();
        }

        private ActionResult SenhaOuUsuarioInvalido()
        {
            ModelState.AddModelError("", "Credencias Invalidas");
            return View("Login");
        }
    }
}