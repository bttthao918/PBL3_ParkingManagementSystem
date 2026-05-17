namespace ParkingManagement.FE.Models.ViewModels
{
    public interface IRevenueStatisticsViewModel
    {
        StatisticsHeaderViewModel Header { get; }
        List<StatisticsKpiCardViewModel> Kpis { get; }
        StatisticsTableViewModel Table { get; }
<<<<<<< HEAD
        RevenueStatisticsChartConfig Charts { get; }
        List<StatisticsBreakdownItemViewModel> PaymentMethodBreakdown { get; }
        List<StatisticsBreakdownItemViewModel> VehicleTypeBreakdown { get; }
        List<StatisticsRankItemViewModel> Rankings { get; }
        string RankingTitle { get; }
=======
        string ChartConfigJson { get; }
        string LineChartTitle { get; }
        string DonutTitle { get; }
        string BarTitle { get; }
        string RankTitle { get; }
        string ProgressTitle { get; }
        List<StatisticsBreakdownItemViewModel> DonutItems { get; }
        List<StatisticsBreakdownItemViewModel> BarItems { get; }
        List<StatisticsRankItemViewModel> RankItems { get; }
        List<StatisticsBreakdownItemViewModel> ProgressItems { get; }
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
    }

    public class StatisticsKpiCardViewModel
    {
        public string Title { get; set; } = "";
        public string Value { get; set; } = "";
        public string ChangeText { get; set; } = "";
        public string Icon { get; set; } = "";
        public string ColorClass { get; set; } = "blue";
    }

    public class StatisticsTableViewModel
    {
        public List<string> Headers { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();
    }

    public class StatisticsHeaderViewModel
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string DateRangeText { get; set; } = "";
        public string ActivePeriod { get; set; } = "30days";
    }

    public class StatisticsBreakdownItemViewModel
    {
        public string Label { get; set; } = "";
<<<<<<< HEAD
        public decimal Value { get; set; }
=======
        public string Value { get; set; } = "";
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
        public decimal Percentage { get; set; }
        public string ColorClass { get; set; } = "blue";
    }

    public class StatisticsRankItemViewModel
    {
<<<<<<< HEAD
        public string Rank { get; set; } = "";
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public string Note { get; set; } = "";
    }

    public class RevenueStatisticsChartConfig
    {
        public string Type { get; set; } = "revenue";
        public RevenueLineChartConfig Line { get; set; } = new();
        public RevenueSeriesChartConfig Donut { get; set; } = new();
        public RevenueSeriesChartConfig Bar { get; set; } = new();
    }

    public class RevenueLineChartConfig
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Current { get; set; } = new();
        public List<decimal> Previous { get; set; } = new();
    }

    public class RevenueSeriesChartConfig
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Data { get; set; } = new();
=======
        public int Rank { get; set; }
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public string ChangeText { get; set; } = "";
        public string ChangeClass { get; set; } = "";
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
    }
}
