using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models.ViewModels;

namespace ParkingManagement.FE.Pages.Shared.Statistics
{
    public class RevenueStatisticsModel : PageModel, IRevenueStatisticsViewModel
    {
        public StatisticsHeaderViewModel Header { get; set; } = new();
        public List<StatisticsKpiCardViewModel> Kpis { get; set; } = new();
        public StatisticsTableViewModel Table { get; set; } = new();
        public string ChartConfigJson { get; set; } = "{}";
        public string LineChartTitle { get; set; } = "Doanh thu theo ngày";
        public string DonutTitle { get; set; } = "Cơ cấu doanh thu";
        public string BarTitle { get; set; } = "Doanh thu theo nhóm";
        public string RankTitle { get; set; } = "Top doanh thu";
        public string ProgressTitle { get; set; } = "Tỷ trọng doanh thu";
        public List<StatisticsBreakdownItemViewModel> DonutItems { get; set; } = new();
        public List<StatisticsBreakdownItemViewModel> BarItems { get; set; } = new();
        public List<StatisticsRankItemViewModel> RankItems { get; set; } = new();
        public List<StatisticsBreakdownItemViewModel> ProgressItems { get; set; } = new();
    }
}
