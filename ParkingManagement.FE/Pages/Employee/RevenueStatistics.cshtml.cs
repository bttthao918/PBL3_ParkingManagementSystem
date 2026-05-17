using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
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

        [BindProperty(SupportsGet = true)]
        public string Period { get; set; } = "30days";

        public StatisticsHeaderViewModel Header { get; set; } = new();
        public List<StatisticsKpiCardViewModel> Kpis { get; set; } = new();
        public StatisticsTableViewModel Table { get; set; } = new();
<<<<<<< HEAD
        public RevenueStatisticsChartConfig Charts { get; set; } = new();
        public List<StatisticsBreakdownItemViewModel> PaymentMethodBreakdown { get; set; } = new();
        public List<StatisticsBreakdownItemViewModel> VehicleTypeBreakdown { get; set; } = new();
        public List<StatisticsRankItemViewModel> Rankings { get; set; } = new();
        public string RankingTitle { get; set; } = "Top ngày doanh thu cao nhất";
=======
        public string ChartConfigJson { get; set; } = "{}";
        public string LineChartTitle { get; set; } = "Doanh thu theo ngày";
        public string DonutTitle { get; set; } = "Cơ cấu doanh thu theo loại xe";
        public string BarTitle { get; set; } = "Số vé theo loại xe";
        public string RankTitle { get; set; } = "Top 5 ngày doanh thu cao nhất";
        public string ProgressTitle { get; set; } = "Tỷ trọng doanh thu theo loại xe";
        public List<StatisticsBreakdownItemViewModel> DonutItems { get; set; } = new();
        public List<StatisticsBreakdownItemViewModel> BarItems { get; set; } = new();
        public List<StatisticsRankItemViewModel> RankItems { get; set; } = new();
        public List<StatisticsBreakdownItemViewModel> ProgressItems { get; set; } = new();
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Báo cáo doanh thu";
            ViewData["Role"] = "Nhân viên";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Nhân viên";

            Period = NormalizePeriod(Period);
            var employeeId = User.FindFirst("related_id")?.Value;
            if (string.IsNullOrEmpty(employeeId))
            {
                SetEmptyData();
                return;
            }

            var report = await _reportService.GetEmployeeRevenueReportAsync(employeeId, Period);
            if (report == null)
            {
                SetEmptyData();
                return;
            }

            Header = new StatisticsHeaderViewModel
            {
                Title = "Báo cáo doanh thu cá nhân",
                Description = "Thống kê doanh thu theo dữ liệu xử lý thực tế",
                DateRangeText = $"{report.PeriodStart:dd/MM/yyyy} - {report.PeriodEnd:dd/MM/yyyy}",
                ActivePeriod = Period
            };

            var changeText = report.RevenueChangePercentage >= 0
                ? $"+{report.RevenueChangePercentage:F1}% so với kỳ trước"
                : $"-{Math.Abs(report.RevenueChangePercentage):F1}% so với kỳ trước";

            Kpis = new()
            {
                new() { Title = "Tổng doanh thu", Value = $"{report.TotalRevenue:N0} đ", ChangeText = changeText, Icon = "fa-solid fa-sack-dollar", ColorClass = "blue" },
                new() { Title = "Tổng số vé", Value = $"{report.TotalTickets:N0} vé", ChangeText = report.Trend, Icon = "fa-solid fa-ticket", ColorClass = "orange" },
                new() { Title = "Trung bình/vé", Value = $"{report.AverageRevenuePerTicket:N0} đ", ChangeText = "Theo vé phát sinh trong kỳ", Icon = "fa-solid fa-calculator", ColorClass = "green" }
            };

<<<<<<< HEAD
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
=======
            Table = new StatisticsTableViewModel
