using System.ComponentModel.DataAnnotations;

namespace IdentityProjeto01.ViewModels
{
    public class ContaEsqueciSenhaViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}