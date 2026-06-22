const { test, expect, request } = require("@playwright/test");

const ADMIN_EMAIL = process.env.PLAYWRIGHT_ADMIN_EMAIL || "admin@hospital.local";
const ADMIN_PASSWORD = process.env.PLAYWRIGHT_ADMIN_PASSWORD || "Admin12345";
const MISSING_ID = 999999;

const timestamp = () => Date.now();
const isoNow = () => new Date().toISOString();
const isoFuture = (days = 1) => new Date(Date.now() + days * 24 * 60 * 60 * 1000).toISOString();

let authRequest;
const created = {
  Medications: new Set(),
  Prescriptions: new Set(),
  MedicalRecords: new Set(),
  Appointments: new Set(),
  Departments: new Set(),
  Doctors: new Set(),
  Patients: new Set()
};

const resources = {
  Patients: {
    path: "/api/Patients",
    contentKey: "firstName",
    expectedValue: "Playwright",
    updatedValue: "Playwright Updated",
    valid: () => ({
      firstName: "Playwright",
      lastName: `Patient${timestamp()}`,
      gender: 1,
      dateOfBirth: "1990-01-01T00:00:00.000Z",
      email: `pw.patient.${timestamp()}@test.local`,
      phoneNumber: "+385111111",
      address: "API Test Street 1"
    }),
    update: () => ({
      firstName: "Playwright Updated",
      lastName: `Patient${timestamp()}`,
      gender: 2,
      dateOfBirth: "1991-02-02T00:00:00.000Z",
      email: `pw.patient.updated.${timestamp()}@test.local`,
      phoneNumber: "+385222222",
      address: "Updated API Test Street 2"
    }),
    invalid: () => ({
      firstName: "",
      lastName: "",
      gender: 0,
      dateOfBirth: null
    })
  },
  Doctors: {
    path: "/api/Doctors",
    contentKey: "specialty",
    expectedValue: "Diagnostics",
    updatedValue: "Cardiology",
    valid: () => ({
      firstName: "Playwright",
      lastName: `Doctor${timestamp()}`,
      gender: 1,
      specialty: "Diagnostics",
      email: `pw.doctor.${timestamp()}@test.local`,
      phoneNumber: "+385333333",
      departmentIds: []
    }),
    update: () => ({
      firstName: "Playwright Updated",
      lastName: `Doctor${timestamp()}`,
      gender: 1,
      specialty: "Cardiology",
      email: `pw.doctor.updated.${timestamp()}@test.local`,
      phoneNumber: "+385444444",
      departmentIds: []
    }),
    invalid: () => ({
      firstName: "",
      lastName: "",
      gender: 0,
      specialty: "",
      departmentIds: []
    })
  },
  Departments: {
    path: "/api/Departments",
    contentKey: "name",
    expectedValue: "Playwright Department",
    updatedValue: "Playwright Department Updated",
    valid: () => ({
      name: `Playwright Department ${timestamp()}`,
      location: "API Wing",
      phoneNumber: "+385555555",
      headOfDepartment: "Dr. API Head",
      doctorIds: []
    }),
    update: () => ({
      name: `Playwright Department Updated ${timestamp()}`,
      location: "Updated API Wing",
      phoneNumber: "+385666666",
      headOfDepartment: "Dr. Updated Head",
      doctorIds: []
    }),
    invalid: () => ({
      name: "",
      location: "",
      doctorIds: []
    })
  },
  Appointments: {
    path: "/api/Appointments",
    contentKey: "room",
    expectedValue: "PW-101",
    updatedValue: "PW-202",
    dependencies: async () => ({
      patientId: await createEntity("Patients"),
      doctorId: await createEntity("Doctors")
    }),
    valid: ({ patientId, doctorId }) => ({
      patientId,
      doctorId,
      scheduledAt: isoFuture(1),
      status: 1,
      room: "PW-101",
      notes: "Playwright appointment"
    }),
    update: ({ patientId, doctorId }) => ({
      patientId,
      doctorId,
      scheduledAt: isoFuture(2),
      status: 2,
      room: "PW-202",
      notes: "Updated Playwright appointment"
    }),
    invalid: () => ({
      patientId: 0,
      doctorId: 0,
      scheduledAt: null,
      status: 0,
      room: "",
      notes: "Invalid appointment"
    })
  },
  MedicalRecords: {
    path: "/api/MedicalRecords",
    contentKey: "diagnosis",
    expectedValue: "Playwright diagnosis",
    updatedValue: "Updated Playwright diagnosis",
    dependencies: async () => ({
      patientId: await createEntity("Patients")
    }),
    valid: ({ patientId }) => ({
      patientId,
      createdAt: isoNow(),
      diagnosis: "Playwright diagnosis",
      notes: "Playwright medical record"
    }),
    update: ({ patientId }) => ({
      patientId,
      createdAt: isoNow(),
      diagnosis: "Updated Playwright diagnosis",
      notes: "Updated Playwright medical record"
    }),
    invalid: () => ({
      patientId: 0,
      createdAt: null,
      diagnosis: "",
      notes: "Invalid record"
    })
  },
  Prescriptions: {
    path: "/api/Prescriptions",
    contentKey: "issuedBy",
    expectedValue: "Dr. Playwright",
    updatedValue: "Dr. Playwright Updated",
    dependencies: async () => {
      const patientId = await createEntity("Patients");
      return {
        medicalRecordId: await createEntity("MedicalRecords", { patientId })
      };
    },
    valid: ({ medicalRecordId }) => ({
      medicalRecordId,
      issuedAt: isoNow(),
      issuedBy: "Dr. Playwright"
    }),
    update: ({ medicalRecordId }) => ({
      medicalRecordId,
      issuedAt: isoNow(),
      issuedBy: "Dr. Playwright Updated"
    }),
    invalid: () => ({
      medicalRecordId: 0,
      issuedAt: null,
      issuedBy: ""
    })
  },
  Medications: {
    path: "/api/Medications",
    contentKey: "name",
    expectedValue: "Playwright Medication",
    updatedValue: "Playwright Medication Updated",
    dependencies: async () => {
      const patientId = await createEntity("Patients");
      const medicalRecordId = await createEntity("MedicalRecords", { patientId });
      return {
        prescriptionId: await createEntity("Prescriptions", { medicalRecordId })
      };
    },
    valid: ({ prescriptionId }) => ({
      prescriptionId,
      name: "Playwright Medication",
      dosage: "100mg",
      instructions: "Use in API test"
    }),
    update: ({ prescriptionId }) => ({
      prescriptionId,
      name: "Playwright Medication Updated",
      dosage: "200mg",
      instructions: "Updated API instructions"
    }),
    invalid: () => ({
      prescriptionId: 0,
      name: "",
      dosage: "",
      instructions: ""
    })
  }
};

