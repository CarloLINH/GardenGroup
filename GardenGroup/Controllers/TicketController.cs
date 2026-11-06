using GardenGroup.Enums;
using GardenGroup.Extensions;
using GardenGroup.Models;
using GardenGroup.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using System.Net.NetworkInformation;

namespace GardenGroup.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;
        private const int PageSize = 100;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        public async Task<ActionResult> Index(Statuses? status, int? assignedTo, string? q, string sort = TicketSort.Newest, int page = 0)
        {
            int? reportedBy = null;

            if (!User.IsServiceDesk())
            {
                reportedBy = User.GetEmployeeId();
                if (reportedBy == null)
                {
                    return Forbid();
                }
                assignedTo = null;
            }

            page = Math.Max(page, 0);
            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

            var tickets = await _ticketService.GetTicketsAsync(new TicketListQuery
            {
                Status = status,
                AssignedToId = assignedTo,
                ReportedById = reportedBy,
                Search = q,
                Sort = sort,
                Page = page,
                PageSize = PageSize
            });

            var paged = q is not null || sort is TicketSort.PriorityDesc or TicketSort.PriorityAsc;

            ViewBag.Paged = paged;
            ViewBag.HasNext = paged && tickets.Count == PageSize;
            ViewBag.Status = status;
            ViewBag.AssignedTo = assignedTo;
            ViewBag.Search = q;
            ViewBag.Sort = sort;
            ViewBag.Page = page;

            return View(tickets);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                _ticketService.CreateTicket(ticket);
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        public ActionResult Edit(string id)
        {
            var ticket = _ticketService.GetTicketById(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Id = id;
                _ticketService.UpdateTicket(ticket);
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        public ActionResult Delete(string id)
        {
            var ticket = _ticketService.GetTicketById(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            _ticketService.DeleteTicket(id);
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Details(string id)
        {
            try
            {
                MongoDB.Bson.ObjectId.Parse(id);
            }
            catch (FormatException)
            {
                return BadRequest("The ticket identifier is invalid.");
            }

            var ticket = _ticketService.GetTicketWithEmployees(id);

            if (ticket is null)
            {
                return NotFound();
            }

            return View(ticket);
        }
    }
}