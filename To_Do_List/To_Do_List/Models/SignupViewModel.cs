using System.ComponentModel.DataAnnotations;

namespace To_Do_List.Models
{
    public class SignupViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage ="Both Password doesn't Match")]
        public string ConfirmPassword { get; set; }
    }
}
