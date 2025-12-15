namespace FoodAppAPI.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        public string Role { get; set; } = "User";

        public bool IsDeleted { get; set; } = false; // soft delete

        public DateTime? DeletedAt { get; set; }

        // Navigation property or relationship
        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<WasteLog> WasteLogs { get; set; } = new List<WasteLog>();
        public ICollection<Category> Category { get; set; } = new List<Category>();
    }
}
