using System.ComponentModel.DataAnnotations;

namespace FoodAppAPI.Dtos
{
    public class ConsumeItemDto
    {
        [Required(ErrorMessage = "ItemID  is required.")]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Consumption amount is required.")]
        public int ConsumptionAmount { get; set; }
    }
}
