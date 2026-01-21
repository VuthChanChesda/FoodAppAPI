namespace FoodAppAPI.Dtos.InsightDataDto
{
    public class ItemInsightDto
    {
        public int MonthlyWasteCount { get; set; }
        public double WasteChangePercentage { get; set; }
        public int MonthlyConsumedCount { get; set; }
        public double ConsumedChangePercentage { get; set; }

        public MostConsumedItemDto MostConsumedItem { get; set; }

        public List<WeeklyTrendDto> WeeklyConsumption { get; set; }

        public List<CategoryWasteDto> WasteByCategory { get; set; }
    }
}
