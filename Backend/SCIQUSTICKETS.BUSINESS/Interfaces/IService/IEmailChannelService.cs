using System;
using System.Threading.Tasks;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
    public interface IEmailChannelService
    {
        Task ProcessInboxMessageAsync(EmailInboxMessage message);
        Task<bool> SendOutboundReplyAsync(Guid ticketId, string body, string? sentByUserId);
        Task SyncMailboxesAsync();
    }
}
