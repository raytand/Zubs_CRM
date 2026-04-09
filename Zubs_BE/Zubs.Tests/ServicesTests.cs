using System.Collections.Concurrent;
using AutoMapper;
using Zubs.Application.DTOs;
using Zubs.Application.Exceptions;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Application.Interfaces.Services;
using Zubs.Application.Services;
using Zubs.Domain.Entities;
using Zubs.Domain.Enums;

namespace Zubs.Tests;

public class ServicesTests
{
    // -------------------------------------------------------------------------
    // NullCacheService — always misses, used in all unit tests
    // -------------------------------------------------------------------------
    private class NullCacheService : ICacheService
    {
        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
            => Task.FromResult(default(T?));

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task RemoveAsync(string key, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    // -------------------------------------------------------------------------
    // ServiceService tests
    // -------------------------------------------------------------------------
    [Fact]
    public async Task ServiceService_Create_GetAll_GetById_Update_Delete_And_DuplicateCode()
    {
        var repo = new InMemoryServiceRepository();
        var mapper = BuildMapper(cfg =>
        {
            cfg.CreateMap<ServiceCreateDto, Service>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore())
                .ForMember(dest => dest.TreatmentRecords, opt => opt.Ignore());

            cfg.CreateMap<ServiceUpdateDto, Service>()
                .ForMember(dest => dest.Appointments, opt => opt.Ignore())
                .ForMember(dest => dest.TreatmentRecords, opt => opt.Ignore());

            cfg.CreateMap<Service, ServiceDto>();
        });

        var svc = new ServiceService(repo, mapper, new NullCacheService());

        var createDto = new ServiceCreateDto
        {
            Code = "SVC001",
            Name = "Test Service",
            Description = "desc",
            Price = 100m
        };

        var created = await svc.CreateAsync(createDto);
        Assert.NotNull(created);
        Assert.Equal(createDto.Code, created.Code);
        Assert.Equal(createDto.Name, created.Name);
        Assert.Equal(createDto.Price, created.Price);

        var all = (await svc.GetAllAsync()).ToList();
        Assert.Single(all);

        var byId = await svc.GetByIdAsync(created.Id);
        Assert.NotNull(byId);
        Assert.Equal(created.Id, byId!.Id);

        var updateDto = new ServiceUpdateDto
        {
            Id = created.Id,
            Code = "SVC001",
            Name = "Updated Service",
            Description = "updated",
            Price = 150m
        };
        await svc.UpdateAsync(updateDto);

        var updatedEntity = await repo.GetByIdAsync(updateDto.Id);
        Assert.NotNull(updatedEntity);
        Assert.Equal("Updated Service", updatedEntity!.Name);
        Assert.Equal(150m, updatedEntity.Price);

        // Duplicate code should throw
        var createDto2 = new ServiceCreateDto { Code = "SVC001", Name = "Another", Description = "", Price = 50m };
        await Assert.ThrowsAsync<AppException>(async () => await svc.CreateAsync(createDto2));

        await svc.DeleteAsync(created.Id);
        var afterDelete = await svc.GetByIdAsync(created.Id);
        Assert.Null(afterDelete);

        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await svc.DeleteAsync(created.Id));
    }

