namespace GardenGroup.Enums
{
    public enum TicketStatus { Open, Resolved, Closed }
    public enum TicketPriority { Low = 1, Medium = 2, High = 3, Critical = 4 }
    public enum Role { Regular, ServiceDesk }
    public enum TicketCategory { Open, Resolved, ClosedWithoutResolve }
    // Sorteervolgorde van een ticketlijst op prioriteit (dropdown "Sorteren op prioriteit").
    public enum TicketSortOrder { PriorityHighToLow = 0, PriorityLowToHigh = 1 }
}
