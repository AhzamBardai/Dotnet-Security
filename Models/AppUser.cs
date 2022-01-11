using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecurityFinal.Models {
    public class AppUser : IdentityUser {

        [Required]
        public string Name { get; set; }

        [Display(Name = "Current Movie")]
        public int? MovieId { get; set; }
        
        [Display(Name = "Movie Started At")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yy - hh:mm}")]
        public DateTime StartTime { get; set; }
        
        [Display(Name = "Movie Ending At")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yy - hh:mm}")]
        public DateTime EndTime { get; set; }
        
        [NotMapped]
        [Required]
        [Display(Name = "Role")]
        public string RoleId { get; set; }
        [NotMapped]
        public string Role { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> RoleList { get; set; }

    }
}
