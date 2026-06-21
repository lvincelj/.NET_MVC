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
public class MedicalRecordsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public MedicalRecordsController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MedicalRecordDto>>> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Query parameters 'page' and 'pageSize' must be greater than 0.");
        }

        pageSize = Math.Min(pageSize, 100);

        var query = _context.MedicalRecords
            .Include(r => r.Patient)
            .Include(r => r.Prescriptions)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(r =>
                EF.Functions.Like(r.Diagnosis, pattern) ||
                EF.Functions.Like(r.Notes!, pattern) ||
                EF.Functions.Like(r.Patient.FirstName, pattern) ||
                EF.Functions.Like(r.Patient.LastName, pattern) ||
                r.Prescriptions.Any(p => EF.Functions.Like(p.IssuedBy, pattern)));
        }

        var records = await query
            .OrderBy(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<MedicalRecordDto>>(records));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<MedicalRecordDto>> GetById(int id)
    {
        var record = await _context.MedicalRecords
            .Include(r => r.Patient)
            .Include(r => r.Prescriptions)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (record is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<MedicalRecordDto>(record));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public async Task<ActionResult<MedicalRecordDto>> Create([FromBody] CreateMedicalRecordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await _context.Patients.AnyAsync(p => p.Id == dto.PatientId))
        {
            ModelState.AddModelError(nameof(CreateMedicalRecordDto.PatientId), "Patient does not exist.");
            return BadRequest(ModelState);
        }

        var record = _mapper.Map<MedicalRecord>(dto);
        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();

        var created = await _context.MedicalRecords
            .Include(r => r.Patient)
            .Include(r => r.Prescriptions)
            .AsNoTracking()
            .FirstAsync(r => r.Id == record.Id);

        return CreatedAtAction(nameof(GetById), new { id = record.Id }, _mapper.Map<MedicalRecordDto>(created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicalRecordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var record = await _context.MedicalRecords.FirstOrDefaultAsync(r => r.Id == id);
        if (record is null)
        {
            return NotFound();
        }

        if (!await _context.Patients.AnyAsync(p => p.Id == dto.PatientId))
        {
            ModelState.AddModelError(nameof(UpdateMedicalRecordDto.PatientId), "Patient does not exist.");
            return BadRequest(ModelState);
        }

        _mapper.Map(dto, record);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var record = await _context.MedicalRecords.FirstOrDefaultAsync(r => r.Id == id);
        if (record is null)
        {
            return NotFound();
        }

        var hasPrescriptions = await _context.Prescriptions.AnyAsync(p => p.MedicalRecordId == id);
        if (hasPrescriptions)
        {
            return Conflict("Medical record cannot be deleted while prescriptions exist.");
        }

        _context.MedicalRecords.Remove(record);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
