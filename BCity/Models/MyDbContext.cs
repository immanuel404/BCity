using Microsoft.EntityFrameworkCore;

namespace BCity.Models
{
    public class MyDbContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ContactClientLink> ClientContactLinks { get; set; }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // enforce unique constraint on client_code
            modelBuilder.Entity<Client>()
                .HasIndex(c => c.ClientCode)
                .IsUnique(); // clientcode must be unique

            // enforce unique constraint on email
            modelBuilder.Entity<Contact>()
                .HasIndex(c => c.Email)
                .IsUnique(); ; // email must be unique


            // clientid and contactid used as composite primary keys 
            modelBuilder.Entity<ContactClientLink>()
                .HasKey(link => new { link.ClientId, link.ContactId });

            // client can have many contact-links
            modelBuilder.Entity<ContactClientLink>()
                .HasOne(link => link.Client)
                .WithMany(client => client.ContactLinks)  
                .HasForeignKey(link => link.ClientId);

            // contact can have many client-links
            modelBuilder.Entity<ContactClientLink>()
                .HasOne(link => link.Contact)
                .WithMany(contact => contact.ClientLinks)
                .HasForeignKey(link => link.ContactId);
        }
    }
}