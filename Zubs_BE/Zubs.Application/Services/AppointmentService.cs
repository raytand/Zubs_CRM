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

    public AppointmentService(IAppointmentRepository repo, IAuditLogService auditLogService, IMapper mapper)
    {
        _repo = repo;
        _auditLogService = auditLogService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<AppointmentDto>>(await _repo.GetAllAsync());

    public async Task<IEnumerable<AppointmentDto>> GetByDoctorAsync(Guid doctorId)
        => _mapper.Map<IEnumerable<AppointmentDto>>(await _repo.GetByDoctorIdAsync(doctorId));

    public async Task<IEnumerable<AppointmentDto>> GetByPatientAsync(Guid patientId)
        => _mapper.Map<IEnumerable<AppointmentDto>>(await _repo.GetByPatientIdAsync(patientId));

    public async Task DeleteAsync (Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity != null)
        {
            await _repo.DeleteAsync(id);
            await _auditLogService.LogAsync(
                entity: nameof(Appointment),
                entityId: entity.Id,
                action: "Delete Appointment"
            );
        }
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

        return _mapper.Map<AppointmentDto>(entity);
    }

    public async Task UpdateAsync(AppointmentUpdateDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id);
        if (entity == null)
            throw new KeyNotFoundException($"Appointment with ID {dto.Id} not found.");

        _mapper.Map(dto, entity);

        await _repo.UpdateAsync(entity);
        await _auditLogService.LogAsync(
            entity: nameof(Appointment),
            entityId: entity.Id,
            action: "Update Appointment"
        );
    }

}
