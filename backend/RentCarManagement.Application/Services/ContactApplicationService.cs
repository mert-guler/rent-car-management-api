using RentCarManagement.Application.Abstractions;
using RentCarManagement.Application.Contracts;
using RentCarManagement.Domain.Entities;

namespace RentCarManagement.Application.Services;

public sealed class ContactApplicationService(IContactRepository contacts) : IContactApplicationService
{
    public async Task<ContactReceipt> CreateAsync(ContactRequest request)
    {
        var supportRequest = new SupportRequest
        {
            Name = request.Name.Trim(), Email = request.Email.Trim(),
            Subject = request.Subject.Trim(), Message = request.Message.Trim()
        };
        contacts.Add(supportRequest);
        await contacts.SaveChangesAsync();
        return new ContactReceipt(supportRequest.Id, supportRequest.CreatedAt, "Your message has been received.");
    }
}
