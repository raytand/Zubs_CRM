namespace Zubs.Application.DTOs
{
    public class DentalChartDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string ToothNumber { get; set; } = null!;
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }

    public class DentalChartCreateDto
    {
        public Guid PatientId { get; set; }
        public string ToothNumber { get; set; } = null!;
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }

    public class DentalChartUpdateDto
    {
        public Guid Id { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
}
