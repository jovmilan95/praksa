using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace PraksaProjekat.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<WorkingHours> HoursList { get; set; }
        public List<Contract> ContractList { get; set; }

    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        public System.Data.Entity.DbSet<PraksaProjekat.Models.WorkingHours> WorkingHours { get; set; }

        public System.Data.Entity.DbSet<PraksaProjekat.Models.Contract> Contracts { get; set; }

        public System.Data.Entity.DbSet<PraksaProjekat.Models.NotificationEmail> NotificationEmails { get; set; }


        

      
        
    }
}