using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models.ViewModels;
using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Pages.Employee
{
        [Authorize(Roles = "Employee")]
    public class CustomerManagementModel : PageModel
        {
            private readonly Services.ICustomerApiService _customerService;

            public CustomerManagementModel(Services.ICustomerApiService customerService)
            {
                _customerService = customerService;
            }

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

            public async Task OnGetAsync()
            {
                var filter = new EmployeeCustomerSearchFilterDto
                {
                    SearchKeyword = Search ?? "",
                    PageNumber = 1,
                    PageSize = 100
                };

                var result = await _customerService.SearchForEmployeeAsync(filter);

                if (result != null && result.Items != null)
                {
                    Customers = result.Items.Select((c, index) => new CustomerItemVM
                    {
                        Id = index + 1, // Temporary, until BE returns integer IDs or FE uses string ID
                        FullName = c.FullName,
                        CustomerCode = c.CustomerId,
                        Phone = c.PhoneNumber,
                        MainPlate = "-", // BE currently doesn't return this in list
                        VehicleCount = c.TotalTickets > 0 ? 1 : 0, 
                        VehicleTooltip = "",
                        VehicleType = "-",
                        VipLevel = c.HasActiveMonthlyTicket ? "Tháng" : "Thường",
                        StatusText = c.LastVisit.HasValue ? c.LastVisit.Value.ToString("dd/MM/yyyy") : "Chưa gửi",
                        StatusClass = c.HasActiveMonthlyTicket ? "parking" : "left"
                    }).ToList();

                    TotalCustomers = result.TotalItems;
                    ActiveCustomers = result.Items.Count(x => x.HasActiveMonthlyTicket);
                    VipCustomers = result.Items.Count(x => x.HasActiveMonthlyTicket);
                    NewCustomers = 0;
                }
                
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
                        Email = "-",
                        Address = "-",
                        DateOfBirth = "-",
                        RegisterDate = "-",
                        VipLevel = selected.VipLevel,
                        TotalSpent = 0,
                        TotalTickets = 0, // Should be fetched from detail endpoint if BE had one
                        DiscountPercent = selected.VipLevel == "Thường" ? 0 : 10,
                        VipProgress = 0,
                        AmountToNextLevel = 0,
                        Vehicles = new List<CustomerVehicleVM>(),
                        Histories = new List<CustomerParkingHistoryVM>()
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

