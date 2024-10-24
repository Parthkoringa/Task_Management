using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace To_Do_List.Models
{
    public class TaskViewModel
    {
        [Required]
        public string Task_title { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Task_description { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        [Required]
        public bool IsComplete { get; set; } = false;

    }
}
