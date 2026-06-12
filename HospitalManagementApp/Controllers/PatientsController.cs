using HospitalManagementApp.Data;
using HospitalManagementApp.Models;
using HospitalManagementApp.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementApp.Controllers;

public class PatientsController : Controller
{
    private readonly PatientRepository _repo;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public PatientsController(
        PatientRepository repo,
        AppDbContext context,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _repo = repo;
        _context = context;
        _environment = environment;
        _configuration = configuration;
    }

    [HttpGet("/patients")]
    [AllowAnonymous]
    public IActionResult Index(string? term) => View(_repo.GetAll(term));

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Search(string? term) => PartialView("_PatientList", _repo.GetAll(term));

    [HttpGet("/patients/options")]
    [AllowAnonymous]
    public IActionResult Options(string? term)
    {
        var options = _repo.GetAll(term)
            .Take(20)
            .Select(p => new
            {
                value = p.Id,
                label = p.LastName + ", " + p.FirstName
            });

        return Json(options);
    }

    [HttpGet("/patients/{id:int}")]
    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        var patient = _repo.GetById(id);
        if (patient == null) return NotFound();
        return View(patient);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Create() => View(new Patient());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Create(Patient patient)
    {
        if (!ModelState.IsValid)
        {
            return View(patient);
        }

        _repo.Add(patient);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Edit(int id)
    {
        var patient = _repo.GetByIdForEdit(id);
        if (patient == null) return NotFound();
        return View(patient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Edit(int id, Patient patient)
    {
        if (id != patient.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(patient);
        }

        if (!_repo.Update(patient)) return NotFound();
        return RedirectToAction(nameof(Details), new { id = patient.Id });
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult Delete(int id)
    {
        var patient = _repo.GetById(id);
        if (patient == null) return NotFound();
        ViewBag.CanDelete = _repo.CanDelete(id);
        return View(patient);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!_repo.Delete(id))
        {
            TempData["Error"] = "Patient cannot be deleted while appointments, medical records, or files exist.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/patients/{id:int}/files/upload")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public async Task<IActionResult> UploadFile(int id, IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "Datoteka nije poslana." });
        }

        var patientExists = await _context.Patients.AnyAsync(p => p.Id == id);
        if (!patientExists)
        {
            return NotFound(new { error = "Pacijent ne postoji." });
        }

        var maxSizeMb = _configuration.GetValue<int?>("FileUpload:MaxSizeMb") ?? 20;
        var maxSizeBytes = maxSizeMb * 1024L * 1024L;
        if (file.Length > maxSizeBytes)
        {
            return BadRequest(new { error = $"Maksimalna veličina datoteke je {maxSizeMb} MB." });
        }

        var allowedTypes = _configuration.GetSection("FileUpload:AllowedContentTypes").Get<string[]>() ??
            ["application/pdf", "image/jpeg", "image/png", "application/dicom", "application/octet-stream"];

        if (!allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Nepodržan tip datoteke." });
        }

        var uploadRootFolder = _configuration["FileUpload:PatientFilesRootFolder"] ?? "uploads/pacijenti";
        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var patientFolder = Path.Combine(webRoot, uploadRootFolder, id.ToString());
        Directory.CreateDirectory(patientFolder);

        var extension = Path.GetExtension(file.FileName);
        var diskFileName = $"{Guid.NewGuid():N}{extension}";
        var diskPath = Path.Combine(patientFolder, diskFileName);

        await using (var stream = System.IO.File.Create(diskPath))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = "/" + Path.GetRelativePath(webRoot, diskPath).Replace("\\", "/");

        var metadata = new PacijentDatoteka
        {
            PacijentId = id,
            OriginalnoIme = Path.GetFileName(file.FileName),
            NazivNaDisku = diskFileName,
            Putanja = relativePath,
            ContentType = file.ContentType,
            Velicina = file.Length,
            DatumUtc = DateTime.UtcNow
        };

        _context.PacijentDatoteke.Add(metadata);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = metadata.Id,
            originalnoIme = metadata.OriginalnoIme,
            putanja = metadata.Putanja,
            contentType = metadata.ContentType,
            velicina = metadata.Velicina,
            datumUtc = metadata.DatumUtc.ToString("u")
        });
    }

    [HttpGet("/patients/{id:int}/files")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFiles(int id)
    {
        var patientExists = await _context.Patients.AnyAsync(p => p.Id == id);
        if (!patientExists)
        {
            return NotFound(new { error = "Pacijent ne postoji." });
        }

        var files = await _context.PacijentDatoteke
            .AsNoTracking()
            .Where(f => f.PacijentId == id)
            .OrderByDescending(f => f.DatumUtc)
            .Select(f => new
            {
                id = f.Id,
                originalnoIme = f.OriginalnoIme,
                putanja = f.Putanja,
                contentType = f.ContentType,
                velicina = f.Velicina,
                datumUtc = f.DatumUtc
            })
            .ToListAsync();

        return Ok(files);
    }

    [HttpPost("/patients/{patientId:int}/files/{fileId:int}/delete")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public async Task<IActionResult> DeleteFile(int patientId, int fileId)
    {
        var file = await _context.PacijentDatoteke
            .FirstOrDefaultAsync(f => f.Id == fileId && f.PacijentId == patientId);

        if (file is null)
        {
            return NotFound(new { error = "Datoteka nije pronađena." });
        }

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var relativePath = file.Putanja.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var diskPath = Path.Combine(webRoot, relativePath);

        if (System.IO.File.Exists(diskPath))
        {
            System.IO.File.Delete(diskPath);
        }

        _context.PacijentDatoteke.Remove(file);
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }
}
