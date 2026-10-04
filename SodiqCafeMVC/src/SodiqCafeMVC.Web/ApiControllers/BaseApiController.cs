using Microsoft.AspNetCore.Mvc;
using SodiqCafeMVC.Application.Common.Models;

namespace SodiqCafeMVC.Web.ApiControllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult ApiOk<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.Ok(data, message));
    }

    protected IActionResult ApiFail(string message, int statusCode = 400)
    {
        return StatusCode(statusCode, ApiResponse<object>.Fail(message));
    }
}
