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
public class PatientsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public PatientsController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Query parameters 'page' and 'pageSize' must be greater than 0.");
        }

        pageSize = Math.Min(pageSize, 100);

        var query = _context.Patients
            .Include(p => p.Appointments)
            .Include(p => p.MedicalRecords)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.FirstName, pattern) ||
                EF.Functions.Like(p.LastName, pattern) ||
                EF.Functions.Like(p.Email!, pattern) ||
                EF.Functions.Like(p.PhoneNumber!, pattern) ||
                EF.Functions.Like(p.Address!, pattern));
        }

        var patients = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<PatientDto>>(patients));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<PatientDto>> GetById(int id)
    {
        var patient = await _context.Patients
            .Include(p => p.Appointments)
            .Include(p => p.MedicalRecords)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<PatientDto>(patient));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public async Task<ActionResult<PatientDto>> Create([FromBody] CreatePatientDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var patient = _mapper.Map<Patient>(dto);
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        var created = await _context.Patients
            .Include(p => p.Appointments)
            .Include(p => p.MedicalRecords)
            .AsNoTracking()
            .FirstAsync(p => p.Id == patient.Id);

        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, _mapper.Map<PatientDto>(created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
        if (patient is null)
        {
            return NotFound();
        }

        _mapper.Map(dto, patient);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await _context.Patients
            .Include(p => p.Appointments)
            .Include(p => p.MedicalRecords)
                .ThenInclude(r => r.Prescriptions)
                    .ThenInclude(pr => pr.Medications)
            .Include(p => p.PacijentDatoteke)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (patient is null)
        {
            return NotFound();
        }

        var prescriptions = patient.MedicalRecords.SelectMany(r => r.Prescriptions).ToList();
        _context.Medications.RemoveRange(prescriptions.SelectMany(p => p.Medications));
        _context.Prescriptions.RemoveRange(prescriptions);
        _context.MedicalRecords.RemoveRange(patient.MedicalRecords);
        _context.Appointments.RemoveRange(patient.Appointments);
        _context.PacijentDatoteke.RemoveRange(patient.PacijentDatoteke);
        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
