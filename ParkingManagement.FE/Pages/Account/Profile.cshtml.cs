using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ParkingManagement.FE.Pages.Account;
public class ProfileModel : PageModel
{
    public AccountViewModel Account { get; set; } = new();

    [BindProperty]
    public EditProfileInputModel Input { get; set; } = new();

    public void OnGet()
    {
        LoadAccount();

        Input = new EditProfileInputModel
        {
            FullName = Account.FullName,
            Email = Account.Email,
            PhoneNumber = Account.PhoneNumber,
            Gender = Account.Gender
        };
    }

    public IActionResult OnPost()
    {
        LoadAccount();

        if (!ModelState.IsValid)
            return Page();

        // TODO: update database tại đây
        // Ví dụ:
        // user.FullName = Input.FullName;
        // user.Email = Input.Email;
        // user.PhoneNumber = Input.PhoneNumber;
        // user.Gender = Input.Gender;
        // await _context.SaveChangesAsync();

        TempData["Success"] = "Cập nhật thông tin thành công.";
        return RedirectToPage();
    }

    private void LoadAccount()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Admin";

        Account = new AccountViewModel
        {
            FullName = User.Identity?.Name ?? "Admin",
            Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "admin@parking.com",
            PhoneNumber = "0123 456 789",
            RoleName = role,
            Gender = "Nam",
            CreatedAt = new DateTime(2026, 1, 1, 10, 30, 0)
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
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    public string PhoneNumber { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng chọn giới tính")]
    public string Gender { get; set; } = "Nam";
}