using MongoDB.Driver;

namespace GardenGroup.Services
{
    public interface ITicketTransferService
    {
        Task<UpdateResult> TransferAsync(string ticketId, int employeeId);
    }
}
