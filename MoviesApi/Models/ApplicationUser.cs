using Microsoft.AspNetCore.Identity;

namespace MoviesApi.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string FName { get; set; }
        public string LName { get; set; }
    }
}
