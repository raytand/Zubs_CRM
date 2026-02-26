namespace Zubs.Application.DTOs;

public class TreatmentRecordDto
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid ServiceId { get; set; }
    public string? Notes { get; set; }
    public DateOnly PerformedAt { get; set; }
}
public class TreatmentRecordCreateDto
{
    public Guid AppointmentId { get; set; }
    public Guid ServiceId { get; set; }
    public string? Notes { get; set; }
    public DateOnly PerformedAt { get; set; }
}

