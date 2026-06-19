using KMCEventSphereWeb.Models;
using KMCEventSphereWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Net.WebRequestMethods;

namespace KMCEventSphereWeb.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly UserService _userService;

        public UsersModel(UserService userService)
        {
            _userService = userService;
        }

        public List<UserModel> Users { get; set; } = new();

        public int CurrentUserId { get; set; } 

        [TempData]
        public string? ErrorMessage { get; set; }

        [TempData]
        public string? Message { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Filter { get; set; } = "key";

        [BindProperty(SupportsGet = true)]
        public string Value { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {
            // LOGIN CHECK
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                ErrorMessage = "User not logged in";
                return RedirectToPage("/Login");
            }

            CurrentUserId = HttpContext.Session.GetInt32("UserID") ?? 0;

            // IF SEARCH USED
            if (!string.IsNullOrEmpty(Value))
            {
                var result = await _userService.SearchUsersAsync(Filter, Value);

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    ErrorMessage = result.ErrorMessage;

                    // If API says not logged in → redirect
                    if (ErrorMessage.Contains("not logged in", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToPage("/Login");
                    }

                    return Page();
                }

                Users = result.Users ?? new List<UserModel>();
                return Page();
            }

            // NORMAL LOAD
            var normal = await _userService.GetAllUsersAsync();

            if (!string.IsNullOrEmpty(normal.ErrorMessage))
            {
                ErrorMessage = normal.ErrorMessage;

                if (ErrorMessage.Contains("not logged in", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToPage("/Login");
                }

                return Page();
            }

            Users = normal.Users ?? new List<UserModel>();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToPage("/Login");
            }

            CurrentUserId = HttpContext.Session.GetInt32("UserID") ?? 0;

            var (success, message) = await _userService.DeleteUserAsync(id);

            if (!success)
            {
                ErrorMessage = message;
                Message = null;
            }
            else
            {
                Message = message;
                ErrorMessage = null;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateRoleAsync(int id, string role)
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
                return RedirectToPage("/Login");

            CurrentUserId = HttpContext.Session.GetInt32("UserID") ?? 0;

            // SAFETY CHECK
            if (id == CurrentUserId)
            {
                ErrorMessage = "You cannot update your own role";
                return RedirectToPage();
            }

            // CALL SERVICE 
            var (success, message) = await _userService.UpdateUserRoleAsync(id, role);

            if (!success)
            {
                ErrorMessage = message;
                Message = null;
            }
            else
            {
                Message = message;
                ErrorMessage = null;
            }

            return RedirectToPage(); 
        }
    }
}