namespace Patitas_Backend.Core.Entities;
public class PatientSearchParameters
{
    public string? AnimalName { get; set; }
    public string? OwnerName { get; set; }
    public string? OwnerNationalId { get; set; }
    public string? Species { get; set; }
    public string? Breed { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}