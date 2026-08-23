using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using pharmacystock.Models.ViewModels;

namespace pharmacystock.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // =========================
        // LOGIN
        // =========================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError("", "Email veya şifre hatalı.");

            return View(model);
        }

        // =========================
        // REGISTER
        // =========================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Aynı e-posta ile kayıt var mı?
            var existingEmail = await _userManager.FindByEmailAsync(model.Email);

            if (existingEmail != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "Bu e-posta adresi zaten kullanılıyor.");

                return View(model);
            }

            // Aynı kullanıcı adı var mı?
            var existingUserName = await _userManager.FindByNameAsync(model.UserName);

            if (existingUserName != null)
            {
                ModelState.AddModelError(
                    "UserName",
                    "Bu kullanıcı adı zaten kullanılıyor.");

                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(
                user,
                model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);

                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // =========================
        // FORGOT PASSWORD
        // =========================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Lütfen e-posta adresinizi giriniz.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ViewBag.Error = "Bu e-posta adresine ait kullanıcı bulunamadı.";
                return View();
            }

            var token =
                await _userManager.GeneratePasswordResetTokenAsync(user);

            return RedirectToAction(
                nameof(ResetPassword),
                new
                {
                    email = user.Email,
                    token = token
                });
        }

        // =========================
        // RESET PASSWORD
        // =========================

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(nameof(Login));
            }

            ViewBag.Email = email;
            ViewBag.Token = token;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            string email,
            string token,
            string password,
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Yeni şifre boş bırakılamaz.";
                ViewBag.Email = email;
                ViewBag.Token = token;

                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Şifreler eşleşmiyor.";
                ViewBag.Email = email;
                ViewBag.Token = token;

                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                token,
                password);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] =
                    "Şifreniz başarıyla değiştirildi. Yeni şifrenizle giriş yapabilirsiniz.";

                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.Email = email;
            ViewBag.Token = token;

            return View();
        }

        // =========================
        // LOGOUT
        // =========================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(Login));
        }
    }
}