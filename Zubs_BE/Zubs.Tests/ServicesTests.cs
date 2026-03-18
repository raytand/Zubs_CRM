using System.Collections.Concurrent;
using AutoMapper;
using Zubs.Application.DTOs;
using Zubs.Application.Exceptions;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Application.Interfaces.Services;
using Zubs.Application.Services;
using Zubs.Domain.Entities;
using Zubs.Domain.Enums;
using Xunit;
using System.Linq;
using System;

namespace Zubs.Tests;

public class ServicesTests
{
    [Fact]
    public async Task ServiceService_Create_GetAll_GetById_Update_Delete_And_DuplicateCode()
    {
        var repo = new InMemoryServiceRepository();

        // AutoMapper configuration for Service mappings
        var config = new MapperConfiguration(cfg =>
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
        config.AssertConfigurationIsValid();
        var mapper = config.CreateMapper();

        var svc = new ServiceService(repo, mapper);

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

        // Update
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

        // Duplicate code check
        var createDto2 = new ServiceCreateDto
        {
            Code = "SVC001",
            Name = "Another",
            Description = "",
            Price = 50m
        };

        await Assert.ThrowsAsync<AppException>(async () => await svc.CreateAsync(createDto2));

        // Delete
        await svc.DeleteAsync(created.Id);
        var afterDelete = await svc.GetByIdAsync(created.Id);
        Assert.Null(afterDelete);

        // Deleting non-existing should throw
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await svc.DeleteAsync(created.Id));
    }

