using Microsoft.EntityFrameworkCore;

namespace PatientApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Models.Patient> Patients { get; set; }
}