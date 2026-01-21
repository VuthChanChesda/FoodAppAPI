using System.ComponentModel.DataAnnotations;

public class Consumption
{
    [Key]
    public int ConsumptionId { get; set; }

    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public int UserId { get; set; }

    public int AmountConsumed { get; set; }

    public DateTime ConsumedAt { get; set; }

    public int CategoryId { get; set; }
}