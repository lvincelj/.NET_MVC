# Sitemap

This sitemap maps reachable MVC endpoints to controller action and rendered view.

## Attribute-routed endpoints

| Method | URL | Controller | Action | View |
|---|---|---|---|---|
| GET | /patients | PatientsController | Index | Views/Patients/Index.cshtml |
| GET | /patients/{id:int} | PatientsController | Details | Views/Patients/Details.cshtml |
| GET | /staff/doctors | DoctorsController | Index | Views/Doctors/Index.cshtml |
| GET | /schedule/appointments/{id:int} | AppointmentsController | Details | Views/Appointments/Details.cshtml |
| GET | /medical-records | MedicalRecordsController | Index | Views/MedicalRecords/Index.cshtml |
| GET | /medical-records/{id:int} | MedicalRecordsController | Details | Views/MedicalRecords/Details.cshtml |
| POST | /medical-records/{id:int}/mark-reviewed | MedicalRecordsController | MarkReviewed | No direct view (redirects to MedicalRecords/Details) |

## Conventional-route endpoints

Route template configured in Program.cs:
/{controller=Home}/{action=Index}/{id?}

### Home
| Method | URL | Controller | Action | View |
|---|---|---|---|---|
| GET | / | HomeController | Index | Views/Home/Index.cshtml |
| GET | /Home | HomeController | Index | Views/Home/Index.cshtml |
| GET | /Home/Index | HomeController | Index | Views/Home/Index.cshtml |
| GET | /Home/Privacy | HomeController | Privacy | Views/Home/Privacy.cshtml |
| GET | /Home/Error | HomeController | Error | Views/Shared/Error.cshtml |

### Appointments
| Method | URL | Controller | Action | View |
|---|---|---|---|---|
| GET | /Appointments | AppointmentsController | Index | Views/Appointments/Index.cshtml |
| GET | /Appointments/Index | AppointmentsController | Index | Views/Appointments/Index.cshtml |

### Departments
| Method | URL | Controller | Action | View |
|---|---|---|---|---|
| GET | /Departments | DepartmentsController | Index | Views/Departments/Index.cshtml |
| GET | /Departments/Index | DepartmentsController | Index | Views/Departments/Index.cshtml |
| GET | /Departments/Details/{id} | DepartmentsController | Details | Views/Departments/Details.cshtml |

### Doctors
| Method | URL | Controller | Action | View |
|---|---|---|---|---|
| GET | /Doctors/Details/{id} | DoctorsController | Details | Views/Doctors/Details.cshtml |

### Medications
| Method | URL | Controller | Action | View |
|---|---|---|---|---|
| GET | /Medications | MedicationsController | Index | Views/Medications/Index.cshtml |
| GET | /Medications/Index | MedicationsController | Index | Views/Medications/Index.cshtml |
| GET | /Medications/Details/{id} | MedicationsController | Details | Views/Medications/Details.cshtml |

### Prescriptions
| Method | URL | Controller | Action | View |
|---|---|---|---|---|
| GET | /Prescriptions | PrescriptionsController | Index | Views/Prescriptions/Index.cshtml |
| GET | /Prescriptions/Index | PrescriptionsController | Index | Views/Prescriptions/Index.cshtml |
| GET | /Prescriptions/Details/{id} | PrescriptionsController | Details | Views/Prescriptions/Details.cshtml |

## Notes
- Patients and MedicalRecords endpoints are defined with attribute routing, so those actions are mapped by their explicit URL templates.
- Some controllers contain a mix of attribute-routed and conventionally-routed actions (for example Doctors and Appointments).
- This sitemap covers MVC action URLs; it does not enumerate static files served from wwwroot.
