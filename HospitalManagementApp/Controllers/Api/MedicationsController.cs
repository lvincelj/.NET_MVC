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
public class MedicationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public MedicationsController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MedicationDto>>> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Query parameters 'page' and 'pageSize' must be greater than 0.");
        }

        pageSize = Math.Min(pageSize, 100);

        var query = _context.Medications
            .Include(m => m.Prescription)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(m =>
                EF.Functions.Like(m.Name, pattern) ||
                EF.Functions.Like(m.Dosage, pattern) ||
                EF.Functions.Like(m.Instructions!, pattern) ||
                EF.Functions.Like(m.Prescription.IssuedBy, pattern));
        }

        var medications = await query
            .OrderBy(m => m.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<MedicationDto>>(medications));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<MedicationDto>> GetById(int id)
    {
        var medication = await _context.Medications
            .Include(m => m.Prescription)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medication is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<MedicationDto>(medication));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public async Task<ActionResult<MedicationDto>> Create([FromBody] CreateMedicationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await _context.Prescriptions.AnyAsync(p => p.Id == dto.PrescriptionId))
        {
            ModelState.AddModelError(nameof(CreateMedicationDto.PrescriptionId), "Prescription does not exist.");
            return BadRequest(ModelState);
        }

        var medication = _mapper.Map<Medication>(dto);
        _context.Medications.Add(medication);
        await _context.SaveChangesAsync();

        var created = await _context.Medications
            .Include(m => m.Prescription)
            .AsNoTracking()
            .FirstAsync(m => m.Id == medication.Id);

        return CreatedAtAction(nameof(GetById), new { id = medication.Id }, _mapper.Map<MedicationDto>(created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var medication = await _context.Medications.FirstOrDefaultAsync(m => m.Id == id);
        if (medication is null)
        {
            return NotFound();
        }

        if (!await _context.Prescriptions.AnyAsync(p => p.Id == dto.PrescriptionId))
        {
            ModelState.AddModelError(nameof(UpdateMedicationDto.PrescriptionId), "Prescription does not exist.");
            return BadRequest(ModelState);
        }

        _mapper.Map(dto, medication);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var medication = await _context.Medications.FirstOrDefaultAsync(m => m.Id == id);
        if (medication is null)
        {
            return NotFound();
        }

        _context.Medications.Remove(medication);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
