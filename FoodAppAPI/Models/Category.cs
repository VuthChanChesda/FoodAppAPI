using FoodAppAPI.Models;

public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Emoji { get; set; } = null!;
    public ICollection<Item> Items { get; set; } = new List<Item>();
}
