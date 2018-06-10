using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MediaStorage.Data.WebApp.Entities
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    //[Table("AspNetUsers")]
    public class ApplicationUser : IdentityUser
    {
        // public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        // {
        //     // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
        //     var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
        //     // Add custom user claims here
        //     return userIdentity;
        // }
    }
}
