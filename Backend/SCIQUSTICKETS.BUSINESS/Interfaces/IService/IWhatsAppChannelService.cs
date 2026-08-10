using System;
using System.Threading.Tasks;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
    public interface IWhatsAppChannelService
    {
        Task ProcessInboxMessageAsync(WhatsAppInboxMessage message);
        Task<bool> SendOutboundReplyAsync(Guid ticketId, string body, string? templateName, string? sentByUserId);
        bool ValidateWebhookSignature(string payload, string signature, string provider);
    }
}
