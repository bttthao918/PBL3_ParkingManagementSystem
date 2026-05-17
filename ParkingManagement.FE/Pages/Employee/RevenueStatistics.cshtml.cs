using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models.ViewModels;
using ParkingManagement.FE.Services;

namespace ParkingManagement.FE.Pages.Employee
{
    [Authorize(Roles = "Employee")]
    public class RevenueStatisticsModel : PageModel, IRevenueStatisticsViewModel
    {
        private readonly IReportService _reportService;

        public RevenueStatisticsModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        public StatisticsHeaderViewModel Header { get; set; } = new();
        public List<StatisticsKpiCardViewModel> Kpis { get; set; } = new();
        public StatisticsTableViewModel Table { get; set; } = new();
        public RevenueStatisticsChartConfig Charts { get; set; } = new();
        public List<StatisticsBreakdownItemViewModel> PaymentMethodBreakdown { get; set; } = new();
        public List<StatisticsBreakdownItemViewModel> VehicleTypeBreakdown { get; set; } = new();
        public List<StatisticsRankItemViewModel> Rankings { get; set; } = new();
        public string RankingTitle { get; set; } = "Top ngày doanh thu cao nhất";

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Báo cáo doanh thu";
            ViewData["Role"] = "Nhân viên";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Nhân viên";

            var employeeId = User.FindFirst("related_id")?.Value;
            
            var today = DateTime.Now;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            Header = new StatisticsHeaderViewModel
            {
                Title = "Báo cáo doanh thu cá nhân",
                Description = "Thống kê doanh thu theo thời gian",
                DateRangeText = $"{monthStart:dd/MM/yyyy} - {today:dd/MM/yyyy}"
            };

            if (!string.IsNullOrEmpty(employeeId))
            {
                var report = await _reportService.GetEmployeeRevenueReportAsync(employeeId, "month");
                
                if (report != null)
                {
                    var changeText = report.RevenueChangePercentage >= 0 
                        ? $"↑ {report.RevenueChangePercentage:F1}% so với kỳ trước"
                        : $"↓ {Math.Abs(report.RevenueChangePercentage):F1}% so với kỳ trước";

                    Kpis = new()
                    {
                        new() { Title = "Tổng doanh thu", Value = $"{report.TotalRevenue:N0} đ", ChangeText = changeText, Icon = "fa-solid fa-sack-dollar", ColorClass = "blue" },
                        new() { Title = "Tổng số vé", Value = $"{report.TotalTickets:N0} vé", ChangeText = report.Trend, Icon = "fa-solid fa-ticket", ColorClass = "orange" },
                        new() { Title = "Trung bình/vé", Value = $"{report.AverageRevenuePerTicket:N0} đ", ChangeText = "", Icon = "fa-solid fa-calculator", ColorClass = "green" }
                    };

                    // Add vehicle type breakdown
                    if (report.RevenueByVehicleType != null)
                    {
                        foreach (var kvp in report.RevenueByVehicleType)
                        {
                            var ticketCount = report.TicketsByVehicleType?.GetValueOrDefault(kvp.Key, 0) ?? 0;
                            Kpis.Add(new StatisticsKpiCardViewModel
                            {
                                Title = kvp.Key,
                                Value = $"{kvp.Value:N0} đ",
                                ChangeText = $"{ticketCount} vé",
                                Icon = kvp.Key == "Xe máy" ? "fa-solid fa-motorcycle" : "fa-solid fa-car",
                                ColorClass = kvp.Key == "Xe máy" ? "cyan" : "purple"
                            });
                        }
                    }

                    // Build table from daily breakdown
                    Table = new StatisticsTableViewModel
                    {
                        Headers = new() { "Ngày", "Tổng doanh thu", "Số vé", "Trung bình/vé" },
                        Rows = new()
                    };

                    if (report.DailyBreakdown != null)
                    {
                        foreach (var day in report.DailyBreakdown.OrderByDescending(d => d.Date).Take(10))
                        {
                            Table.Rows.Add(new List<string>
                            {
                                day.Date.ToString("dd/MM/yyyy"),
                                $"{day.TotalRevenue:N0} đ",
                                $"{day.TicketCount} vé",
                                $"{day.AverageRevenuePerTicket:N0} đ"
                            });
                        }
                    }

                    PaymentMethodBreakdown = BuildBreakdown(report.RevenueByPaymentMethod);
                    VehicleTypeBreakdown = BuildBreakdown(report.RevenueByVehicleType);
                    Rankings = report.TopDays.Select((day, index) => new StatisticsRankItemViewModel
                    {
                        Rank = (index + 1).ToString(),
                        Label = day.Date.ToString("dd/MM/yyyy"),
                        Value = $"{day.TotalRevenue:N0} đ",
                        Note = $"{day.TicketCount:N0} vé"
                    }).ToList();

                    var dailyBreakdown = report.DailyBreakdown ?? new();
                    var revenueByPaymentMethod = report.RevenueByPaymentMethod ?? new();
                    var revenueByVehicleType = report.RevenueByVehicleType ?? new();

                    Charts = new RevenueStatisticsChartConfig
                    {
                        Line = new RevenueLineChartConfig
                        {
                            Labels = dailyBreakdown.OrderBy(x => x.Date).Select(x => x.Date.ToString("dd/MM")).ToList(),
                            Current = dailyBreakdown.OrderBy(x => x.Date).Select(x => x.TotalRevenue).ToList()
                        },
                        Donut = new RevenueSeriesChartConfig
                        {
                            Labels = revenueByPaymentMethod.Keys.ToList(),
                            Data = revenueByPaymentMethod.Values.ToList()
                        },
                        Bar = new RevenueSeriesChartConfig
                        {
                            Labels = revenueByVehicleType.Keys.ToList(),
                            Data = revenueByVehicleType.Values.ToList()
                        }
                    };
                }
                else
                {
                    SetEmptyData();
                }
            }
            else
            {
                SetEmptyData();
            }
        }

        private void SetEmptyData()
        {
            Kpis = new()
            {
                new() { Title = "Tổng doanh thu", Value = "0 đ", ChangeText = "Chưa có dữ liệu", Icon = "fa-solid fa-sack-dollar", ColorClass = "blue" },
                new() { Title = "Tổng số vé", Value = "0 vé", ChangeText = "", Icon = "fa-solid fa-ticket", ColorClass = "orange" },
                new() { Title = "Trung bình/vé", Value = "0 đ", ChangeText = "", Icon = "fa-solid fa-calculator", ColorClass = "green" }
            };

            Table = new StatisticsTableViewModel
            {
                Headers = new() { "Ngày", "Tổng doanh thu", "Số vé", "Trung bình/vé" },
                Rows = new()
            };
        }

        private static List<StatisticsBreakdownItemViewModel> BuildBreakdown(Dictionary<string, decimal>? values)
        {
            var total = values?.Values.Sum() ?? 0;
            var colors = new[] { "blue", "green", "purple", "orange", "cyan" };

            return (values ?? new())
                .OrderByDescending(x => x.Value)
                .Select((x, index) => new StatisticsBreakdownItemViewModel
                {
                    Label = x.Key,
                    Value = x.Value,
                    Percentage = total > 0 ? Math.Round(x.Value / total * 100, 1) : 0,
                    ColorClass = colors[index % colors.Length]
                })
                .ToList();
        }
    }
}

