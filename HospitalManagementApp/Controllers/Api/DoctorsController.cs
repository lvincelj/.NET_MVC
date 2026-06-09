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
public class DoctorsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public DoctorsController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<DoctorDto>>> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Query parameters 'page' and 'pageSize' must be greater than 0.");
        }

        pageSize = Math.Min(pageSize, 100);

        var query = _context.Doctors
            .Include(d => d.Departments)
            .Include(d => d.Appointments)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(d =>
                EF.Functions.Like(d.FirstName, pattern) ||
                EF.Functions.Like(d.LastName, pattern) ||
                EF.Functions.Like(d.Specialty, pattern) ||
                EF.Functions.Like(d.Email!, pattern) ||
                EF.Functions.Like(d.PhoneNumber!, pattern) ||
                d.Departments.Any(dep => EF.Functions.Like(dep.Name, pattern)));
        }

        var doctors = await query
            .OrderBy(d => d.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<DoctorDto>>(doctors));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<DoctorDto>> GetById(int id)
    {
        var doctor = await _context.Doctors
            .Include(d => d.Departments)
            .Include(d => d.Appointments)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<DoctorDto>(doctor));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public async Task<ActionResult<DoctorDto>> Create([FromBody] CreateDoctorDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var doctor = _mapper.Map<Doctor>(dto);

        if (dto.DepartmentIds.Count > 0)
        {
            var departmentIds = dto.DepartmentIds.Distinct().ToList();
            var departments = await _context.Departments.Where(d => departmentIds.Contains(d.Id)).ToListAsync();
            doctor.Departments = departments;
        }

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        var created = await _context.Doctors
            .Include(d => d.Departments)
            .Include(d => d.Appointments)
            .AsNoTracking()
            .FirstAsync(d => d.Id == doctor.Id);

        return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, _mapper.Map<DoctorDto>(created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDoctorDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var doctor = await _context.Doctors
            .Include(d => d.Departments)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor is null)
        {
            return NotFound();
        }

        _mapper.Map(dto, doctor);

        var departmentIds = dto.DepartmentIds.Distinct().ToList();
        var departments = await _context.Departments.Where(d => departmentIds.Contains(d.Id)).ToListAsync();
        doctor.Departments.Clear();
        foreach (var department in departments)
        {
            doctor.Departments.Add(department);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var doctor = await _context.Doctors
            .Include(d => d.Departments)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor is null)
        {
            return NotFound();
        }

        // Remove many-to-many links explicitly before delete.
        doctor.Departments.Clear();
        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
