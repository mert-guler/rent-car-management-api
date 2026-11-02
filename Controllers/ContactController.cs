using Microsoft.AspNetCore.Mvc;
using RentCarManagement.Api.Data;
using RentCarManagement.Api.Models;

namespace RentCarManagement.Api.Controllers;

[ApiController, Route("api/contact")]
public sealed class ContactController(RentCarDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(ContactRequest request)
    {
        var item = new SupportRequest { Name = request.Name.Trim(), Email = request.Email.Trim(), Subject = request.Subject.Trim(), Message = request.Message.Trim() };
        db.SupportRequests.Add(item);
        await db.SaveChangesAsync();
        return Accepted(new { item.Id, item.CreatedAt, Message = "Your message has been received." });
    }
}
