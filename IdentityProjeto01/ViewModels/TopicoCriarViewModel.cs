using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IdentityProjeto01.ViewModels
{
    public class TopicoCriarViewModel
    {
        [Display(Name = "Titulo")]
        public string  Titulo { get; set; }
        [Display(Name = "Conteudo")]
        public string Conteudo { get; set; }
    }
}