using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlivEliteEvents.Data;
using SlivEliteEvents.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SlivEliteEvents.Controllers
{
    [Authorize(Roles = "Manager")]
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
                Start = e.StartTime,
                End = e.EndTime,
                e.IsAllDay,
                Description = e.Description,
                e.Location,
                e.GuestCapacity,
                e.ContactPhone,
                e.Status
            }).ToListAsync();

            // Format dates and add colors for FullCalendar
            var formattedEvents = events.Select(e => new
            {
                id = e.Id,
                title = e.Title,
                start = e.Start.ToString("o"), // ISO 8601 for FullCalendar
                end = e.IsAllDay ? null : e.End?.ToString("o"),
                allDay = e.IsAllDay,
                backgroundColor = e.Status switch
                {
                    "Pending" => "#007bff", // Blue
                    "Confirmed" => "#28a745", // Green
                    "Completed" => "#dc3545", // Red
                    _ => "#007bff" // Default to blue
                },
                borderColor = e.Status switch
                {
                    "Pending" => "#0056b3", // Darker blue
                    "Confirmed" => "#1e7e34", // Darker green
                    "Completed" => "#c82333", // Darker red
                    _ => "#0056b3"
                },
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
                ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
            }
            else
            {
                ViewBag.StartDate = DateTime.Today.ToString("yyyy-MM-dd");
            }
            return View(model);
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventModel, string StartTimeTime, string EndTimeTime)
        {
            if (eventModel.IsAllDay)
            {
                eventModel.EndTime = null;
                ModelState.Remove("StartTimeTime");
                ModelState.Remove("EndTimeTime");
            }
            else
            {
                // Combine date and time for StartTime
                if (!string.IsNullOrEmpty(StartTimeTime))
                {
                    if (DateTime.TryParse($"{eventModel.StartTime:yyyy-MM-dd} {StartTimeTime}", out var startDateTime))
                    {
                        eventModel.StartTime = startDateTime;
                    }
                    else
                    {
                        ModelState.AddModelError("StartTimeTime", "Невалиден формат за почетно време.");
                    }
                }
                else
                {
                    ModelState.AddModelError("StartTimeTime", "Почетното време е задолжително за нецелодневни настани.");
                }

                // Combine date and time for EndTime
                if (!string.IsNullOrEmpty(EndTimeTime) && eventModel.EndTime.HasValue)
                {
                    if (DateTime.TryParse($"{eventModel.EndTime.Value:yyyy-MM-dd} {EndTimeTime}", out var endDateTime))
                    {
                        eventModel.EndTime = endDateTime;
                    }
                    else
                    {
                        ModelState.AddModelError("EndTimeTime", "Невалиден формат за крајно време.");
                    }
                }
                else if (!eventModel.EndTime.HasValue && !eventModel.IsAllDay)
                {
                    ModelState.AddModelError("EndTime", "Крајниот датум е задолжителен за нецелодневни настани.");
                }

                // Validate EndTime is after StartTime
                if (!eventModel.IsAllDay && eventModel.EndTime.HasValue && eventModel.EndTime <= eventModel.StartTime)
                {
                    ModelState.AddModelError("EndTime", "Крајното време мора да биде после почетното време.");
                }
            }

            if (ModelState.IsValid)
            {
                eventModel.CreatedAt = DateTime.Now;
                _context.Add(eventModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            Console.WriteLine("Validation errors: " + string.Join(", ", errors));
            ViewBag.StartDate = eventModel.StartTime.ToString("yyyy-MM-dd");
            return View(eventModel);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel == null) return NotFound();
            ViewBag.StartDate = eventModel.StartTime.ToString("yyyy-MM-dd");
            return View(eventModel);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event eventModel, string StartTimeTime, string EndTimeTime)
        {
            if (id != eventModel.Id) return NotFound();

            if (eventModel.IsAllDay)
            {
                eventModel.EndTime = null;
                ModelState.Remove("StartTimeTime");
                ModelState.Remove("EndTimeTime");
            }
            else
            {
                // Combine date and time for StartTime
                if (!string.IsNullOrEmpty(StartTimeTime))
                {
                    if (DateTime.TryParse($"{eventModel.StartTime:yyyy-MM-dd} {StartTimeTime}", out var startDateTime))
                    {
                        eventModel.StartTime = startDateTime;
                    }
                    else
                    {
                        ModelState.AddModelError("StartTimeTime", "Невалиден формат за почетно време.");
                    }
                }
                else
                {
                    ModelState.AddModelError("StartTimeTime", "Почетното време е задолжително за нецелодневни настани.");
                }

                // Combine date and time for EndTime
                if (!string.IsNullOrEmpty(EndTimeTime) && eventModel.EndTime.HasValue)
                {
                    if (DateTime.TryParse($"{eventModel.EndTime.Value:yyyy-MM-dd} {EndTimeTime}", out var endDateTime))
                    {
                        eventModel.EndTime = endDateTime;
                    }
                    else
                    {
                        ModelState.AddModelError("EndTimeTime", "Невалиден формат за крајно време.");
                    }
                }
                else if (!eventModel.EndTime.HasValue && !eventModel.IsAllDay)
                {
                    ModelState.AddModelError("EndTime", "Крајниот датум е задолжителен за нецелодневни настани.");
                }

                // Validate EndTime is after StartTime
                if (!eventModel.IsAllDay && eventModel.EndTime.HasValue && eventModel.EndTime <= eventModel.StartTime)
                {
                    ModelState.AddModelError("EndTime", "Крајното време мора да биде после почетното време.");
                }
            }

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

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            Console.WriteLine("Validation errors: " + string.Join(", ", errors));
            ViewBag.StartDate = eventModel.StartTime.ToString("yyyy-MM-dd");
            return View(eventModel);
        }

        // GET: Events/List
        public IActionResult List(string sortOrder, string searchString, int page = 1, int pageSize = 10)
        {
            ViewData["DateSortParm"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentSearch"] = searchString;

            var events = _context.Events.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                events = events.Where(e => e.Title.Contains(searchString));
            }

            // Apply sorting
            events = sortOrder == "date" 
                ? events.OrderBy(e => e.StartTime) 
                : sortOrder == "date_desc" 
                    ? events.OrderByDescending(e => e.StartTime) 
                    : events.OrderBy(e => e.StartTime);

            // Pagination
            int totalEvents = events.Count();
            int totalPages = (int)Math.Ceiling(totalEvents / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages)); // Ensure valid page
            var paginatedEvents = events.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(paginatedEvents);
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