using AutoMapper;
using HospitalManagementApp.Data;
using HospitalManagementApp.DTOs;
using HospitalManagementApp.Models;
using HospitalManagementApp.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementApp.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public AppointmentsController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Query parameters 'page' and 'pageSize' must be greater than 0.");
        }

        pageSize = Math.Min(pageSize, 100);

        var query = _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(a =>
                EF.Functions.Like(a.Room, pattern) ||
                EF.Functions.Like(a.Notes!, pattern) ||
                EF.Functions.Like(a.Patient.FirstName, pattern) ||
                EF.Functions.Like(a.Patient.LastName, pattern) ||
                EF.Functions.Like(a.Doctor.FirstName, pattern) ||
                EF.Functions.Like(a.Doctor.LastName, pattern) ||
                EF.Functions.Like(a.Doctor.Specialty, pattern));
        }

        var appointments = await query
            .OrderBy(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<AppointmentDto>>(appointments));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<AppointmentDto>> GetById(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<AppointmentDto>(appointment));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await ValidateReferences(dto.PatientId, dto.DoctorId);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var appointment = _mapper.Map<Appointment>(dto);
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        var created = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .AsNoTracking()
            .FirstAsync(a => a.Id == appointment.Id);

        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, _mapper.Map<AppointmentDto>(created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (appointment is null)
        {
            return NotFound();
        }

        await ValidateReferences(dto.PatientId, dto.DoctorId);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _mapper.Map(dto, appointment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (appointment is null)
        {
            return NotFound();
        }

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task ValidateReferences(int patientId, int doctorId)
    {
        if (!await _context.Patients.AnyAsync(p => p.Id == patientId))
        {
            ModelState.AddModelError(nameof(CreateAppointmentDto.PatientId), "Patient does not exist.");
        }

        if (!await _context.Doctors.AnyAsync(d => d.Id == doctorId))
        {
            ModelState.AddModelError(nameof(CreateAppointmentDto.DoctorId), "Doctor does not exist.");
        }
    }
}
