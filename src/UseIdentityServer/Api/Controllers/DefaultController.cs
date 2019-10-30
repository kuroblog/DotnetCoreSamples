using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
public class DefaultController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return await Task.FromResult(new JsonResult("Hello, World!"));
    }

    [Authorize(Roles = "superadmin")]
    [HttpGet, Route("sadmin")]
    public async Task<IActionResult> SuperAdmin()
    {
        return await Task.FromResult(new JsonResult("SuperAdmin"));
    }

    [Authorize(Roles = "admin")]
    [HttpGet, Route("admin")]
    public async Task<IActionResult> Admin()
    {
        return await Task.FromResult(new JsonResult("Admin"));
    }
}