// Author: Mart (Index), Emre (all other actions)
using GardenGroup.Authorization;
using GardenGroup.Enums;
using GardenGroup.Extensions;
using GardenGroup.Models;
using GardenGroup.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Bson;

namespace GardenGroup.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly IEmployeeService _employeeService;
        private readonly ITicketEscalator _escalator;
        private readonly ITicketTransferService _ticketTransferService;
        private const int PageSize = 100;

        public TicketController(ITicketService ticketService, IEmployeeService employeeService, ITicketEscalator escalator, ITicketTransferService ticketTransferService)
        {
            _ticketService = ticketService;
            _employeeService = employeeService;
            _escalator = escalator;
            _ticketTransferService = ticketTransferService;
        }

        // ---------------------------------------------------------------- list (Mart)

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
            var me = User.GetEmployeeId();
            if (me is null)
            {
                return Forbid();
            }

            LoadReporters();
            return View(new Ticket { Title = "", ReportById = me.Value });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Title,Description,Type,Priority,Deadline,ReportById")] Ticket ticket)
        {
            var me = User.GetEmployeeId();
            if (me is null)
            {
                return Forbid();
            }

            ModelState.Remove(nameof(Ticket.ReportById));

            if (User.IsServiceDesk())
            {
                if (!_employeeService.GetActiveEmployees().Any(e => e.EmployeeId == ticket.ReportById))
                {
                    ModelState.AddModelError(nameof(Ticket.ReportById), "Choose an employee from the list.");
                }
            }
            else
            {
                ticket.ReportById = me.Value;
            }

            if (ticket.Deadline is null)
            {
                ModelState.AddModelError(nameof(Ticket.Deadline), "Choose a deadline.");
            }
            else if (ticket.Deadline.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError(nameof(Ticket.Deadline), "The deadline cannot be in the past.");
            }

            if (!ModelState.IsValid)
            {
                LoadReporters();
                return View(ticket);
            }

            ticket.Deadline = ticket.Deadline!.Value.Date;
            _ticketService.CreateTicket(ticket);

            TempData["Success"] = $"Ticket #{ticket.TicketId} was created.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "ServiceDesk")]
        public ActionResult Edit(string id)
        {
            var ticket = GetExistingTicket(id);
            if (ticket == null)
            {
                return NotFound();
            }

            LoadAssignees();
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ServiceDesk")]
        public ActionResult Edit(string id, [Bind("Title,Description,Type,Priority,Deadline,Status,AssignedToId")] Ticket ticket)
        {
            var existing = GetExistingTicket(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (ticket.AssignedToId is int assignee && !GetAssignees().Any(e => e.EmployeeId == assignee))
            {
                ModelState.AddModelError(nameof(Ticket.AssignedToId), "Choose a Service Desk employee from the list.");
            }

            ticket.Id = id;
            ticket.TicketId = existing.TicketId;

            if (!ModelState.IsValid)
            {
                LoadAssignees();
                return View(ticket);
            }

            ticket.Deadline = ticket.Deadline?.Date;
            _ticketService.UpdateTicket(ticket);

            TempData["Success"] = $"Ticket #{existing.TicketId} was saved.";
            return RedirectToAction(nameof(Details), new { id });
        }


        [Authorize(Policy = "ServiceDesk")]
        public ActionResult Delete(string id)
        {
            var ticket = GetExistingTicket(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ServiceDesk")]
        public ActionResult DeleteConfirmed(string id)
        {
            var ticket = GetExistingTicket(id);
            if (ticket == null)
            {
                return NotFound();
            }

            _ticketService.DeleteTicket(id);
            TempData["Success"] = $"Ticket #{ticket.TicketId} was deleted.";
            return RedirectToAction(nameof(Index));
        }


        public ActionResult Details(string id)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return BadRequest("The ticket identifier is invalid.");
            }

            var ticket = _ticketService.GetTicketWithEmployees(id);

            if (ticket is null)
            {
                return NotFound();
            }

            if (!User.IsServiceDesk() && ticket.ReportById != User.GetEmployeeId())
            {
                return Forbid();
            }

            return View(ticket);
        }

        [HttpGet]
        [Authorize(Policy = "ServiceDesk")]
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
        [Authorize(Policy = "ServiceDesk")]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ServiceDesk")]
        public ActionResult Escalate(string id)
        {
            return ShowResult(_escalator.Escalate(id), id,
                "The ticket was escalated to management.",
                "This ticket cannot be escalated (it is already resolved, closed or escalated).");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ServiceDesk")]
        public ActionResult Close(string id)
        {
            return ShowResult(_escalator.Close(id), id,
                "The ticket was closed.",
                "This ticket is already closed.");
        }

        private ActionResult ShowResult(EscalationResult result, string id, string success, string notAllowed)
        {
            if (result == EscalationResult.NotFound)
            {
                return NotFound();
            }

            if (result == EscalationResult.Done)
            {
                TempData["Success"] = success;
            }
            else
            {
                TempData["Error"] = notAllowed;
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        private Ticket? GetExistingTicket(string id)
        {
            return ObjectId.TryParse(id, out _) ? _ticketService.GetTicketById(id) : null;
        }

        private List<Employee> GetAssignees()
        {
            return _employeeService.GetActiveEmployees().Where(e => AppRoles.IsServiceDesk(e.Role)).ToList();
        }

        private void LoadAssignees()
        {
            ViewBag.Assignees = GetAssignees()
                .Select(e => new SelectListItem($"{e.FirstName} {e.LastName}", e.EmployeeId.ToString()))
                .ToList();
        }

        private void LoadReporters()
        {
            if (!User.IsServiceDesk())
            {
                return;
            }

            ViewBag.Reporters = _employeeService.GetActiveEmployees()
                .Select(e => new SelectListItem($"{e.FirstName} {e.LastName} (#{e.EmployeeId})", e.EmployeeId.ToString()))
                .ToList();
        }
    }
}