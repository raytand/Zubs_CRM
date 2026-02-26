using AutoMapper;
using Zubs.Application.DTOs;
using Zubs.Domain.Entities;

namespace Zubs.Application.Mappings;

public class PatientProfile : Profile
{
    public PatientProfile()
    {
        CreateMap<PatientCreateDto, Patient>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.MedicalRecords, opt => opt.Ignore())
            .ForMember(d => d.DentalCharts, opt => opt.Ignore())
            .ForMember(d => d.Appointments, opt => opt.Ignore())
            .ForMember(d => d.Payments, opt => opt.Ignore());

        CreateMap<PatientUpdateDto, Patient>()
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.MedicalRecords, opt => opt.Ignore())
            .ForMember(d => d.DentalCharts, opt => opt.Ignore())
            .ForMember(d => d.Appointments, opt => opt.Ignore())
            .ForMember(d => d.Payments, opt => opt.Ignore());

        CreateMap<Patient, PatientDto>();
    }
}
