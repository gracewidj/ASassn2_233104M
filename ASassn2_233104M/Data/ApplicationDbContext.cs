using Microsoft.EntityFrameworkCore;
using ASassn2_233104M.Models; 

namespace ASassn2_233104M.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
		{
		}

		public DbSet<User> Users { get; set; }  // This defines the 'Users' table in your database
        public DbSet<AuditLog> AuditLogs { get; set; }

    }
}
