using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Controllers;

public class ErrorsController : Controller
{
    // Chamado pelo UseExceptionHandler (exceções não tratadas)
    [Route("Errors/Error")]
    public IActionResult Error()
    {
        Response.StatusCode = 500;
        return View("Error");
    }

    // Chamado pelo UseStatusCodePagesWithReExecute (404, 403, ...)
    [Route("Errors/{statusCode:int}")]
    public IActionResult StatusCodeHandler(int statusCode)
    {
        ViewBag.StatusCode = statusCode;
        return statusCode == 404 ? View("NotFound") : View("Error");
    }
}
