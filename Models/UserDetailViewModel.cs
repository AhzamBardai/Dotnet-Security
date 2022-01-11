using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SecurityFinal.Models {
    public class UserDetailViewModel {
        [Required]
        public AppUser User { get; set; }
        [Display(Name = "Current Movie In Progress")]
        public Movie CurrentMovie { get; set; }
        [Required]
        [Display(Name = "Past Movies")]
        public IEnumerable<UserMovie> PastMovies { get; set; }
    }
}
