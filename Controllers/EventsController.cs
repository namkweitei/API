using API.Data;
using API.Models;
using API.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/events
        [HttpGet]
        public async Task<ActionResult<Event>> GetEvent()
        {
            var @event = await _context.Events.FindAsync(1); // Assume there is only one event in the system
            if (@event == null)
            {
                return NotFound();
            }

            return @event;
        }

        // POST: api/events
        [HttpPost]
        public async Task<ActionResult<EventDTO >> PostEvent(EventDTO eventDTO)
        {
            var @event = new Event
            {
                EventName = eventDTO.EventName,
                FirstClosureDate = eventDTO.FirstClosureDate,
                FinalClosureDate = eventDTO.FinalClosureDate,
                DurationBetweenClosure = eventDTO.DurationBetweenClosure,
                FacultyID = eventDTO.FacultyID
            };
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            return eventDTO;
        }

        // PUT: api/events
        [HttpPut]
        public async Task<IActionResult> PutEvent( Event @event)
        {
            if (@event.EventID != 1) // Assume there is only one event in the system
            {
                return BadRequest();
            }

            _context.Entry(@event).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!EventExists(@event.EventID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventID == id);
        }
    }
}
