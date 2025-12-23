using System.ComponentModel.DataAnnotations;

namespace FoodAppAPI.Dtos
{
    public class ItemDto
    {

        [Required(ErrorMessage = "Category id is required.")]
        public int CategoryId { get; set; }  
        
        [Required(ErrorMessage = "Item name is required.")]
        public string ItemName { get; set; } = null!;

        public IFormFile? Image { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "IsShoppingList is required.")]
        public bool IsShoppingList { get; set; }

        public DateTime? AddedDate { get; set; }

        public DateTime? ExpiryDate { get; set; }


        [Required(ErrorMessage = "IsExpiredProcessed is required.")]
        public bool IsExpiredProcessed { get; set; }


    }
}