test.describe.configure({ mode: "serial" });

test.beforeAll(async ({ playwright, baseURL }) => {
  authRequest = await loginAsAdmin(playwright, baseURL);
});

test.afterAll(async () => {
  await cleanupCreatedEntities();
  await authRequest?.dispose();
});

for (const [name, config] of Object.entries(resources)) {
  test.describe(`${name} API`, () => {
    test(`GET ${config.path} returns JSON array and invalid paging returns 400`, async () => {
      const ok = await authRequest.get(config.path);
      expect(ok.status()).toBe(200);
      expect(ok.headers()["content-type"]).toContain("application/json");
      expect(await ok.json()).toEqual(expect.any(Array));

      const bad = await authRequest.get(`${config.path}?page=0&pageSize=20`);
      expect(bad.status()).toBe(400);
      expect(await bad.text()).toContain("page");
    });

    test(`GET ${config.path}/{id} returns JSON object and missing id returns 404`, async () => {
      const id = await createEntity(name);

      const ok = await authRequest.get(`${config.path}/${id}`);
      expect(ok.status()).toBe(200);
      expect(ok.headers()["content-type"]).toContain("application/json");
      const body = await ok.json();
      expect(body.id).toBe(id);
      expect(body).toHaveProperty(config.contentKey);

      const missing = await authRequest.get(`${config.path}/${MISSING_ID}`);
      expect(missing.status()).toBe(404);
      await expectNotFoundContent(missing);
    });

    test(`POST ${config.path} creates JSON object and invalid payload returns 400`, async () => {
      const deps = await resolveDependencies(name);
      const ok = await authRequest.post(config.path, { data: config.valid(deps) });
      expect(ok.status()).toBe(201);
      expect(ok.headers()["content-type"]).toContain("application/json");
      const body = await ok.json();
      trackCreated(name, body.id);
      expect(body.id).toEqual(expect.any(Number));
      expect(String(body[config.contentKey])).toContain(config.expectedValue);

      const bad = await authRequest.post(config.path, { data: config.invalid() });
      expect(bad.status()).toBe(400);
      expect(await bad.json()).toEqual(expect.any(Object));
    });

    test(`PUT ${config.path}/{id} updates resource and missing id returns 404`, async () => {
      const deps = await resolveDependencies(name);
      const id = await createEntity(name, deps);

      const ok = await authRequest.put(`${config.path}/${id}`, { data: config.update(deps) });
      expect(ok.status()).toBe(204);
      expect(await ok.text()).toBe("");

      const afterUpdate = await authRequest.get(`${config.path}/${id}`);
      expect(afterUpdate.status()).toBe(200);
      const body = await afterUpdate.json();
      expect(String(body[config.contentKey])).toContain(config.updatedValue);

      const missing = await authRequest.put(`${config.path}/${MISSING_ID}`, { data: config.update(deps) });
      expect(missing.status()).toBe(404);
      await expectNotFoundContent(missing);
    });

    test(`DELETE ${config.path}/{id} removes resource and missing id returns 404`, async () => {
      const id = await createEntity(name);

      const ok = await authRequest.delete(`${config.path}/${id}`);
      expect(ok.status()).toBe(204);
      expect(await ok.text()).toBe("");
      created[name].delete(id);

      const verifyGone = await authRequest.get(`${config.path}/${id}`);
      expect(verifyGone.status()).toBe(404);

      const missing = await authRequest.delete(`${config.path}/${MISSING_ID}`);
      expect(missing.status()).toBe(404);
      await expectNotFoundContent(missing);
    });
  });
}

