using Microsoft.AspNetCore.Identity;

namespace ECommerceWeb.Models
{
    public class ECommerceUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Phone { set; get; }
    }
}
