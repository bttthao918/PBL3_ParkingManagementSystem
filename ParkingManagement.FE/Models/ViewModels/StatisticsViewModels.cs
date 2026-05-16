namespace ParkingManagement.FE.Models.ViewModels
{
    public interface IRevenueStatisticsViewModel
    {
        StatisticsHeaderViewModel Header { get; }
        List<StatisticsKpiCardViewModel> Kpis { get; }
        StatisticsTableViewModel Table { get; }
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
        public string Value { get; set; } = "";
        public decimal Percentage { get; set; }
        public string ColorClass { get; set; } = "blue";
    }

    public class StatisticsRankItemViewModel
    {
        public int Rank { get; set; }
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public string ChangeText { get; set; } = "";
        public string ChangeClass { get; set; } = "";
    }
}