test("10-step chained API scenario creates, reads, updates, deletes, and verifies missing data", async () => {
  const patientCreate = await authRequest.post(resources.Patients.path, {
    data: resources.Patients.valid()
  });
  expect(patientCreate.status()).toBe(201);
  const patient = await patientCreate.json();
  trackCreated("Patients", patient.id);

  const doctorCreate = await authRequest.post(resources.Doctors.path, {
    data: resources.Doctors.valid()
  });
  expect(doctorCreate.status()).toBe(201);
  const doctor = await doctorCreate.json();
  trackCreated("Doctors", doctor.id);

  const appointmentCreate = await authRequest.post(resources.Appointments.path, {
    data: resources.Appointments.valid({ patientId: patient.id, doctorId: doctor.id })
  });
  expect(appointmentCreate.status()).toBe(201);
  const appointment = await appointmentCreate.json();
  trackCreated("Appointments", appointment.id);

  const appointmentRead = await authRequest.get(`${resources.Appointments.path}/${appointment.id}`);
  expect(appointmentRead.status()).toBe(200);
  expect(await appointmentRead.json()).toMatchObject({
    id: appointment.id,
    patientId: patient.id,
    doctorId: doctor.id
  });

  const appointmentUpdate = await authRequest.put(`${resources.Appointments.path}/${appointment.id}`, {
    data: resources.Appointments.update({ patientId: patient.id, doctorId: doctor.id })
  });
  expect(appointmentUpdate.status()).toBe(204);

  const medicalRecordCreate = await authRequest.post(resources.MedicalRecords.path, {
    data: resources.MedicalRecords.valid({ patientId: patient.id })
  });
  expect(medicalRecordCreate.status()).toBe(201);
  const medicalRecord = await medicalRecordCreate.json();
  trackCreated("MedicalRecords", medicalRecord.id);

  const prescriptionCreate = await authRequest.post(resources.Prescriptions.path, {
    data: resources.Prescriptions.valid({ medicalRecordId: medicalRecord.id })
  });
  expect(prescriptionCreate.status()).toBe(201);
  const prescription = await prescriptionCreate.json();
  trackCreated("Prescriptions", prescription.id);

  const medicationCreate = await authRequest.post(resources.Medications.path, {
    data: resources.Medications.valid({ prescriptionId: prescription.id })
  });
  expect(medicationCreate.status()).toBe(201);
  const medication = await medicationCreate.json();
  trackCreated("Medications", medication.id);

  const patientDelete = await authRequest.delete(`${resources.Patients.path}/${patient.id}`);
  expect(patientDelete.status()).toBe(204);
  created.Patients.delete(patient.id);

  await expectGone(resources.Patients.path, patient.id);
  await expectGone(resources.Appointments.path, appointment.id);
  await expectGone(resources.MedicalRecords.path, medicalRecord.id);
  await expectGone(resources.Prescriptions.path, prescription.id);
  await expectGone(resources.Medications.path, medication.id);
});

