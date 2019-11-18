using IdentityProjeto01.ViewModels;
using System.Web.Mvc;

namespace IdentityProjeto01.Controllers
{
    public class TopicoController : Controller
    {
        [Authorize]
        public ActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult Criar(TopicoCriarViewModel modelo)
        {
            return View();
        }
    }
}