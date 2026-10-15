using AliadaLegal.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AliadaLegal.Web.Data;

public class ApplicationDbContext
    : IdentityDbContext<ApplicactionUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}