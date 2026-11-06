using GardenGroup.Repositories;
using MongoDB.Driver;

namespace GardenGroup.Services;

public sealed class TicketTransferService : ITicketTransferService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public TicketTransferService(
        ITicketRepository ticketRepository,
        IEmployeeRepository employeeRepository)
    {
        _ticketRepository = ticketRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<UpdateResult> TransferAsync(
        string ticketId,
        int employeeId)
    {
        if (!await IsActiveEmployeeAsync(employeeId))
        {
            throw new InvalidOperationException(
                "The selected employee does not exist or is inactive.");
        }

        var result = _ticketRepository.TransferTicket(
            ticketId,
            employeeId);

        if (result.MatchedCount == 0)
        {
            var ticket = _ticketRepository.GetTicketById(ticketId);

            if (ticket is null)
            {
                throw new KeyNotFoundException(
                    "The ticket could not be found.");
            }

            // De ticket was already assigned to this employee.
            return result;
        }

        return result;
    }

    private async Task<bool> IsActiveEmployeeAsync(int employeeId)
    {
        var employee = await _employeeRepository.GetByEmployeeIdAsync(employeeId);

        return employee is not null && employee.Active;
    }
}