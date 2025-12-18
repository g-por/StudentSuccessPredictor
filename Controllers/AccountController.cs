using Microsoft.AspNetCore.Mvc;

namespace StudentSuccessPredictor.Controllers;

public class AccountController : Controller
{
    private const string DemoLogin = "teacher";
    private const string DemoPassword = "12345";

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(string login, string password)
    {
        if (login == DemoLogin && password == DemoPassword)
        {
            HttpContext.Session.SetString("TEACHER", login);
            return RedirectToAction("Index", "Journal");
        }

        ViewBag.Error = "Невірний логін або пароль.";
        return View();
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("TEACHER");
        return RedirectToAction("Index", "Home");
    }
}
