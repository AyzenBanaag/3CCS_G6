using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BarangayBayanihanOnline.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<Volunteer> Volunteers { get; set; }
        public DbSet<LoginHistory> LoginHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User-Event relationship
            modelBuilder.Entity<Event>()
                .HasOne(e => e.User)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure User-Post relationship
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure User-Donation relationship
            modelBuilder.Entity<Donation>()
                .HasOne(d => d.User)
                .WithMany(u => u.Donations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure User-ContactMessage relationship
            modelBuilder.Entity<ContactMessage>()
                .HasOne(c => c.User)
                .WithMany(u => u.ContactMessages)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure User-Volunteer relationship
            modelBuilder.Entity<Volunteer>()
                .HasOne(v => v.User)
                .WithMany(u => u.Volunteers)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure User-LoginHistory relationship
            modelBuilder.Entity<LoginHistory>()
                .HasOne(l => l.User)
                .WithMany(u => u.LoginHistories)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add indexes for better performance
            modelBuilder.Entity<Event>()
                .HasIndex(e => e.UserId);

            modelBuilder.Entity<Event>()
                .HasIndex(e => e.CreatedAt);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }

    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }

        // Navigation properties
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Event> Events { get; set; } = new List<Event>();
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public ICollection<ContactMessage> ContactMessages { get; set; } = new List<ContactMessage>();
        public ICollection<Volunteer> Volunteers { get; set; } = new List<Volunteer>();
        public ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();
    }

    public class Post
    {
        [Key]
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
    }

    public class Event
    {
        [Key]
        public int EventId { get; set; }
        public int? UserId { get; set; }
        public string EventName { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string EventType { get; set; }
        public string Visibility { get; set; }
        public string RepeatFrequency { get; set; }
        public string Description { get; set; }
        public string CoHosts { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
    }

    public class Donation
    {
        [Key]
        public int DonationId { get; set; }
        public int? UserId { get; set; }
        public decimal Amount { get; set; }
        public string GCashReferenceNumber { get; set; }
        public string DonorName { get; set; }
        public DateTime DonationDate { get; set; }
        public string Status { get; set; }
        public User User { get; set; }
    }

    public class ContactMessage
    {
        [Key]
        public int MessageId { get; set; }
        public int? UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; }
        public User User { get; set; }
    }

    public class Volunteer
    {
        [Key]
        public int VolunteerId { get; set; }
        public int? UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string AreasOfInterest { get; set; }
        public DateTime ApplicationDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public User User { get; set; }
    }

    public class LoginHistory
    {
        [Key]
        public int LoginId { get; set; }
        public int UserId { get; set; }
        public DateTime LoginTime { get; set; }
        public string? IpAddress { get; set; }
        public User User { get; set; }
    }
}