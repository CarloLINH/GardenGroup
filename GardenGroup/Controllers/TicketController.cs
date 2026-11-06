using GardenGroup.Enums;
using GardenGroup.Models;
using GardenGroup.Repositories;
using GardenGroup.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;

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

        public async Task<ActionResult> Index(Statuses? status, int? assignedTo, string sort = "newest", int page = 0)
        {
            page = Math.Max(page, 0);

            var tickets = await _ticketService.GetTicketsAsync(
                status, assignedTo, sort, page, PageSize);

            ViewBag.HasNext = sort is "priorityDesc" or "priorityAsc"
                && tickets.Count == PageSize;

            ViewBag.Status = status;
            ViewBag.AssignedTo = assignedTo;
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