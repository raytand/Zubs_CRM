using AutoMapper;
using Zubs.Application.DTOs;
using Zubs.Domain.Entities;

namespace Zubs.Application.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();

            CreateMap<Patient, PatientDto>().ReverseMap().ForMember(d => d.CreatedAt, opt => opt.Ignore());

            CreateMap<Doctor, DoctorDto>().ReverseMap();
            CreateMap<DoctorDto, Doctor>().ForMember(d => d.User, opt => opt.Ignore());
            CreateMap<DoctorUpdateDto, Doctor>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.UserId, opt => opt.Ignore())
                .ForMember(d => d.User, opt => opt.Ignore());
            CreateMap<DoctorCreateDto, Doctor>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.User, opt => opt.Ignore())
                .ForMember(d => d.UserId, opt => opt.Ignore());

            CreateMap<Appointment, AppointmentDto>().ReverseMap();
            CreateMap<AppointmentCreateDto, Appointment>()
                .ForMember(d => d.Id, opt => opt.Ignore());
            CreateMap<AppointmentUpdateDto, Appointment>()
                .ForMember(d => d.Id, opt => opt.Ignore());

            CreateMap<Service, ServiceDto>();
            CreateMap<ServiceCreateDto, Service>()
                .ForMember(d => d.Id, opt => opt.Ignore());
            CreateMap<ServiceUpdateDto, Service>()
                .ForMember(d => d.Id, opt => opt.Ignore());

            CreateMap<Payment, PaymentDto>().ReverseMap();
            CreateMap<PaymentCreateDto, Payment>()
                .ForMember(p => p.Id, opt => opt.Ignore());
            CreateMap<PaymentUpdateDto, Payment>()
                .ForMember(p => p.Id, opt => opt.Ignore());


            CreateMap<TreatmentRecord, TreatmentRecordDto>();
            CreateMap<TreatmentRecordCreateDto, TreatmentRecord>();
            CreateMap<TreatmentRecordDto, TreatmentRecord>();

            CreateMap<DentalChart, DentalChartDto>();
            CreateMap<DentalChartCreateDto, DentalChart>();
            CreateMap<DentalChartUpdateDto, DentalChart>();

            CreateMap<MedicalRecord, MedicalRecordDto>().ReverseMap().ForMember(d => d.CreatedAt, opt => opt.Ignore());
            CreateMap<MedicalRecordCreateDto, MedicalRecord>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore());
            CreateMap<MedicalRecordUpdateDto, MedicalRecord>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore());


            CreateMap<AuditLog, AuditLogDto>()
                .ForMember(dest => dest.ChangedByUsername, opt => opt.MapFrom(src => src.User!.Username))
                .ReverseMap();

            CreateMap<User, UserDto>();
            CreateMap<UserUpdateDto, User>()
                .ForMember(x => x.PasswordHash, opt => opt.Ignore())
                .ForMember(x => x.Username, opt => opt.Ignore());

        }
    }
}
