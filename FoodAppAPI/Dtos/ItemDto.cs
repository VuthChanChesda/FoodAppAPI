using System.ComponentModel.DataAnnotations;

namespace FoodAppAPI.Dtos
{
    public class ItemDto
    {

        [Required(ErrorMessage = "Category id is required.")]
        public int CategoryId { get; set; }  
        
        [Required(ErrorMessage = "Item name is required.")]
        public string ItemName { get; set; } = null!;

        [Required(ErrorMessage = "Quantity is required.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "IsShoppingList is required.")]
        public bool IsShoppingList { get; set; }

        public DateTime? AddedDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

    }
}
