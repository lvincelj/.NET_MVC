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
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public DepartmentsController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Query parameters 'page' and 'pageSize' must be greater than 0.");
        }

        pageSize = Math.Min(pageSize, 100);

        var query = _context.Departments
            .Include(d => d.Doctors)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(d =>
                EF.Functions.Like(d.Name, pattern) ||
                EF.Functions.Like(d.Location, pattern) ||
                EF.Functions.Like(d.PhoneNumber!, pattern) ||
                EF.Functions.Like(d.HeadOfDepartment!, pattern) ||
                d.Doctors.Any(doc =>
                    EF.Functions.Like(doc.FirstName, pattern) ||
                    EF.Functions.Like(doc.LastName, pattern)));
        }

        var departments = await query
            .OrderBy(d => d.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<DepartmentDto>>(departments));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<DepartmentDto>> GetById(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Doctors)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<DepartmentDto>(department));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public async Task<ActionResult<DepartmentDto>> Create([FromBody] CreateDepartmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var department = _mapper.Map<Department>(dto);

        if (dto.DoctorIds.Count > 0)
        {
            var doctorIds = dto.DoctorIds.Distinct().ToList();
            var doctors = await _context.Doctors.Where(d => doctorIds.Contains(d.Id)).ToListAsync();
            department.Doctors = doctors;
        }

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        var created = await _context.Departments
            .Include(d => d.Doctors)
            .AsNoTracking()
            .FirstAsync(d => d.Id == department.Id);

        return CreatedAtAction(nameof(GetById), new { id = department.Id }, _mapper.Map<DepartmentDto>(created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var department = await _context.Departments
            .Include(d => d.Doctors)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department is null)
        {
            return NotFound();
        }

        _mapper.Map(dto, department);

        var doctorIds = dto.DoctorIds.Distinct().ToList();
        var doctors = await _context.Doctors.Where(d => doctorIds.Contains(d.Id)).ToListAsync();
        department.Doctors.Clear();
        foreach (var doctor in doctors)
        {
            department.Doctors.Add(doctor);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Doctors)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department is null)
        {
            return NotFound();
        }

        // Remove many-to-many links explicitly before delete.
        department.Doctors.Clear();
        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
