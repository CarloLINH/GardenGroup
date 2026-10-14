namespace GardenGroup.Enums
{
    public class Enums
    {
        
        public enum TicketStatus { Open, Resolved, Closed }
        public enum TicketPriority { Low = 1, Medium = 2, High = 3, Critical = 4 }
        public enum Role { Regular, ServiceDesk }
        public enum TicketCategory { Open, Resolved, ClosedWithoutResolve }
    }
}
