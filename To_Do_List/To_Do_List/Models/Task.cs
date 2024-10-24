using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace To_Do_List.Models
{
    [Table(name:"Tasks")]
    public class Task
    {
        [Key]
        [Column(name:"TaskId")]
        public int TaskId { get; set; }

        [Required]
        public string Task_title { get; set; }

        [Required]
        public string Task_description { get; set; }
        
        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public bool IsComplete { get; set; } = false;


        public bool DateExpired => !IsComplete && DueDate < DateTime.Now;

        public int UserId { get; set; }

        [BindNever]
        public virtual User User { get; set; }



    }
}
