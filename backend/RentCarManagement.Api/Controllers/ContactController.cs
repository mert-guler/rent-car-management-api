using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RentCarManagement.Application.Abstractions;
using RentCarManagement.Application.Contracts;

namespace RentCarManagement.Api.Controllers;

[ApiController, Route("api/contact")]
public sealed class ContactController(IContactApplicationService contact) : ControllerBase
{
    [HttpPost, EnableRateLimiting("contact")]
    public async Task<IActionResult> Create(ContactRequest request) => Accepted(await contact.CreateAsync(request));
}
