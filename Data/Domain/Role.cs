using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TES.Data.Domain
{
    public class Role : IdentityRole<long, UserRole>
    {
        public Role()
        {
        }

        public Role(string name)
        {
            Name = name;
        }

        [StringLength(200)]
        public string PersianName { get; set; }
    }
}