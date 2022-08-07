using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace KrkaWeb.Models
{

    // Za uporabnike, administratorje in skladiscnike
    public class ApplicationUser : IdentityUser
    {
        public DateTime LastLogin { get; set; }
        public DateTime DateCreated { get; set; }
        public string UniqueToken { get; set; }
        public string UserRole { get; set; }
        public Int32 WarehouseNumber { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
            
        }
    }

    public class DeliveryDbContext : DbContext
    {
        public DeliveryDbContext() : base("DeliveryConnection")
        {

        }

        public DbSet<ApplicationDelivery> Deliveries { get; set; }
    }

}