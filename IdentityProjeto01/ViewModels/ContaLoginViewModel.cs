using System.ComponentModel.DataAnnotations;

namespace IdentityProjeto01.ViewModels
{
    public class ContaLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Senha { get; set; }
    }
}