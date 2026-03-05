using System.ComponentModel.DataAnnotations;

namespace photoCon.Dto
{
    public class LoginView
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
