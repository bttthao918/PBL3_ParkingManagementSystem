using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagement.FE.Models;
using ParkingManagement.FE.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ParkingManagement.FE.Pages.Account;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly IAccountProfileService _profileService;

    public ProfileModel(IAccountProfileService profileService)
    {
        _profileService = profileService;
    }

    public AccountViewModel Account { get; set; } = new();

    [BindProperty]
    public EditProfileInputModel Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadAccountAsync();
        FillInputFromAccount();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadAccountAsync();

        if (!ModelState.IsValid)
            return Page();

        var result = await _profileService.UpdateProfileAsync(new UpdateAccountProfileDto
        {
            FullName = Input.FullName.Trim(),
            Email = Input.Email.Trim(),
            PhoneNumber = Input.PhoneNumber.Trim(),
            Gender = Input.Gender
        });

        if (result?.Success != true)
        {
            TempData["Error"] = result?.Message ?? "Không thể cập nhật thông tin tài khoản.";
            return RedirectToPage();
        }

        if (result.Data != null)
        {
            HttpContext.Session.SetString("full_name", result.Data.FullName);
            HttpContext.Session.SetString("email", result.Data.Email);
        }

        TempData["Success"] = result.Message;
        return RedirectToPage();
    }

    private async Task LoadAccountAsync()
    {
        var profile = await _profileService.GetProfileAsync();
        if (profile != null)
        {
            Account = new AccountViewModel
            {
                FullName = profile.FullName,
                Email = profile.Email,
                PhoneNumber = profile.PhoneNumber,
                RoleName = profile.RoleName,
                Gender = profile.Gender,
                CreatedAt = profile.CreatedAt
            };
            return;
        }

        TempData["Error"] = _profileService.LastRequestUnauthorized
            ? "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
            : "Không tải được thông tin tài khoản từ database.";

        Account = new AccountViewModel
        {
            FullName = User.Identity?.Name ?? "",
            Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "",
            PhoneNumber = "",
            RoleName = User.FindFirst(ClaimTypes.Role)?.Value ?? "",
            Gender = "Khác",
            CreatedAt = DateTime.UtcNow
        };
    }

    private void FillInputFromAccount()
    {
        Input = new EditProfileInputModel
        {
            FullName = Account.FullName,
            Email = Account.Email,
            PhoneNumber = Account.PhoneNumber,
            Gender = Account.Gender
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
}

public class EditProfileInputModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    [MaxLength(100)]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(100)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^(0|\+84)[0-9]{9}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string PhoneNumber { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng chọn giới tính")]
    public string Gender { get; set; } = "Khác";
}
