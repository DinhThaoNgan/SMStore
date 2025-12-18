namespace CuaHangBanSach.ViewModels
{
    public class RevenueFilterViewModel
    {
        public string FilterType { get; set; } = "day";

        // Theo ngày
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Theo tháng
        public int? FromMonth { get; set; }
        public int? FromMonthYear { get; set; }
        public int? ToMonth { get; set; }
        public int? ToMonthYear { get; set; }

        // Theo năm
        public int? FromYear { get; set; }
        public int? ToYear { get; set; }

        // Kết quả đơn hàng
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalRevenue { get; set; }

        // 🆕 Kết quả nhập hàng
        public decimal TotalImportCost { get; set; }
        public decimal NetProfit => TotalRevenue - TotalImportCost;

        // 🆕 Dữ liệu biểu đồ
        public List<string> Labels { get; set; } = new();
        public List<decimal> Revenues { get; set; } = new();
        public List<decimal> ImportCosts { get; set; } = new();

        public string? ChartImageBase64 { get; set; }
    }
}