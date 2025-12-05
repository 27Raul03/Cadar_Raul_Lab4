namespace Cadar_Raul_Lab4.Models
{
    public class DashboardViewModel
    {
        public int TotalPredictions { get; set; }
        public List<PaymentTypeStat> PaymentTypeStats { get; set; } = new();
        public List<PriceBucketStat> PriceBuckets { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class PaymentTypeStat
    {
        public string PaymentType { get; set; } = string.Empty;
        public double AveragePrice { get; set; }
        public int Count { get; set; }
    }

    public class PriceBucketStat
    {
        public string Label { get; set; } = string.Empty; // ex. "0-10", "10-20"
        public int Count { get; set; }
    }
}
