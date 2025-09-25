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
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    start = e.StartTime,
                    end = e.EndTime,
                    allDay = e.IsAllDay,
                    description = e.Description,
                    location = e.Location,
                    guestCapacity = e.GuestCapacity,
                    contactPhone = e.ContactPhone,
                    status = e.Status
                })
                .ToListAsync();

            return Json(events);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eventModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
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
            return View(eventModel);
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