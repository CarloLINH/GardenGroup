// Author: Emre
using GardenGroup.Enums;
using GardenGroup.Repositories;
using MongoDB.Bson;

namespace GardenGroup.Services;

public class TicketEscalator : ITicketEscalator
{
    private readonly ITicketRepository _tickets;

    public TicketEscalator(ITicketRepository tickets) => _tickets = tickets;
    public static bool CanEscalate(Statuses status) => status is Statuses.Open or Statuses.In_Progress;

    public static bool CanClose(Statuses status) => status != Statuses.Closed;

    public EscalationResult Escalate(string ticketId) => Change(ticketId, CanEscalate, Statuses.Escalated);

    public EscalationResult Close(string ticketId) => Change(ticketId, CanClose, Statuses.Closed);

    private EscalationResult Change(string ticketId, Func<Statuses, bool> isAllowed, Statuses target)
    {
        if (string.IsNullOrWhiteSpace(ticketId) || !ObjectId.TryParse(ticketId, out _))
        {
            return EscalationResult.NotFound;
        }

        var ticket = _tickets.GetTicketById(ticketId);
        if (ticket is null)
        {
            return EscalationResult.NotFound;
        }

        if (!isAllowed(ticket.Status))
        {
            return EscalationResult.NotAllowed;
        }

        return _tickets.UpdateStatus(ticketId, ticket.Status, target)
            ? EscalationResult.Done
            : EscalationResult.NotAllowed;
    }
}
