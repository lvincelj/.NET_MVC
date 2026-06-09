using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementApp.Models;

public class PacijentDatoteka
{
    [Key]
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int PacijentId { get; set; }

    [Required]
    [StringLength(260)]
    public string OriginalnoIme { get; set; } = string.Empty;

    [Required]
    [StringLength(260)]
    public string NazivNaDisku { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Putanja { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string ContentType { get; set; } = string.Empty;

    public long Velicina { get; set; }

    public DateTime DatumUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(PacijentId))]
    public virtual Patient Pacijent { get; set; } = null!;
}