    [Fact]
    public async Task AppointmentService_Create_GetByDoctor_GetByPatient_Update_Delete_And_NotFound()
    {
        var repo = new InMemoryAppointmentRepository();
        var audit = new InMemoryAuditLogService();

        // AutoMapper configuration for Appointment mappings
        var config = new MapperConfiguration(cfg =>
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
        config.AssertConfigurationIsValid();
        var mapper = config.CreateMapper();

        var svc = new AppointmentService(repo, audit, mapper);

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

        // Update
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

        // Ensure audit logged update
        Assert.Contains(audit.Logs, l => l.action == "Update Appointment" && l.entity == nameof(Appointment) && l.entityId == updated.Id);

        // Delete
        await svc.DeleteAsync(created.Id);
        var afterDelete = await repo.GetByIdAsync(created.Id);
        Assert.Null(afterDelete);

        // Ensure audit logged delete
        Assert.Contains(audit.Logs, l => l.action == "Delete Appointment" && l.entity == nameof(Appointment) && l.entityId == created.Id);

        // Update non-existing should throw
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await svc.UpdateAsync(new AppointmentUpdateDto { Id = Guid.NewGuid(), StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1), Status = AppointmentStatus.Scheduled }));
    }

    // Additional tests for other services
    [Fact]
    public async Task DoctorService_Create_Update_Delete_And_UserNotFound()
    {
        var userRepo = new InMemoryUserRepoForDoctor();
        var repo = new InMemoryDoctorRepository();

        // AutoMapper for Doctor
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<Zubs.Application.DTOs.DoctorCreateDto, Zubs.Domain.Entities.Doctor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore());

            c.CreateMap<Zubs.Application.DTOs.DoctorUpdateDto, Zubs.Domain.Entities.Doctor>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore());

            c.CreateMap<Zubs.Domain.Entities.Doctor, Zubs.Application.DTOs.DoctorDto>();
        });
        cfg.AssertConfigurationIsValid();
        var mapper = cfg.CreateMapper();

        var svc = new Zubs.Application.Services.DoctorService(repo, userRepo, mapper);

        var user = new Zubs.Domain.Entities.User { Id = Guid.NewGuid(), Username = "u", Email = "u@x.com", PasswordHash = "h", Role = Zubs.Domain.Enums.UserRole.Doctor, CreatedAt = DateTime.UtcNow };
        await userRepo.AddAsync(user);

        var createDto = new Zubs.Application.DTOs.DoctorCreateDto { UserId = user.Id, FirstName = "A", LastName = "B", Email = "e@e.com" };
        var created = await svc.CreateAsync(createDto);
        Assert.NotNull(created);
        Assert.Equal(createDto.FirstName, created.FirstName);

        // Update
        var updateDto = new Zubs.Application.DTOs.DoctorUpdateDto { Id = created.Id, FirstName = "AA", LastName = "BB" };
        await svc.UpdateAsync(updateDto);
        var updated = await repo.GetByIdAsync(updateDto.Id);
        Assert.Equal("AA", updated!.FirstName);

        // Delete
        await svc.DeleteAsync(created.Id);
        var after = await repo.GetByIdAsync(created.Id);
        Assert.Null(after);

        // Create with missing user should throw
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await svc.CreateAsync(new Zubs.Application.DTOs.DoctorCreateDto { UserId = Guid.NewGuid(), FirstName = "X", LastName = "Y" }));
    }

    [Fact]
    public async Task PaymentService_Create_Update_Delete_And_Errors()
    {
        var repo = new InMemoryPaymentRepository();
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<Zubs.Application.DTOs.PaymentCreateDto, Zubs.Domain.Entities.Payment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.Appointment, opt => opt.Ignore());

            c.CreateMap<Zubs.Application.DTOs.PaymentUpdateDto, Zubs.Domain.Entities.Payment>()
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                .ForMember(dest => dest.AppointmentId, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.Appointment, opt => opt.Ignore());

            c.CreateMap<Zubs.Domain.Entities.Payment, Zubs.Application.DTOs.PaymentDto>();
        });
        cfg.AssertConfigurationIsValid();
        var mapper = cfg.CreateMapper();

        var svc = new Zubs.Application.Services.PaymentService(repo, mapper);

        var patientId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        var createDto = new Zubs.Application.DTOs.PaymentCreateDto { PatientId = patientId, AppointmentId = appointmentId, Amount = 200m, PaidAt = DateOnly.FromDateTime(DateTime.UtcNow), Method = Zubs.Domain.Enums.PaymentMethod.Cash };
        var created = await svc.CreateAsync(createDto);
        Assert.NotNull(created);
        Assert.Equal(200m, created.Amount);

        // Update with empty id should throw
        await Assert.ThrowsAsync<ArgumentException>(async () => await svc.UpdateAsync(new Zubs.Application.DTOs.PaymentUpdateDto { Id = Guid.Empty }));

        // Update non-existing should throw KeyNotFound
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await svc.UpdateAsync(new Zubs.Application.DTOs.PaymentUpdateDto { Id = Guid.NewGuid(), PatientId = patientId, AppointmentId = appointmentId, Amount = 10m, PaidAt = DateOnly.FromDateTime(DateTime.UtcNow), Method = Zubs.Domain.Enums.PaymentMethod.Cash }));

        // Delete existing no throw
        await svc.DeleteAsync(created.Id);
        var after = await repo.GetByIdAsync(created.Id);
        Assert.Null(after);
    }

    [Fact]
    public async Task PatientService_Create_Update_Delete_NotFound()
    {
        var repo = new InMemoryPatientRepository();
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<Zubs.Application.DTOs.PatientCreateDto, Zubs.Domain.Entities.Patient>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore())
                .ForMember(dest => dest.MedicalRecords, opt => opt.Ignore())
                .ForMember(dest => dest.DentalCharts, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore());

            c.CreateMap<Zubs.Application.DTOs.PatientUpdateDto, Zubs.Domain.Entities.Patient>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore())
                .ForMember(dest => dest.MedicalRecords, opt => opt.Ignore())
                .ForMember(dest => dest.DentalCharts, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore());

            c.CreateMap<Zubs.Domain.Entities.Patient, Zubs.Application.DTOs.PatientDto>();
        });
        cfg.AssertConfigurationIsValid();
        var mapper = cfg.CreateMapper();

        var svc = new Zubs.Application.Services.PatientService(repo, mapper);

        var createDto = new Zubs.Application.DTOs.PatientCreateDto { FirstName = "Fn", LastName = "Ln", Phone = "123", Email = "e@e.com" };
        var created = await svc.CreateAsync(createDto);
        Assert.NotNull(created);

        var updateDto = new Zubs.Application.DTOs.PatientUpdateDto { Id = created.Id, FirstName = "X", LastName = "Y", Phone = "1", Email = "a@b.com" };
        await svc.UpdateAsync(updateDto);
        var updated = await repo.GetByIdAsync(updateDto.Id);
        Assert.Equal("X", updated!.FirstName);

        await svc.DeleteAsync(created.Id);
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await svc.DeleteAsync(created.Id));
    }

    [Fact]
    public async Task MedicalRecordService_Create_Update_Delete_GetByPatient()
    {
        var repo = new InMemoryMedicalRecordRepository();
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<Zubs.Application.DTOs.MedicalRecordCreateDto, Zubs.Domain.Entities.MedicalRecord>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore());

            c.CreateMap<Zubs.Application.DTOs.MedicalRecordUpdateDto, Zubs.Domain.Entities.MedicalRecord>()
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore());

            c.CreateMap<Zubs.Domain.Entities.MedicalRecord, Zubs.Application.DTOs.MedicalRecordDto>();
        });
        cfg.AssertConfigurationIsValid();
        var mapper = cfg.CreateMapper();

        var svc = new Zubs.Application.Services.MedicalRecordService(repo, mapper);

        var patientId = Guid.NewGuid();
        var createDto = new Zubs.Application.DTOs.MedicalRecordCreateDto { PatientId = patientId, Allergies = "a" };
        var created = await svc.CreateAsync(createDto);
        Assert.Equal(patientId, created.PatientId);

        var list = (await svc.GetByPatientAsync(patientId)).ToList();
        Assert.Single(list);

        var updateDto = new Zubs.Application.DTOs.MedicalRecordUpdateDto { Id = created.Id, Allergies = "b" };
        await svc.UpdateAsync(updateDto);
        var updated = await repo.GetByIdAsync(updateDto.Id);
        Assert.Equal("b", updated!.Allergies);

        await svc.DeleteAsync(created.Id);
        var after = await repo.GetByIdAsync(created.Id);
        Assert.Null(after);
    }

    [Fact]
    public async Task TreatmentRecordService_Create_GetByAppointment_Update_Delete()
    {
        var repo = new InMemoryTreatmentRecordRepository();
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<Zubs.Application.DTOs.TreatmentRecordCreateDto, Zubs.Domain.Entities.TreatmentRecord>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Appointment, opt => opt.Ignore())
                .ForMember(dest => dest.Service, opt => opt.Ignore())
                .ForMember(dest => dest.AppointmentId, opt => opt.MapFrom(src => src.AppointmentId))
                .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.ServiceId));

            c.CreateMap<Zubs.Application.DTOs.TreatmentRecordDto, Zubs.Domain.Entities.TreatmentRecord>()
                .ForMember(dest => dest.Appointment, opt => opt.Ignore())
                .ForMember(dest => dest.Service, opt => opt.Ignore());

            c.CreateMap<Zubs.Domain.Entities.TreatmentRecord, Zubs.Application.DTOs.TreatmentRecordDto>();
        });
        cfg.AssertConfigurationIsValid();
        var mapper = cfg.CreateMapper();

        var svc = new Zubs.Application.Services.TreatmentRecordService(repo, mapper);

        var appointmentId = Guid.NewGuid();
        var createDto = new Zubs.Application.DTOs.TreatmentRecordCreateDto { AppointmentId = appointmentId, PerformedAt = DateOnly.FromDateTime(DateTime.UtcNow), ServiceId = Guid.NewGuid() };
        var created = await svc.CreateAsync(createDto);
        Assert.Equal(appointmentId, created.AppointmentId);

        var list = (await svc.GetByAppointmentAsync(appointmentId)).ToList();
        Assert.Single(list);

        var dto = new Zubs.Application.DTOs.TreatmentRecordDto { Id = created.Id, AppointmentId = appointmentId, ServiceId = created.ServiceId, Notes = "n", PerformedAt = created.PerformedAt };
        await svc.UpdateAsync(dto);

        var updated = await repo.GetByIdAsync(created.Id);
        Assert.Equal("n", updated!.Notes);

        await svc.DeleteAsync(created.Id);
        var after = await repo.GetByIdAsync(created.Id);
        Assert.Null(after);
    }

    [Fact]
    public async Task DentalChartService_Create_GetByPatient_Update_Delete()
    {
        var repo = new InMemoryDentalChartRepository();
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<Zubs.Application.DTOs.DentalChartCreateDto, Zubs.Domain.Entities.DentalChart>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore());

            c.CreateMap<Zubs.Application.DTOs.DentalChartUpdateDto, Zubs.Domain.Entities.DentalChart>()
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                .ForMember(dest => dest.ToothNumber, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore());

            c.CreateMap<Zubs.Domain.Entities.DentalChart, Zubs.Application.DTOs.DentalChartDto>();
        });
        cfg.AssertConfigurationIsValid();
        var mapper = cfg.CreateMapper();

        var svc = new Zubs.Application.Services.DentalChartService(repo, mapper);

        var patientId = Guid.NewGuid();
        var createDto = new Zubs.Application.DTOs.DentalChartCreateDto { PatientId = patientId, ToothNumber = "12", Status = "Healthy" };
        var created = await svc.CreateAsync(createDto);
        Assert.Equal("12", created.ToothNumber);

        var list = (await svc.GetByPatientAsync(patientId)).ToList();
        Assert.Single(list);

        var updateDto = new Zubs.Application.DTOs.DentalChartUpdateDto { Id = created.Id, Notes = "note", Status = "Cavity" };
        await svc.UpdateAsync(updateDto);
        var updated = await repo.GetByIdAsync(created.Id);
        Assert.Equal("note", updated!.Notes);

        await svc.DeleteAsync(created.Id);
        var after = await repo.GetByIdAsync(created.Id);
        Assert.Null(after);
    }

    [Fact]
    public async Task AuditLogService_Log_AddsEntry_With_CurrentUser()
    {
        var repo = new InMemoryAuditLogRepository();
        var current = new SimpleCurrentUserService { UserId = Guid.NewGuid() };
        var cfg = new MapperConfiguration(c => c.CreateMap<Zubs.Domain.Entities.AuditLog, Zubs.Application.DTOs.AuditLogDto>()
            .ForMember(dest => dest.ChangedByUsername, opt => opt.MapFrom(_ => (string)null!)));
        cfg.AssertConfigurationIsValid();
        var mapper = cfg.CreateMapper();

        var svc = new Zubs.Application.Services.AuditLogService(repo, current, mapper);
        var entityId = Guid.NewGuid();
        await svc.LogAsync("X", entityId, "Create");

        var all = (await repo.GetAllAsync()).ToList();
        Assert.Single(all);
        Assert.Equal("X", all[0].Entity);
        Assert.Equal(current.UserId, all[0].ChangedBy);
    }

    // In-memory implementations used for tests
    private class InMemoryServiceRepository : IServiceRepository
    {
        private readonly ConcurrentDictionary<Guid, Service> _store = new();

        public Task AddAsync(Service entity)
        {
            _store[entity.Id] = entity;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            _store.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Service>> GetAllAsync()
            => Task.FromResult(_store.Values.AsEnumerable());

        public Task<Service?> GetByIdAsync(Guid id)
        {
            _store.TryGetValue(id, out var s);
            return Task.FromResult(s);
        }

        public Task UpdateAsync(Service entity)
        {
            _store[entity.Id] = entity;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync() => Task.CompletedTask;

        public Task<bool> ExistsByCodeAsync(string code)
        {
            var exists = _store.Values.Any(s => s.Code == code);
            return Task.FromResult(exists);
        }
    }

    private class InMemoryAppointmentRepository : IAppointmentRepository
    {
        private readonly ConcurrentDictionary<Guid, Appointment> _store = new();

        public Task AddAsync(Appointment entity)
        {
            _store[entity.Id] = entity;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            _store.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Appointment>> GetAllAsync()
            => Task.FromResult(_store.Values.AsEnumerable());

        public Task<Appointment?> GetByIdAsync(Guid id)
        {
            _store.TryGetValue(id, out var a);
            return Task.FromResult(a);
        }

        public Task<IEnumerable<Appointment>> GetByDoctorIdAsync(Guid doctorId)
            => Task.FromResult(_store.Values.Where(x => x.DoctorId == doctorId).AsEnumerable());

        public Task<IEnumerable<Appointment>> GetByPatientIdAsync(Guid patientId)
            => Task.FromResult(_store.Values.Where(x => x.PatientId == patientId).AsEnumerable());

        public Task UpdateAsync(Appointment entity)
        {
            _store[entity.Id] = entity;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    private class InMemoryAuditLogService : IAuditLogService
    {
        public List<(string entity, Guid entityId, string action)> Logs { get; } = new();

        public Task<IEnumerable<Zubs.Application.DTOs.AuditLogDto>> GetAllAsync()
        {
            return Task.FromResult(Enumerable.Empty<Zubs.Application.DTOs.AuditLogDto>());
        }

        public Task LogAsync(string entity, Guid entityId, string action)
        {
            Logs.Add((entity, entityId, action));
            return Task.CompletedTask;
        }
    }

    // Additional in-memory implementations for new tests
    private class InMemoryUserRepoForDoctor : Zubs.Application.Interfaces.Repositories.IUserRepository
    {
        private readonly ConcurrentDictionary<Guid, Zubs.Domain.Entities.User> _store = new();
        public Task AddAsync(Zubs.Domain.Entities.User entity) { _store[entity.Id] = entity; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<Zubs.Domain.Entities.User>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<Zubs.Domain.Entities.User?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var u); return Task.FromResult(u); }
        public Task<Zubs.Domain.Entities.User?> GetByUsernameAsync(string username) { var u = _store.Values.FirstOrDefault(x => x.Username == username); return Task.FromResult(u); }
        public Task SaveChangesAsync() => Task.CompletedTask;
        public Task UpdateAsync(Zubs.Domain.Entities.User entity) { _store[entity.Id] = entity; return Task.CompletedTask; }
    }

    private class InMemoryDoctorRepository : Zubs.Application.Interfaces.Repositories.IDoctorRepository
    {
        private readonly ConcurrentDictionary<Guid, Zubs.Domain.Entities.Doctor> _store = new();
        public Task AddAsync(Zubs.Domain.Entities.Doctor entity) { _store[entity.Id] = entity; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<Zubs.Domain.Entities.Doctor>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<Zubs.Domain.Entities.Doctor?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var d); return Task.FromResult(d); }
        public Task UpdateAsync(Zubs.Domain.Entities.Doctor entity) { _store[entity.Id] = entity; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    private class InMemoryPaymentRepository : Zubs.Application.Interfaces.Repositories.IPaymentRepository
    {
        private readonly ConcurrentDictionary<Guid, Zubs.Domain.Entities.Payment> _store = new();
        public Task AddAsync(Zubs.Domain.Entities.Payment entity) { _store[entity.Id] = entity; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<Zubs.Domain.Entities.Payment>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<Zubs.Domain.Entities.Payment?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var v); return Task.FromResult(v); }
        public Task UpdateAsync(Zubs.Domain.Entities.Payment entity) { _store[entity.Id] = entity; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    private class InMemoryPatientRepository : Zubs.Application.Interfaces.Repositories.IPatientRepository
    {
        private readonly ConcurrentDictionary<Guid, Zubs.Domain.Entities.Patient> _store = new();
        public Task AddAsync(Zubs.Domain.Entities.Patient entity) { _store[entity.Id] = entity; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<Zubs.Domain.Entities.Patient>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<Zubs.Domain.Entities.Patient?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var v); return Task.FromResult(v); }
        public Task UpdateAsync(Zubs.Domain.Entities.Patient entity) { _store[entity.Id] = entity; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    private class InMemoryMedicalRecordRepository : Zubs.Application.Interfaces.Repositories.IMedicalRecordRepository
    {
        private readonly ConcurrentDictionary<Guid, Zubs.Domain.Entities.MedicalRecord> _store = new();
        public Task AddAsync(Zubs.Domain.Entities.MedicalRecord entity) { _store[entity.Id] = entity; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<Zubs.Domain.Entities.MedicalRecord>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<Zubs.Domain.Entities.MedicalRecord?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var v); return Task.FromResult(v); }
        public Task UpdateAsync(Zubs.Domain.Entities.MedicalRecord entity) { _store[entity.Id] = entity; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
        public Task<IEnumerable<Zubs.Domain.Entities.MedicalRecord>> GetByPatientIdAsync(Guid patientId) => Task.FromResult(_store.Values.Where(x => x.PatientId == patientId).AsEnumerable());
    }

    private class InMemoryTreatmentRecordRepository : Zubs.Application.Interfaces.Repositories.ITreatmentRecordRepository
    {
        private readonly ConcurrentDictionary<Guid, Zubs.Domain.Entities.TreatmentRecord> _store = new();
        public Task AddAsync(Zubs.Domain.Entities.TreatmentRecord entity) { _store[entity.Id] = entity; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<Zubs.Domain.Entities.TreatmentRecord>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<Zubs.Domain.Entities.TreatmentRecord?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var v); return Task.FromResult(v); }
        public Task UpdateAsync(Zubs.Domain.Entities.TreatmentRecord entity) { _store[entity.Id] = entity; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
        public Task<IEnumerable<Zubs.Domain.Entities.TreatmentRecord>> GetByAppointmentAsync(Guid appointmentId) => Task.FromResult(_store.Values.Where(x => x.AppointmentId == appointmentId).AsEnumerable());
    }

    private class InMemoryDentalChartRepository : Zubs.Application.Interfaces.Repositories.IDentalChartRepository
    {
        private readonly ConcurrentDictionary<Guid, Zubs.Domain.Entities.DentalChart> _store = new();
        public Task AddAsync(Zubs.Domain.Entities.DentalChart entity) { _store[entity.Id] = entity; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<Zubs.Domain.Entities.DentalChart>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<Zubs.Domain.Entities.DentalChart?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var v); return Task.FromResult(v); }
        public Task UpdateAsync(Zubs.Domain.Entities.DentalChart entity) { _store[entity.Id] = entity; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
        public Task<IEnumerable<Zubs.Domain.Entities.DentalChart>> GetByPatientAsync(Guid patientId) => Task.FromResult(_store.Values.Where(x => x.PatientId == patientId).AsEnumerable());
    }

    private class InMemoryAuditLogRepository : Zubs.Application.Interfaces.Repositories.IAuditLogRepository
    {
        private readonly ConcurrentDictionary<Guid, Zubs.Domain.Entities.AuditLog> _store = new();
        public Task AddAsync(Zubs.Domain.Entities.AuditLog entity) { _store[entity.Id] = entity; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _store.TryRemove(id, out _); return Task.CompletedTask; }
        public Task<IEnumerable<Zubs.Domain.Entities.AuditLog>> GetAllAsync() => Task.FromResult(_store.Values.AsEnumerable());
        public Task<Zubs.Domain.Entities.AuditLog?> GetByIdAsync(Guid id) { _store.TryGetValue(id, out var v); return Task.FromResult(v); }
        public Task UpdateAsync(Zubs.Domain.Entities.AuditLog entity) { _store[entity.Id] = entity; return Task.CompletedTask; }
        public Task SaveChangesAsync() => Task.CompletedTask;
        public Task<IEnumerable<Zubs.Domain.Entities.AuditLog>> GetByEntityAsync(string entityName, Guid entityId) => Task.FromResult(_store.Values.Where(x => x.Entity == entityName && x.EntityId == entityId).AsEnumerable());
    }

    private class SimpleCurrentUserService : Zubs.Application.Interfaces.Helpers.ICurrentUserService
    {
        public Guid? UserId { get; set; }
    }
}