using InertiaSharp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InertiaSharp.Sample.Controllers;

[Route("")]
public class HomeController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
        => User.Identity?.IsAuthenticated == true
            ? Redirect("/dashboard")
            : Redirect("/login");

    [HttpGet("403")]
    [AllowAnonymous]
    public IActionResult Forbidden()
        => this.Inertia("Errors/Forbidden");

    [HttpGet("error")]
    [AllowAnonymous]
    public IActionResult Error()
        => this.Inertia("Errors/ServerError");
}
