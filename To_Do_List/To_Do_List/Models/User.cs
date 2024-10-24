using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace To_Do_List.Models
{
    [Index(nameof(User.Name), IsUnique = true)]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Password { get; set; }

        public ICollection<Task> Tasks { get; set; }

    }
}
