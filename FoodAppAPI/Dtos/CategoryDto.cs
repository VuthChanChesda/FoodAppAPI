using System.ComponentModel.DataAnnotations;

namespace FoodAppAPI.Dtos
{
    public class CategoryDTO
    {
        [Required(ErrorMessage = "Category name is required.")]
        public string CategoryName { get; set; } = null!;
        
        public int UserId { get; set; }

        [Required(ErrorMessage = "Emoji is required.")]
        public string Emoji { get; set; } = null!;
    }
}
