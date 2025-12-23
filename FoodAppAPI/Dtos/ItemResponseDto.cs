using System.ComponentModel.DataAnnotations;

namespace FoodAppAPI.Dtos
{
    public class ItemResponseDto
    {
            [Required(ErrorMessage = "Item id is required.")]
            public int ItemId { get; set; }

            [Required(ErrorMessage = "Category id is required.")]
            public int CategoryId { get; set; }

            [Required(ErrorMessage = "Item name is required.")]
            public string ItemName { get; set; } = null!;

            public string ImageUrl { get; set; } = null!;

            [Required(ErrorMessage = "Quantity is required.")]
            public int Quantity { get; set; }

            [Required(ErrorMessage = "IsShoppingList is required.")]
            public bool IsShoppingList { get; set; }

            public DateTime? AddedDate { get; set; }

            public DateTime? ExpiryDate { get; set; }

            public double ExpiryProgress { get; set; } // 0–100


        }
    }



