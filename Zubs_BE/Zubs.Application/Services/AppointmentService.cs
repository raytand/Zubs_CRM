using AutoMapper;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Application.Interfaces.Services;
using Zubs.Domain.Entities;

namespace Zubs.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _repo;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    private static string ByDoctorKey(Guid doctorId) => $"appointments:doctor:{doctorId}";
    private static string ByPatientKey(Guid patientId) => $"appointments:patient:{patientId}";
    private const string AllKey = "appointments:all";

    public AppointmentService(
        IAppointmentRepository repo,
        IAuditLogService auditLogService,
        IMapper mapper,
        ICacheService cache)
    {
        _repo = repo;
        _auditLogService = auditLogService;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAllAsync()
    {
        var cached = await _cache.GetAsync<IEnumerable<AppointmentDto>>(AllKey);
        if (cached is not null) return cached;

        var result = _mapper.Map<IEnumerable<AppointmentDto>>(await _repo.GetAllAsync());
        await _cache.SetAsync(AllKey, result, TimeSpan.FromMinutes(2));
        return result;
    }

    public async Task<IEnumerable<AppointmentDto>> GetByDoctorAsync(Guid doctorId)
    {
        var key = ByDoctorKey(doctorId);
        var cached = await _cache.GetAsync<IEnumerable<AppointmentDto>>(key);
        if (cached is not null) return cached;

        var result = _mapper.Map<IEnumerable<AppointmentDto>>(await _repo.GetByDoctorIdAsync(doctorId));
        await _cache.SetAsync(key, result, TimeSpan.FromMinutes(2));
        return result;
    }

    public async Task<IEnumerable<AppointmentDto>> GetByPatientAsync(Guid patientId)
    {
        var key = ByPatientKey(patientId);
        var cached = await _cache.GetAsync<IEnumerable<AppointmentDto>>(key);
        if (cached is not null) return cached;

        var result = _mapper.Map<IEnumerable<AppointmentDto>>(await _repo.GetByPatientIdAsync(patientId));
        await _cache.SetAsync(key, result, TimeSpan.FromMinutes(2));
        return result;
    }

    public async Task<AppointmentDto> CreateAsync(AppointmentCreateDto dto)
    {
        var entity = _mapper.Map<Appointment>(dto);
        entity.Id = Guid.NewGuid();
        await _repo.AddAsync(entity);

        await _auditLogService.LogAsync(
            entity: nameof(Appointment),
            entityId: entity.Id,
            action: "Create Appointment"
        );

        await InvalidateRelatedCaches(entity.DoctorId, entity.PatientId);

        return _mapper.Map<AppointmentDto>(entity);
    }

    public async Task UpdateAsync(AppointmentUpdateDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Appointment with ID {dto.Id} not found.");

        _mapper.Map(dto, entity);
        await _repo.UpdateAsync(entity);

        await _auditLogService.LogAsync(
            entity: nameof(Appointment),
            entityId: entity.Id,
            action: "Update Appointment"
        );

        await InvalidateRelatedCaches(entity.DoctorId, entity.PatientId);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return;

        await _repo.DeleteAsync(id);

        await _auditLogService.LogAsync(
            entity: nameof(Appointment),
            entityId: entity.Id,
            action: "Delete Appointment"
        );

        await InvalidateRelatedCaches(entity.DoctorId, entity.PatientId);
    }

    private async Task InvalidateRelatedCaches(Guid doctorId, Guid patientId)
    {
        await Task.WhenAll(
            _cache.RemoveAsync(AllKey),
            _cache.RemoveAsync(ByDoctorKey(doctorId)),
            _cache.RemoveAsync(ByPatientKey(patientId))
        );
    }
}