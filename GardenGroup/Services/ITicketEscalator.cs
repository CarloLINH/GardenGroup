// Author: Emre
namespace GardenGroup.Services;

public enum EscalationResult
{
    Done,
    NotFound,
    NotAllowed
}

// Individual functionality (Emre): escalate a ticket to management or close it.
public interface ITicketEscalator
{
    // Sends an open ticket to management (status Escalated).
    EscalationResult Escalate(string ticketId);

    // Closes a ticket that is not closed yet.
    EscalationResult Close(string ticketId);
}
