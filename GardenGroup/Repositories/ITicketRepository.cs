using System.Net.Sockets;
using MongoDB.Bson;
using GardenGroup.Models;
using GardenGroup.Enums;
namespace GardenGroup.Repositories
{
    
        public interface ITicketRepository
        {
            Task<Ticket> CreateAsync(Ticket ticket);

            Task<Ticket?> GetByIdAsync(string id);
            // false als er geen ticket bestaat
            Task<bool> UpdateAsync(Ticket ticket);     
            Task<bool> UpdateStatusAsync(ObjectId id, TicketStatus newStatus);
            //status naar closed, soft delete
            Task<bool> CloseAsync(ObjectId id);
        }
    }

