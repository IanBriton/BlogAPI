using System.ComponentModel.DataAnnotations;

namespace BlogAPI.Dto
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is Required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is Required. ")]
        public string Password { get; set; }
    }
}
