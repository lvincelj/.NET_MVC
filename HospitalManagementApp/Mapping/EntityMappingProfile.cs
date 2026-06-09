using AutoMapper;
using HospitalManagementApp.DTOs;
using HospitalManagementApp.Models;

namespace HospitalManagementApp.Mapping;

public class EntityMappingProfile : Profile
{
    public EntityMappingProfile()
    {
        CreateMap<Department, DepartmentSummaryDto>().ReverseMap();
        CreateMap<Doctor, DoctorSummaryDto>().ReverseMap();
        CreateMap<Patient, PatientSummaryDto>().ReverseMap();
        CreateMap<Appointment, AppointmentSummaryDto>().ReverseMap();
        CreateMap<MedicalRecord, MedicalRecordSummaryDto>().ReverseMap();
        CreateMap<Prescription, PrescriptionSummaryDto>().ReverseMap();
        CreateMap<Medication, MedicationSummaryDto>().ReverseMap();

        CreateMap<Department, DepartmentDto>().ReverseMap();
        CreateMap<CreateDepartmentDto, Department>();
        CreateMap<UpdateDepartmentDto, Department>();

        CreateMap<Doctor, DoctorDto>().ReverseMap();
        CreateMap<CreateDoctorDto, Doctor>();
        CreateMap<UpdateDoctorDto, Doctor>();

        CreateMap<Patient, PatientDto>().ReverseMap();
        CreateMap<CreatePatientDto, Patient>();
        CreateMap<UpdatePatientDto, Patient>();

        CreateMap<Appointment, AppointmentDto>().ReverseMap();
        CreateMap<CreateAppointmentDto, Appointment>();
        CreateMap<UpdateAppointmentDto, Appointment>();

        CreateMap<MedicalRecord, MedicalRecordDto>().ReverseMap();
        CreateMap<CreateMedicalRecordDto, MedicalRecord>();
        CreateMap<UpdateMedicalRecordDto, MedicalRecord>();

        CreateMap<Prescription, PrescriptionDto>().ReverseMap();
        CreateMap<CreatePrescriptionDto, Prescription>();
        CreateMap<UpdatePrescriptionDto, Prescription>();

        CreateMap<Medication, MedicationDto>().ReverseMap();
        CreateMap<CreateMedicationDto, Medication>();
        CreateMap<UpdateMedicationDto, Medication>();
    }
}
