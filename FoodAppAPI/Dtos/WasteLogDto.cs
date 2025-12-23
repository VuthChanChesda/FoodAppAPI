using System.ComponentModel.DataAnnotations;

namespace FoodAppAPI.Dtos
{
    public class WasteLogDto
    {
        [Required(ErrorMessage = "Item id is required.")]
        public int ItemId { get; set; }
        [Required(ErrorMessage = "Quantity wasted is required.")]
        public int QuantityWasted { get; set; }
        [Required(ErrorMessage = "Reason for wasting is required.")]
        public string Reason { get; set; } = null!;
        public DateTime? DateWasted { get; set; }
    }
}
