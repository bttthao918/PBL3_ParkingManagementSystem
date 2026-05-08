using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingManagement.FE.Pages.Employee
{
        [Authorize(Roles = "Employee")]
    public class CustomerManagementModel : PageModel
        {
            public int TotalCustomers { get; set; }
            public int ActiveCustomers { get; set; }
            public int VipCustomers { get; set; }
            public int NewCustomers { get; set; }

            [BindProperty(SupportsGet = true)]
            public string? Search { get; set; }

            [BindProperty(SupportsGet = true)]
            public string? StatusFilter { get; set; }

            [BindProperty(SupportsGet = true)]
            public string? VehicleFilter { get; set; }

            [BindProperty(SupportsGet = true)]
            public string? VipFilter { get; set; }

            [BindProperty(SupportsGet = true)]
            public DateTime? RegisterDate { get; set; }

            [BindProperty(SupportsGet = true)]
            public int? SelectedId { get; set; }

            public List<CustomerItemVM> Customers { get; set; } = new List<CustomerItemVM>();

            public CustomerDetailVM? SelectedCustomer { get; set; }

            public void OnGet()
            {
                Customers = new List<CustomerItemVM>
            {
                new CustomerItemVM
                {
                    Id = 1,
                    FullName = "Nguyễn Văn A",
                    CustomerCode = "KH001",
                    Phone = "0905 123 456",
                    MainPlate = "43A-12345",
                    VehicleCount = 3,
                    VehicleTooltip = "43A-56789, 43A-99999",
                    VehicleType = "Ô tô",
                    VipLevel = "Gold",
                    StatusText = "Đang gửi xe",
                    StatusClass = "parking"
                },
                new CustomerItemVM
                {
                    Id = 2,
                    FullName = "Trần Thị B",
                    CustomerCode = "KH002",
                    Phone = "0912 345 678",
                    MainPlate = "43B-67890",
                    VehicleCount = 2,
                    VehicleTooltip = "43B-22222",
                    VehicleType = "Xe máy",
                    VipLevel = "Silver",
                    StatusText = "Đang gửi xe",
                    StatusClass = "parking"
                },
                new CustomerItemVM
                {
                    Id = 3,
                    FullName = "Lê Quang C",
                    CustomerCode = "KH003",
                    Phone = "0934 567 890",
                    MainPlate = "43A-56789",
                    VehicleCount = 4,
                    VehicleTooltip = "43A-99999, 43A-10101, 43A-20202",
                    VehicleType = "Ô tô",
                    VipLevel = "Platinum",
                    StatusText = "Đã rời bãi",
                    StatusClass = "left"
                },
                new CustomerItemVM
                {
                    Id = 4,
                    FullName = "Phạm Thị D",
                    CustomerCode = "KH004",
                    Phone = "0987 654 321",
                    MainPlate = "43C-24680",
                    VehicleCount = 1,
                    VehicleTooltip = "",
                    VehicleType = "Xe máy",
                    VipLevel = "Silver",
                    StatusText = "Nợ phí",
                    StatusClass = "debt"
                },
                new CustomerItemVM
                {
                    Id = 5,
                    FullName = "Hoàng Văn E",
                    CustomerCode = "KH005",
                    Phone = "0333 222 111",
                    MainPlate = "43A-11111",
                    VehicleCount = 1,
                    VehicleTooltip = "",
                    VehicleType = "Ô tô",
                    VipLevel = "Thường",
                    StatusText = "Đang gửi xe",
                    StatusClass = "parking"
                }
            };

                if (!string.IsNullOrWhiteSpace(Search))
                {
                    Customers = Customers
                        .Where(x =>
                            x.FullName.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                            x.CustomerCode.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                            x.Phone.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                            x.MainPlate.Contains(Search, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(StatusFilter))
                {
                    Customers = Customers
                        .Where(x => x.StatusClass.Equals(StatusFilter.ToLower(), StringComparison.OrdinalIgnoreCase)
                                 || x.StatusText.Contains(StatusFilter, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(VehicleFilter))
                {
                    var vehicleText = VehicleFilter == "Car" ? "Ô tô" : "Xe máy";
                    Customers = Customers.Where(x => x.VehicleType == vehicleText).ToList();
                }

                if (!string.IsNullOrWhiteSpace(VipFilter))
                {
                    var vipText = VipFilter == "Normal" ? "Thường" : VipFilter;
                    Customers = Customers.Where(x => x.VipLevel == vipText).ToList();
                }

                TotalCustomers = 1248;
                ActiveCustomers = 320;
                VipCustomers = 186;
                NewCustomers = 42;

                var selectedCustomerId = SelectedId ?? 1;
                var selected = Customers.FirstOrDefault(x => x.Id == selectedCustomerId)
                               ?? Customers.FirstOrDefault();

                if (selected != null)
                {
                    SelectedCustomer = new CustomerDetailVM
                    {
                        Id = selected.Id,
                        FullName = selected.FullName,
                        CustomerCode = selected.CustomerCode,
                        Phone = selected.Phone,
                        Email = "nguyenvana@gmail.com",
                        Address = "123 Lê Duẩn, Hải Châu, Đà Nẵng",
                        DateOfBirth = "12/05/1990",
                        RegisterDate = "01/01/2023",
                        VipLevel = selected.VipLevel,
                        TotalSpent = 8500000,
                        TotalTickets = 245,
                        DiscountPercent = selected.VipLevel == "Thường" ? 0 : 10,
                        VipProgress = 85,
                        AmountToNextLevel = 1500000,
                        Vehicles = new List<CustomerVehicleVM>
                    {
                        new CustomerVehicleVM
                        {
                            PlateNumber = "43A-12345",
                            Type = "Ô tô",
                            IsActive = true
                        },
                        new CustomerVehicleVM
                        {
                            PlateNumber = "43A-56789",
                            Type = "Ô tô",
                            IsActive = false
                        },
                        new CustomerVehicleVM
                        {
                            PlateNumber = "43A-99999",
                            Type = "Ô tô",
                            IsActive = false
                        }
                    },
                        Histories = new List<CustomerParkingHistoryVM>
                    {
                        new CustomerParkingHistoryVM
                        {
                            Date = "20/05/2024",
                            CheckIn = "08:10",
                            CheckOut = "11:45",
                            Fee = 25000
                        },
                        new CustomerParkingHistoryVM
                        {
                            Date = "18/05/2024",
                            CheckIn = "09:20",
                            CheckOut = "10:30",
                            Fee = 20000
                        },
                        new CustomerParkingHistoryVM
                        {
                            Date = "16/05/2024",
                            CheckIn = "07:50",
                            CheckOut = "12:10",
                            Fee = 30000
                        }
                    }
                    };
                }
            }
        }

        [Authorize(Roles = "Employee")]
    public class CustomerItemVM
        {
            public int Id { get; set; }
            public string FullName { get; set; } = "";
            public string CustomerCode { get; set; } = "";
            public string Phone { get; set; } = "";
            public string MainPlate { get; set; } = "";
            public int VehicleCount { get; set; }
            public string VehicleTooltip { get; set; } = "";
            public string VehicleType { get; set; } = "";
            public string VipLevel { get; set; } = "Thường";
            public string StatusText { get; set; } = "";
            public string StatusClass { get; set; } = "";
        }

        [Authorize(Roles = "Employee")]
    public class CustomerDetailVM
        {
            public int Id { get; set; }
            public string FullName { get; set; } = "";
            public string CustomerCode { get; set; } = "";
            public string Phone { get; set; } = "";
            public string Email { get; set; } = "";
            public string Address { get; set; } = "";
            public string DateOfBirth { get; set; } = "";
            public string RegisterDate { get; set; } = "";
            public string VipLevel { get; set; } = "Thường";
            public decimal TotalSpent { get; set; }
            public int TotalTickets { get; set; }
            public int DiscountPercent { get; set; }
            public int VipProgress { get; set; }
            public decimal AmountToNextLevel { get; set; }

            public List<CustomerVehicleVM> Vehicles { get; set; } = new List<CustomerVehicleVM>();
            public List<CustomerParkingHistoryVM> Histories { get; set; } = new List<CustomerParkingHistoryVM>();
        }

        [Authorize(Roles = "Employee")]
    public class CustomerVehicleVM
        {
            public string PlateNumber { get; set; } = "";
            public string Type { get; set; } = "";
            public bool IsActive { get; set; }
        }

        [Authorize(Roles = "Employee")]
    public class CustomerParkingHistoryVM
        {
            public string Date { get; set; } = "";
            public string CheckIn { get; set; } = "";
            public string CheckOut { get; set; } = "";
            public decimal Fee { get; set; }
        }
    }

