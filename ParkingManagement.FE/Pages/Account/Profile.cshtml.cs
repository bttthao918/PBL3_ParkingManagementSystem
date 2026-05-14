using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ParkingManagement.FE.Pages.Account;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly IAuthService _authService;

    public ProfileModel(IAuthService authService)
    {
        _authService = authService;
    }

    public AccountViewModel Account { get; set; } = new();

    [BindProperty]
    public EditProfileInputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAccountAsync();
        PopulateInputFromAccount();

        ViewData["Title"] = "Thông tin tài khoản";
        ViewData["UserName"] = Account.FullName;
        ViewData["Role"] = Account.RoleName switch
        {
            "Employee" => "Nhân viên",
            "Manager" => "Quản lý",
            "Customer" => "Khách hàng",
            _ => Account.RoleName
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadAccountAsync();

        if (!ModelState.IsValid)
            return Page();

        var result = await _authService.UpdateCurrentUserAsync(new UpdateCurrentUserRequest
        {
            FullName = Input.FullName.Trim(),
            PhoneNumber = Input.PhoneNumber.Trim(),
            Gender = Input.Gender
        });

        if (!result.Success)
        {
            ErrorMessage = result.Message;
            return Page();
        }

        TempData["Success"] = result.Message;
        return RedirectToPage();
    }

    private void PopulateInputFromAccount()
    {
        Input = new EditProfileInputModel
        {
            FullName = Account.FullName,
            PhoneNumber = Account.PhoneNumber,
            Gender = Account.Gender
        };
    }

    private async Task LoadAccountAsync()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        var currentUser = await _authService.GetCurrentUserAsync();

        if (currentUser != null)
        {
            Account = new AccountViewModel
            {
                FullName = currentUser.FullName,
                Email = currentUser.Email,
                PhoneNumber = currentUser.PhoneNumber ?? "",
                RoleName = currentUser.Role,
                Gender = AccountViewModel.NormalizeGender(currentUser.Gender),
                CreatedAt = currentUser.CreatedAt == default ? DateTime.Now : currentUser.CreatedAt
            };
            return;
        }

        Account = new AccountViewModel
        {
            FullName = User.FindFirst(ClaimTypes.Name)?.Value ?? "User",
            Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "",
            PhoneNumber = "",
            RoleName = role,
            Gender = "",
            CreatedAt = DateTime.Now
        };
    }
}

public class AccountViewModel
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string RoleName { get; set; } = "";
    public string Gender { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public string PhoneNumberDisplay => string.IsNullOrWhiteSpace(PhoneNumber) ? "-" : PhoneNumber;

    public string GenderDisplay => Gender switch
    {
        "Male" => "Nam",
        "Female" => "Nữ",
        "Other" => "Khác",
        _ => string.IsNullOrWhiteSpace(Gender) ? "-" : Gender
    };

    public static string NormalizeGender(string? gender)
    {
        if (string.IsNullOrWhiteSpace(gender))
            return "";

        return gender.Trim().ToLowerInvariant() switch
        {
            "male" or "nam" => "Male",
            "female" or "nu" or "nữ" => "Female",
            "other" or "khac" or "khác" => "Other",
            _ => gender.Trim()
        };
    }
}

public class EditProfileInputModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Họ và tên phải có từ 3 đến 100 ký tự")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^\+?[0-9]{9,15}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string PhoneNumber { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng chọn giới tính")]
    public string Gender { get; set; } = "";
}
