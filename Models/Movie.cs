using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecurityFinal.Models {
    public class Movie {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Display(Name = "Release Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? Released { get; set; }

        [Required]
        public int Hours { get; set; }
        [Required]
        public int Minutes { get; set; }
    }
}