>>>>>>> 29cb39c9e66b6e80c2371e7511d5036209209a10
            {
                Headers = new() { "Ngày", "Tổng doanh thu", "Số vé", "Trung bình/vé" },
                Rows = report.DailyBreakdown
                    .OrderByDescending(d => d.Date)
                    .Select(day => new List<string>
                    {
                        day.Date.ToString("dd/MM/yyyy"),
                        $"{day.TotalRevenue:N0} đ",
                        $"{day.TicketCount:N0} vé",
                        $"{day.AverageRevenuePerTicket:N0} đ"
                    })
                    .ToList()
            };

            DonutItems = BuildRevenueByVehicleItems(report);
            ProgressItems = DonutItems;
            BarItems = BuildTicketByVehicleItems(report);
            RankItems = report.TopDays
                .OrderByDescending(x => x.TotalRevenue)
                .Take(5)
                .Select((day, index) => new StatisticsRankItemViewModel
                {
                    Rank = index + 1,
                    Label = day.Date.ToString("dd/MM/yyyy"),
                    Value = $"{day.TotalRevenue:N0} đ",
                    ChangeText = $"{day.TicketCount:N0} vé"
                })
                .ToList();

            ChartConfigJson = BuildChartConfig(report);
        }

        private void SetEmptyData()
        {
            Header = new StatisticsHeaderViewModel
            {
                Title = "Báo cáo doanh thu cá nhân",
                Description = "Không thể tải dữ liệu doanh thu.",
                DateRangeText = "",
                ActivePeriod = Period
            };

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
            ChartConfigJson = BuildChartConfig(new EmployeeRevenueReportDto());
        }

        private static string NormalizePeriod(string? period)
        {
            return period?.Trim().ToLowerInvariant() switch
            {
                "today" or "day" => "today",
                "7days" or "week" => "7days",
                "month" => "month",
                "30days" => "30days",
                _ => "30days"
            };
        }

        private static List<StatisticsBreakdownItemViewModel> BuildRevenueByVehicleItems(EmployeeRevenueReportDto report)
        {
            var total = report.RevenueByVehicleType.Sum(x => x.Value);
            var colors = new[] { "blue", "green", "orange", "purple", "cyan" };
            if (report.RevenueByVehicleType.Count == 0)
            {
                return new List<StatisticsBreakdownItemViewModel>
                {
                    new() { Label = "Chưa có dữ liệu", Value = "0 đ", Percentage = 0, ColorClass = "blue" }
                };
            }

            return report.RevenueByVehicleType
                .OrderByDescending(x => x.Value)
                .Select((x, index) => new StatisticsBreakdownItemViewModel
                {
                    Label = x.Key,
                    Value = $"{x.Value:N0} đ",
                    Percentage = total <= 0 ? 0 : Math.Round(x.Value * 100m / total, 1),
                    ColorClass = colors[index % colors.Length]
                })
                .ToList();
        }

        private static List<StatisticsBreakdownItemViewModel> BuildTicketByVehicleItems(EmployeeRevenueReportDto report)
        {
            var total = report.TicketsByVehicleType.Sum(x => x.Value);
            var colors = new[] { "blue", "green", "orange", "purple", "cyan" };
            if (report.TicketsByVehicleType.Count == 0)
            {
                return new List<StatisticsBreakdownItemViewModel>
                {
                    new() { Label = "Chưa có dữ liệu", Value = "0 vé", Percentage = 0, ColorClass = "blue" }
                };
            }

            return report.TicketsByVehicleType
                .OrderByDescending(x => x.Value)
                .Select((x, index) => new StatisticsBreakdownItemViewModel
                {
                    Label = x.Key,
                    Value = $"{x.Value:N0} vé",
                    Percentage = total <= 0 ? 0 : Math.Round(x.Value * 100m / total, 1),
                    ColorClass = colors[index % colors.Length]
                })
                .ToList();
        }

        private static string BuildChartConfig(EmployeeRevenueReportDto report)
        {
            var config = new
            {
                type = "revenue",
                line = new
                {
                    labels = report.DailyBreakdown.Select(x => x.Date.ToString("dd/MM")).ToList(),
                    current = report.DailyBreakdown.Select(x => x.TotalRevenue).ToList(),
                    previous = Array.Empty<decimal>()
                },
                donut = new
                {
                    labels = report.RevenueByVehicleType.Select(x => x.Key).ToList(),
                    data = report.RevenueByVehicleType.Select(x => x.Value).ToList()
                },
                bar = new
                {
                    labels = report.TicketsByVehicleType.Select(x => x.Key).ToList(),
                    data = report.TicketsByVehicleType.Select(x => x.Value).ToList()
                }
            };

            return JsonSerializer.Serialize(config);
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
