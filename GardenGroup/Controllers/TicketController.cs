using GardenGroup.Enums;
using GardenGroup.Models;
using GardenGroup.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace GardenGroup.Controllers
{
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ITicketTransferService _ticketTransferService;
        private readonly IEmployeeService _employeeService;

        public TicketController(ITicketService ticketService, ITicketTransferService ticketTransferService, IEmployeeService employeeService)
        {
            _ticketService = ticketService;
            _ticketTransferService = ticketTransferService;
            _employeeService = employeeService;
        }

        public ActionResult Index(Statuses? status, int? assignedTo, string sort = "newest")
        {
            var tickets = _ticketService.GetTickets(status, assignedTo, sort != "oldest");
            ViewBag.Status = status;
            ViewBag.AssignedTo = assignedTo;
            ViewBag.Sort = sort;
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

        [HttpGet]
        public async Task<IActionResult> Transfer(string id)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return BadRequest("The ticket identifier is invalid.");
            }

            var ticket = _ticketService.GetTicketById(id);

            if (ticket is null)
            {
                return NotFound("The ticket could not be found.");
            }

            var employees = await _employeeService.GetAllAsync();

            ViewBag.Employees = employees
                .Where(employee => employee.Active)
                .OrderBy(employee => employee.LastName)
                .ToList();

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(
            string id,
            int employeeId)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return BadRequest("The ticket identifier is invalid.");
            }

            try
            {
                var result = await _ticketTransferService.TransferAsync(
                    id,
                    employeeId);

                if (result.MatchedCount == 0)
                {
                    TempData["TransferMessage"] =
                        "The ticket was already assigned to this employee.";
                }
                else if (result.ModifiedCount == 1)
                {
                    TempData["TransferMessage"] =
                        "The ticket was successfully transferred.";
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("The ticket could not be found.");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("employeeId", ex.Message);

                var employees = await _employeeService.GetAllAsync();

                ViewBag.Employees = employees
                    .Where(employee => employee.Active)
                    .OrderBy(employee => employee.LastName)
                    .ToList();

                var ticket = _ticketService.GetTicketById(id);

                return View(ticket);
            }
        }
    }
}