using System.Collections.Generic;

namespace BarangayBayanihanOnline.Models
{
    public class AdminViewModel
    {
        public List<Post> Posts { get; set; }
        public List<User> Users { get; set; } = new List<User>();
        public List<Event> Events { get; set; } = new List<Event>();
        public List<Donation> Donations { get; set; } = new List<Donation>();
        public List<ContactMessage> ContactMessages { get; set; } = new List<ContactMessage>();
        public List<Volunteer> Volunteers { get; set; } = new List<Volunteer>();
        public List<LoginHistory> LoginHistory { get; set; } = new List<LoginHistory>();

        // Add counts for quick overview
        public int TotalUsers => Users.Count;
        public int TotalEvents => Events.Count;
        public decimal TotalDonations => Donations.Sum(d => d.Amount);
        public int PendingMessages => ContactMessages.Count(m => m.Status == "New");
        public int PendingVolunteers => Volunteers.Count(v => v.Status == "Pending");
    }
}