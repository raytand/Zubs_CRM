namespace Zubs.Application.DTOs;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string Entity { get; set; } = null!;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = null!;
    public Guid ChangedBy { get; set; }
    public string? ChangedByUsername { get; set; }
    public DateTime ChangedAt { get; set; }
}
public class AuditLogCreateDto
{
    public string Entity { get; set; } = null!;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = null!;
    public Guid ChangedBy { get; set; }
}
