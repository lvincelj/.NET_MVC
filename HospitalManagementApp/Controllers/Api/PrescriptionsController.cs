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
public class PrescriptionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public PrescriptionsController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PrescriptionDto>>> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Query parameters 'page' and 'pageSize' must be greater than 0.");
        }

        pageSize = Math.Min(pageSize, 100);

        var query = _context.Prescriptions
            .Include(p => p.MedicalRecord)
            .Include(p => p.Medications)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.IssuedBy, pattern) ||
                EF.Functions.Like(p.MedicalRecord.Diagnosis, pattern) ||
                p.Medications.Any(m =>
                    EF.Functions.Like(m.Name, pattern) ||
                    EF.Functions.Like(m.Dosage, pattern)));
        }

        var prescriptions = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<PrescriptionDto>>(prescriptions));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<PrescriptionDto>> GetById(int id)
    {
        var prescription = await _context.Prescriptions
            .Include(p => p.MedicalRecord)
            .Include(p => p.Medications)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prescription is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<PrescriptionDto>(prescription));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public async Task<ActionResult<PrescriptionDto>> Create([FromBody] CreatePrescriptionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await _context.MedicalRecords.AnyAsync(r => r.Id == dto.MedicalRecordId))
        {
            ModelState.AddModelError(nameof(CreatePrescriptionDto.MedicalRecordId), "Medical record does not exist.");
            return BadRequest(ModelState);
        }

        var prescription = _mapper.Map<Prescription>(dto);
        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        var created = await _context.Prescriptions
            .Include(p => p.MedicalRecord)
            .Include(p => p.Medications)
            .AsNoTracking()
            .FirstAsync(p => p.Id == prescription.Id);

        return CreatedAtAction(nameof(GetById), new { id = prescription.Id }, _mapper.Map<PrescriptionDto>(created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePrescriptionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var prescription = await _context.Prescriptions.FirstOrDefaultAsync(p => p.Id == id);
        if (prescription is null)
        {
            return NotFound();
        }

        if (!await _context.MedicalRecords.AnyAsync(r => r.Id == dto.MedicalRecordId))
        {
            ModelState.AddModelError(nameof(UpdatePrescriptionDto.MedicalRecordId), "Medical record does not exist.");
            return BadRequest(ModelState);
        }

        _mapper.Map(dto, prescription);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var prescription = await _context.Prescriptions
            .Include(p => p.Medications)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (prescription is null)
        {
            return NotFound();
        }

        _context.Medications.RemoveRange(prescription.Medications);
        _context.Prescriptions.Remove(prescription);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