async function loginAsAdmin(playwright, baseURL) {
  const context = await request.newContext({ baseURL });
  const loginPage = await context.get("/Account/Login");
  expect(loginPage.status()).toBe(200);
  const html = await loginPage.text();
  const token = extractAntiForgeryToken(html);

  const login = await context.post("/Account/Login", {
    form: {
      Email: ADMIN_EMAIL,
      Password: ADMIN_PASSWORD,
      RememberMe: "false",
      __RequestVerificationToken: token
    },
    maxRedirects: 0
  });

  expect([302, 303]).toContain(login.status());
  expect(login.headers()["set-cookie"] || "").toContain(".AspNetCore.Identity.Application");
  return context;
}

function extractAntiForgeryToken(html) {
  const match = html.match(/name="__RequestVerificationToken"[^>]*value="([^"]+)"/i);
  expect(match, "Login form should render an antiforgery token").not.toBeNull();
  return decodeHtml(match[1]);
}

function decodeHtml(value) {
  return value
    .replace(/&quot;/g, '"')
    .replace(/&#x2B;/g, "+")
    .replace(/&#x2F;/g, "/")
    .replace(/&amp;/g, "&");
}

async function resolveDependencies(name) {
  return resources[name].dependencies ? resources[name].dependencies() : {};
}

async function createEntity(name, dependencyOverrides = undefined) {
  const config = resources[name];
  const deps = dependencyOverrides ?? await resolveDependencies(name);
  const response = await authRequest.post(config.path, { data: config.valid(deps) });
  expect(response.status()).toBe(201);
  const body = await response.json();
  trackCreated(name, body.id);
  return body.id;
}

function trackCreated(name, id) {
  if (id) {
    created[name].add(id);
  }
}

async function cleanupCreatedEntities() {
  for (const [name, ids] of Object.entries(created)) {
    const config = resources[name];
    for (const id of Array.from(ids).reverse()) {
      await authRequest?.delete(`${config.path}/${id}`).catch(() => {});
    }
    ids.clear();
  }
}

async function expectGone(path, id) {
  const response = await authRequest.get(`${path}/${id}`);
  expect(response.status()).toBe(404);
  await expectNotFoundContent(response);
}

async function expectNotFoundContent(response) {
  const text = await response.text();
  if (!text) {
    return;
  }

  const body = JSON.parse(text);
  expect(body).toMatchObject({
    status: 404,
    title: expect.stringContaining("Not Found")
  });
}
