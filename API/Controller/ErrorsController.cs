using API.Helpers.Errors;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller;

[Route("errors/{code}")]
public class ErrorsController : BaseApiController
{
    public IActionResult Error(int code)
    {
        return new ObjectResult(new ApiResponse(code));
    }
}