    // -------------------------------------------------------------------------
    // AppointmentService tests
    // -------------------------------------------------------------------------
    [Fact]
    public async Task AppointmentService_Create_GetByDoctor_GetByPatient_Update_Delete_And_NotFound()
    {
        var repo = new InMemoryAppointmentRepository();
        var audit = new InMemoryAuditLogService();
        var mapper = BuildMapper(cfg =>
        {
            cfg.AllowNullCollections = true;

            cfg.CreateMap<AppointmentCreateDto, Appointment>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.Doctor, opt => opt.Ignore())
                .ForMember(dest => dest.Service, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.TreatmentRecords, opt => opt.MapFrom(_ => (ICollection<TreatmentRecord>)null!))
                .ForMember(dest => dest.Payments, opt => opt.MapFrom(_ => (ICollection<Payment>)null!));

            cfg.CreateMap<AppointmentUpdateDto, Appointment>()
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.DoctorId, opt => opt.Ignore())
                .ForMember(dest => dest.Doctor, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceId, opt => opt.Ignore())
                .ForMember(dest => dest.Service, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TreatmentRecords, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore());

            cfg.CreateMap<Appointment, AppointmentDto>();
        });

        var svc = new AppointmentService(repo, audit, mapper, new NullCacheService());

        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        var createDto = new AppointmentCreateDto
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ServiceId = serviceId,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            Status = AppointmentStatus.Scheduled,
            Notes = "note"
        };

        var created = await svc.CreateAsync(createDto);
        Assert.NotNull(created);
        Assert.Equal(patientId, created.PatientId);
        Assert.Equal(doctorId, created.DoctorId);

        var byDoctor = (await svc.GetByDoctorAsync(doctorId)).ToList();
        Assert.Single(byDoctor);

        var byPatient = (await svc.GetByPatientAsync(patientId)).ToList();
        Assert.Single(byPatient);

        var updateDto = new AppointmentUpdateDto
        {
            Id = created.Id,
            StartTime = created.StartTime.AddDays(1),
            EndTime = created.EndTime.AddDays(1),
            Status = AppointmentStatus.Completed,
            Notes = "done"
        };
        await svc.UpdateAsync(updateDto);

        var updated = await repo.GetByIdAsync(updateDto.Id);
        Assert.NotNull(updated);
        Assert.Equal(AppointmentStatus.Completed, updated!.Status);
        Assert.Equal("done", updated.Notes);

        Assert.Contains(audit.Logs, l =>
            l.action == "Update Appointment" &&
            l.entity == nameof(Appointment) &&
            l.entityId == updated.Id);

        await svc.DeleteAsync(created.Id);
        var afterDelete = await repo.GetByIdAsync(created.Id);
        Assert.Null(afterDelete);

        Assert.Contains(audit.Logs, l =>
            l.action == "Delete Appointment" &&
            l.entity == nameof(Appointment) &&
            l.entityId == created.Id);

        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await svc.UpdateAsync(new AppointmentUpdateDto
            {
                Id = Guid.NewGuid(),
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                Status = AppointmentStatus.Scheduled
            }));
    }

    // -------------------------------------------------------------------------
    // DoctorService tests
    // -------------------------------------------------------------------------
    [Fact]
    public async Task DoctorService_Create_Update_Delete_And_UserNotFound()
    {
        var userRepo = new InMemoryUserRepository();
        var repo = new InMemoryDoctorRepository();
        var mapper = BuildMapper(cfg =>
        {
            cfg.CreateMap<DoctorCreateDto, Doctor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore());

            cfg.CreateMap<DoctorUpdateDto, Doctor>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore());

            cfg.CreateMap<Doctor, DoctorDto>();
        });

        var svc = new DoctorService(repo, userRepo, mapper, new NullCacheService());

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "u",
            Email = "u@x.com",
            PasswordHash = "h",
            Role = UserRole.Doctor,
            CreatedAt = DateTime.UtcNow
        };
        await userRepo.AddAsync(user);

        var createDto = new DoctorCreateDto { UserId = user.Id, FirstName = "A", LastName = "B", Email = "e@e.com" };
        var created = await svc.CreateAsync(createDto);
        Assert.NotNull(created);
        Assert.Equal("A", created.FirstName);

        var updateDto = new DoctorUpdateDto { Id = created.Id, FirstName = "AA", LastName = "BB" };
        await svc.UpdateAsync(updateDto);
        var updated = await repo.GetByIdAsync(updateDto.Id);
        Assert.Equal("AA", updated!.FirstName);

        await svc.DeleteAsync(created.Id);
        var after = await repo.GetByIdAsync(created.Id);
        Assert.Null(after);

        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await svc.CreateAsync(new DoctorCreateDto { UserId = Guid.NewGuid(), FirstName = "X", LastName = "Y" }));
    }

    // -------------------------------------------------------------------------
    // PaymentService tests
    // -------------------------------------------------------------------------
    [Fact]
    public async Task PaymentService_Create_Update_Delete_And_Errors()
    {
        var repo = new InMemoryPaymentRepository();
        var mapper = BuildMapper(cfg =>
        {
            cfg.CreateMap<PaymentCreateDto, Payment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.Appointment, opt => opt.Ignore());

            cfg.CreateMap<PaymentUpdateDto, Payment>()
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                .ForMember(dest => dest.AppointmentId, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.Appointment, opt => opt.Ignore());

            cfg.CreateMap<Payment, PaymentDto>();
        });

        var svc = new PaymentService(repo, mapper);

        var patientId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        var createDto = new PaymentCreateDto
        {
            PatientId = patientId,
            AppointmentId = appointmentId,
            Amount = 200m,
            PaidAt = DateOnly.FromDateTime(DateTime.UtcNow),
            Method = PaymentMethod.Cash
        };

        var created = await svc.CreateAsync(createDto);
        Assert.NotNull(created);
        Assert.Equal(200m, created.Amount);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await svc.UpdateAsync(new PaymentUpdateDto { Id = Guid.Empty }));

        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await svc.UpdateAsync(new PaymentUpdateDto
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                AppointmentId = appointmentId,
                Amount = 10m,
                PaidAt = DateOnly.FromDateTime(DateTime.UtcNow),
                Method = PaymentMethod.Cash
            }));

        await svc.DeleteAsync(created.Id);
        var after = await repo.GetByIdAsync(created.Id);
        Assert.Null(after);
    }

    // -------------------------------------------------------------------------
    // PatientService tests
    // -------------------------------------------------------------------------
    [Fact]
    public async Task PatientService_Create_Update_Delete_NotFound()
    {
        var repo = new InMemoryPatientRepository();
        var mapper = BuildMapper(cfg =>
        {
            cfg.CreateMap<PatientCreateDto, Patient>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore())
                .ForMember(dest => dest.MedicalRecords, opt => opt.Ignore())
                .ForMember(dest => dest.DentalCharts, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore());

            cfg.CreateMap<PatientUpdateDto, Patient>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore())
                .ForMember(dest => dest.MedicalRecords, opt => opt.Ignore())
                .ForMember(dest => dest.DentalCharts, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore());

            cfg.CreateMap<Patient, PatientDto>();
        });

        var svc = new PatientService(repo, mapper, new NullCacheService());

        var createDto = new PatientCreateDto { FirstName = "Fn", LastName = "Ln", Phone = "123", Email = "e@e.com" };
        var created = await svc.CreateAsync(createDto);
        Assert.NotNull(created);

        var all = (await svc.GetAllAsync()).ToList();
        Assert.Single(all);

        var byId = await svc.GetByIdAsync(created.Id);
        Assert.NotNull(byId);

        var updateDto = new PatientUpdateDto { Id = created.Id, FirstName = "X", LastName = "Y", Phone = "1", Email = "a@b.com" };
        await svc.UpdateAsync(updateDto);
        var updated = await repo.GetByIdAsync(updateDto.Id);
        Assert.Equal("X", updated!.FirstName);

        await svc.DeleteAsync(created.Id);
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await svc.DeleteAsync(created.Id));
    }

    // -------------------------------------------------------------------------
    // MedicalRecordService tests
    // -------------------------------------------------------------------------
    [Fact]
    public async Task MedicalRecordService_Create_Update_Delete_GetByPatient()
    {
        var repo = new InMemoryMedicalRecordRepository();
        var mapper = BuildMapper(cfg =>
        {
            cfg.CreateMap<MedicalRecordCreateDto, MedicalRecord>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore());

            cfg.CreateMap<MedicalRecordUpdateDto, MedicalRecord>()
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore());

            cfg.CreateMap<MedicalRecord, MedicalRecordDto>();
        });

        var svc = new MedicalRecordService(repo, mapper, new NullCacheService());

        var patientId = Guid.NewGuid();
        var createDto = new MedicalRecordCreateDto { PatientId = patientId, Allergies = "a" };
        var created = await svc.CreateAsync(createDto);
        Assert.Equal(patientId, created.PatientId);

        var list = (await svc.GetByPatientAsync(patientId)).ToList();
        Assert.Single(list);

        var updateDto = new MedicalRecordUpdateDto { Id = created.Id, Allergies = "b" };
        await svc.UpdateAsync(updateDto);
        var updated = await repo.GetByIdAsync(updateDto.Id);
        Assert.Equal("b", updated!.Allergies);

        await svc.DeleteAsync(created.Id);
        var after = await repo.GetByIdAsync(created.Id);
        Assert.Null(after);
    }

    // -------------------------------------------------------------------------
    // TreatmentRecordService tests
    // -------------------------------------------------------------------------
    [Fact]
    public async Task TreatmentRecordService_Create_GetByAppointment_Update_Delete()
    {
        var repo = new InMemoryTreatmentRecordRepository();
        var mapper = BuildMapper(cfg =>
        {
            cfg.CreateMap<TreatmentRecordCreateDto, TreatmentRecord>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Appointment, opt => opt.Ignore())
                .ForMember(dest => dest.Service, opt => opt.Ignore())
                .ForMember(dest => dest.AppointmentId, opt => opt.MapFrom(src => src.AppointmentId))
                .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.ServiceId));

            cfg.CreateMap<TreatmentRecordDto, TreatmentRecord>()
                .ForMember(dest => dest.Appointment, opt => opt.Ignore())
                .ForMember(dest => dest.Service, opt => opt.Ignore());

            cfg.CreateMap<TreatmentRecord, TreatmentRecordDto>();
        });

        var svc = new TreatmentRecordService(repo, mapper, new NullCacheService());

        var appointmentId = Guid.NewGuid();
        var createDto = new TreatmentRecordCreateDto
        {
            AppointmentId = appointmentId,
            PerformedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            ServiceId = Guid.NewGuid()
        };

        var created = await svc.CreateAsync(createDto);
        Assert.Equal(appointmentId, created.AppointmentId);

        var list = (await svc.GetByAppointmentAsync(appointmentId)).ToList();
        Assert.Single(list);

        var dto = new TreatmentRecordDto
        {
            Id = created.Id,
            AppointmentId = appointmentId,
            ServiceId = created.ServiceId,
            Notes = "n",
            PerformedAt = created.PerformedAt
        };
        await svc.UpdateAsync(dto);

        var updated = await repo.GetByIdAsync(created.Id);
        Assert.Equal("n", updated!.Notes);

        await svc.DeleteAsync(created.Id);
        var after = await repo.GetByIdAsync(created.Id);
        Assert.Null(after);
    }

    // -------------------------------------------------------------------------
    // DentalChartService tests
    // -------------------------------------------------------------------------
    [Fact]
    public async Task DentalChartService_Create_GetByPatient_Update_Delete()
    {
        var repo = new InMemoryDentalChartRepository();
        var mapper = BuildMapper(cfg =>
        {
            cfg.CreateMap<DentalChartCreateDto, DentalChart>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore());

            cfg.CreateMap<DentalChartUpdateDto, DentalChart>()
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                .ForMember(dest => dest.ToothNumber, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore());

            cfg.CreateMap<DentalChart, DentalChartDto>();
        });

        var svc = new DentalChartService(repo, mapper, new NullCacheService());

        var patientId = Guid.NewGuid();
        var createDto = new DentalChartCreateDto { PatientId = patientId, ToothNumber = "12", Status = "Healthy" };
        var created = await svc.CreateAsync(createDto);
        Assert.Equal("12", created.ToothNumber);

        var list = (await svc.GetByPatientAsync(patientId)).ToList();
        Assert.Single(list);

        var updateDto = new DentalChartUpdateDto { Id = created.Id, Notes = "note", Status = "Cavity" };
        await svc.UpdateAsync(updateDto);
        var updated = await repo.GetByIdAsync(created.Id);
        Assert.Equal("note", updated!.Notes);

        await svc.DeleteAsync(created.Id);
        var after = await repo.GetByIdAsync(created.Id);
        Assert.Null(after);
    }

    // -------------------------------------------------------------------------
    // AuditLogService tests
    // -------------------------------------------------------------------------
    [Fact]
    public async Task AuditLogService_Log_AddsEntry_With_CurrentUser()
    {
        var repo = new InMemoryAuditLogRepository();
        var current = new SimpleCurrentUserService { UserId = Guid.NewGuid() };
        var mapper = BuildMapper(cfg =>
            cfg.CreateMap<AuditLog, AuditLogDto>()
                .ForMember(dest => dest.ChangedByUsername, opt => opt.MapFrom(_ => (string)null!)));

        var svc = new AuditLogService(repo, current, mapper);
        var entityId = Guid.NewGuid();
        await svc.LogAsync("X", entityId, "Create");

        var all = (await repo.GetAllAsync()).ToList();
        Assert.Single(all);
        Assert.Equal("X", all[0].Entity);
        Assert.Equal(current.UserId, all[0].ChangedBy);
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------
    private static IMapper BuildMapper(Action<IMapperConfigurationExpression> configure)
    {
        var config = new MapperConfiguration(configure);
        config.AssertConfigurationIsValid();
        return config.CreateMapper();
    }

    // -------------------------------------------------------------------------
    // In-memory repositories
    // -------------------------------------------------------------------------
    private class InMemoryServiceRepository : IServiceRepository
    {
        private readonly ConcurrentDictionary<Guid, Service> _store = new();
        public Task AddAsync(Service e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<Service>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<Service?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var v); return Task.FromResult(v); }
        public Task UpdateAsync(Service e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
        public Task<bool> ExistsByCodeAsync(string code) => Task.FromResult(_store.Values.Any(s => s.Code == code));
    }

    private class InMemoryAppointmentRepository : IAppointmentRepository
    {
        private readonly ConcurrentDictionary<Guid, Appointment> _store = new();
        public Task AddAsync(Appointment e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<Appointment>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<Appointment?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var v); return Task.FromResult(v); }
        public Task<IEnumerable<Appointment>> GetByDoctorIdAsync(Guid id) => Task.FromResult(_store.Values.Where(x => x.DoctorId == id).AsEnumerable());
        public Task<IEnumerable<Appointment>> GetByPatientIdAsync(Guid id) => Task.FromResult(_store.Values.Where(x => x.PatientId == id).AsEnumerable());
        public Task UpdateAsync(Appointment e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    private class InMemoryAuditLogService : IAuditLogService
    {
        public List<(string entity, Guid entityId, string action)> Logs { get; } = new();
        public Task<IEnumerable<AuditLogDto>> GetAllAsync() => Task.FromResult(Enumerable.Empty<AuditLogDto>());
        public Task LogAsync(string entity, Guid entityId, string action) { Logs.Add((entity, entityId, action)); return Task.CompletedTask; }
    }

    private class InMemoryUserRepository : IUserRepository
    {
        private readonly ConcurrentDictionary<Guid, User> _store = new();
        public Task AddAsync(User e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<User>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<User?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var v); return Task.FromResult(v); }
        public Task<User?> GetByUsernameAsync(string username) => Task.FromResult(_store.Values.FirstOrDefault(x => x.Username == username));
        public Task UpdateAsync(User e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    private class InMemoryDoctorRepository : IDoctorRepository
    {
        private readonly ConcurrentDictionary<Guid, Doctor> _store = new();
        public Task AddAsync(Doctor e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<Doctor>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<Doctor?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var v); return Task.FromResult(v); }
        public Task UpdateAsync(Doctor e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    private class InMemoryPaymentRepository : IPaymentRepository
    {
        private readonly ConcurrentDictionary<Guid, Payment> _store = new();
        public Task AddAsync(Payment e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<Payment>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<Payment?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var v); return Task.FromResult(v); }
        public Task UpdateAsync(Payment e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    private class InMemoryPatientRepository : IPatientRepository
    {
        private readonly ConcurrentDictionary<Guid, Patient> _store = new();
        public Task AddAsync(Patient e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<Patient>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<Patient?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var v); return Task.FromResult(v); }
        public Task UpdateAsync(Patient e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    private class InMemoryMedicalRecordRepository : IMedicalRecordRepository
    {
        private readonly ConcurrentDictionary<Guid, MedicalRecord> _store = new();
        public Task AddAsync(MedicalRecord e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<MedicalRecord>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<MedicalRecord?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var v); return Task.FromResult(v); }
        public Task UpdateAsync(MedicalRecord e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
        public Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(Guid patientId) => Task.FromResult(_store.Values.Where(x => x.PatientId == patientId).AsEnumerable());
    }

    private class InMemoryTreatmentRecordRepository : ITreatmentRecordRepository
    {
        private readonly ConcurrentDictionary<Guid, TreatmentRecord> _store = new();
        public Task AddAsync(TreatmentRecord e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<TreatmentRecord>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<TreatmentRecord?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var v); return Task.FromResult(v); }
        public Task UpdateAsync(TreatmentRecord e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
        public Task<IEnumerable<TreatmentRecord>> GetByAppointmentAsync(Guid appointmentId) => Task.FromResult(_store.Values.Where(x => x.AppointmentId == appointmentId).AsEnumerable());
    }

    private class InMemoryDentalChartRepository : IDentalChartRepository
    {
        private readonly ConcurrentDictionary<Guid, DentalChart> _store = new();
        public Task AddAsync(DentalChart e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<DentalChart>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<DentalChart?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var v); return Task.FromResult(v); }
        public Task UpdateAsync(DentalChart e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
        public Task<IEnumerable<DentalChart>> GetByPatientAsync(Guid patientId) => Task.FromResult(_store.Values.Where(x => x.PatientId == patientId).AsEnumerable());
    }

    private class InMemoryAuditLogRepository : IAuditLogRepository
    {
        private readonly ConcurrentDictionary<Guid, AuditLog> _store = new();
        public Task AddAsync(AuditLog e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<AuditLog>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<AuditLog?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var v); return Task.FromResult(v); }
        public Task UpdateAsync(AuditLog e) { _store[e.Id] = e; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
        public Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, Guid entityId) => Task.FromResult(_store.Values.Where(x => x.Entity == entityName && x.EntityId == entityId).AsEnumerable());
    }

    private class SimpleCurrentUserService : Zubs.Application.Interfaces.Helpers.ICurrentUserService
    {
        public Guid? UserId { get; set; }
    }
}