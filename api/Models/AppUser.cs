using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
namespace api.Models
{
    public class AppUser : IdentityUser
    {
        public List<Portfolio> Portfolios{get; set;} = new List<Portfolio>();
    }
}
 