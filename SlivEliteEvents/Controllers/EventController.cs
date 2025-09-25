using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlivEliteEvents.Data;
using SlivEliteEvents.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SlivEliteEvents.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public IActionResult Index()
        {
            return View();
        }

        // GET: Events/GetEvents (for FullCalendar)
        [HttpGet]
        public async Task<IActionResult> GetEvents(string status)
        {
            var query = _context.Events.AsQueryable();
            if (!string.IsNullOrEmpty(status))
                query = query.Where(e => e.Status == status);

            var events = await query.Select(e => new
            {
                e.Id,
                e.Title,
                Start = e.StartTime, // Keep as DateTime
                End = e.EndTime,     // Keep as DateTime or null
                e.IsAllDay,
                Description = e.Description,
                e.Location,
                e.GuestCapacity,
                e.ContactPhone,
                e.Status
            }).ToListAsync();

            // Format dates in memory for FullCalendar
            var formattedEvents = events.Select(e => new
            {
                id = e.Id,
                title = e.Title,
                start = e.Start.ToString("o"), // ISO 8601 format
                end = e.IsAllDay ? null : e.End?.ToString("o"), // Handle all-day events
                allDay = e.IsAllDay,
                extendedProps = new
                {
                    description = e.Description,
                    location = e.Location,
                    guestCapacity = e.GuestCapacity,
                    contactPhone = e.ContactPhone,
                    status = e.Status
                }
            });

            return Json(formattedEvents);
        }

        // GET: Events/Create
        public IActionResult Create(string start)
        {
            var model = new Event();
            if (DateTime.TryParse(start, out var startDate))
            {
                model.StartTime = startDate;
                ViewBag.StartDate = startDate;
            }
            return View(model);
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventModel)
        {
            if (eventModel.IsAllDay)
                eventModel.EndTime = null;

            if (ModelState.IsValid)
            {
                _context.Add(eventModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Log validation errors for debugging
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            Console.WriteLine("Validation errors: " + string.Join(", ", errors));

            return View(eventModel);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel == null) return NotFound();
            return View(eventModel);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event eventModel)
        {
            if (id != eventModel.Id) return NotFound();

            if (eventModel.IsAllDay)
                eventModel.EndTime = null;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventModel.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Log validation errors for debugging
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            Console.WriteLine("Validation errors: " + string.Join(", ", errors));

            return View(eventModel);
        }

        // GET: Events/List
        public async Task<IActionResult> List(string sortOrder)
        {
            ViewData["DateSortParm"] = sortOrder == "date" ? "date_desc" : "date";
            var events = from e in _context.Events select e;

            switch (sortOrder)
            {
                case "date":
                    events = events.OrderBy(e => e.StartTime);
                    break;
                case "date_desc":
                    events = events.OrderByDescending(e => e.StartTime);
                    break;
                default:
                    events = events.OrderBy(e => e.StartTime);
                    break;
            }

            return View(await events.ToListAsync());
        }

        // POST: Events/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel == null) return NotFound();

            _context.Events.Remove(eventModel);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}