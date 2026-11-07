// Author: Mart
using System.Text.RegularExpressions;
using GardenGroup.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GardenGroup.Repositories;

//Individual functionality (Mart): searching tickets with AND + OR, newest first.

public class TicketSearcher : ITicketSearcher
{
    private const int MaxPageSize = 100;
    private const int MaxSearchLength = 100;   // protects the database against huge search texts
    private const int MaxTokens = 20;

    private readonly IMongoCollection<Ticket> _tickets;

    public TicketSearcher(IMongoDatabase db) => _tickets = db.GetCollection<Ticket>("tickets");

    /// <inheritdoc />
    public async Task<List<Ticket>> SearchAsync(TicketListQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var groups = ParseSearchText(query.Search);
        if (groups.Count == 0)
        {
            return new List<Ticket>();
        }

        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
        var page = Math.Max(query.Page, 0);

        // (word AND word) OR (word AND word) ...
        var f = Builders<Ticket>.Filter;
        var textFilter = f.Or(groups.Select(group => f.And(group.Select(WordFilter))));

        // The role filter (reportedBy for a regular employee) is combined with AND, so a search can never
        // return tickets the user may not see.
        var filter = textFilter & TicketFilterBuilder.Build(query.Status, query.AssignedToId, query.ReportedById);

        return await _tickets.Find(filter)
            .Project<Ticket>(Builders<Ticket>.Projection.Exclude(t => t.Description))
            .Sort(Builders<Ticket>.Sort.Descending(t => t.CreatedAt).Descending(t => t.Id))
            .Skip(page * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Splits the search text into OR groups of AND words:
    /// "printer AND offline OR wifi" becomes [[printer, offline], [wifi]].
    /// Public and static so it can be unit tested without a database.
    /// </summary>
    public static List<List<string>> ParseSearchText(string? text)
    {
        var groups = new List<List<string>>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return groups;
        }

        if (text.Length > MaxSearchLength)
        {
            text = text[..MaxSearchLength];
        }

        var current = new List<string>();
        foreach (var token in text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Take(MaxTokens))
        {
            if (token == "OR")
            {
                if (current.Count > 0)
                {
                    groups.Add(current);
                    current = new List<string>();
                }
            }
            else if (token != "AND")   // AND is the default between two words, so it is only skipped
            {
                current.Add(token);
            }
        }

        if (current.Count > 0)
        {
            groups.Add(current);
        }

        return groups;
    }

    // Regex.Escape makes the word literal: a user typing "(" or ".*" cannot inject a regular expression.
    private static FilterDefinition<Ticket> WordFilter(string word)
    {
        var f = Builders<Ticket>.Filter;
        var regex = new BsonRegularExpression(Regex.Escape(word), "i");
        return f.Or(f.Regex(t => t.Title, regex), f.Regex(t => t.Description, regex));
    }
}