using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
public class DefaultController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return await Task.FromResult(new JsonResult("Hello, World!"));
    }
}