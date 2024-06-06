using System.ComponentModel.DataAnnotations;

namespace BlogAPI.Dto
{
    public class UpdatePermissionDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }
    }
}
