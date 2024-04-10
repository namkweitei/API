﻿using API.Data;
using API.Models;
using API.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        public EventsController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
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
        public async Task<ActionResult<EventDTO >> PostEvent([FromForm] EventDTO eventDTO)
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
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(int id, [FromForm] EventDTO eventDTO)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            @event.FacultyID = eventDTO.FacultyID;
            @event.EventName = eventDTO.EventName;
            @event.FinalClosureDate = eventDTO.FinalClosureDate;
            @event.DurationBetweenClosure = eventDTO.DurationBetweenClosure;
            @event.FirstClosureDate = eventDTO.FirstClosureDate;

            try
            {
                _context.Entry(@event).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!EventExists(id))
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
