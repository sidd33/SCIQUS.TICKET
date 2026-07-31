const { test, expect } = require('@playwright/test');

test.describe('Employees Controller API Tests', () => {
  let createdGradeId = '';
  let createdDeptId = '';
  let newUserId = '';
  let newUserToken = '';
  let employeeId = '';
  const testEmail = `emp_${Date.now()}@test.com`;
  const testPassword = 'Password@123';
  const nonExistentUserId = '00000000-0000-0000-0000-000000000000';

  test.beforeAll(async ({ request }) => {
    // 1. Create a Grade
    const gradeRes = await request.post('/api/grades', {
      data: { gradeLevel: 5, description: 'Mid Level' }
    });
    expect(gradeRes.status()).toBe(201);
    const grade = await gradeRes.json();
    createdGradeId = grade.id;

    // 2. Create a Department
    const deptRes = await request.post('/api/departments', {
      data: { name: 'Engineering' }
    });
    expect(deptRes.status()).toBe(201);
    const dept = await deptRes.json();
    createdDeptId = dept.departmentId;

    // 3. Register a new user
    const regRes = await request.post('/api/auth/register', {
      data: {
        fullName: 'Employee User',
        email: testEmail,
        password: testPassword,
        confirmPassword: testPassword,
        phoneNumber: '9876543210',
        role: 'Employee'
      }
    });
    expect(regRes.status()).toBe(200);

    // 4. Login as new user to get token and retrieve User ID via /me
    const loginRes = await request.post('/api/auth/login', {
      data: { email: testEmail, password: testPassword }
    });
    expect(loginRes.status()).toBe(200);
    const loginBody = await loginRes.json();
    newUserToken = loginBody.accessToken;

    const meRes = await request.get('/api/auth/me', {
      headers: { 'Authorization': `Bearer ${newUserToken}` }
    });
    expect(meRes.status()).toBe(200);
    const meBody = await meRes.json();
    newUserId = meBody.id;
  });

  // ── Positive Tests ─────────────────────────────────────────────────────────

  test('Should create an Employee for the registered user', async ({ request }) => {
    const response = await request.post(`/api/employees/${newUserId}`, {
      data: {
        name: 'Employee User',
        registeredMobileNumber: '9876543210',
        email: testEmail,
        employeeId: 'EMP-T1001',
        designation: 'Software Engineer',
        departmentId: createdDeptId,
        gradeId: createdGradeId,
        profileImageUrl: 'http://test.com/img.png'
      }
    });

    expect(response.status()).toBe(201);
    const body = await response.json();
    expect(body.id).toBe(newUserId);
    expect(body.name).toBe('Employee User');
    employeeId = body.id;
  });

  test('Should get employee by ID', async ({ request }) => {
    const response = await request.get(`/api/employees/${employeeId}`);
    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body.id).toBe(employeeId);
    expect(body.designation).toBe('Software Engineer');
  });

  test('Should update employee details', async ({ request }) => {
    const response = await request.put(`/api/employees/${employeeId}`, {
      data: {
        name: 'Employee User Updated',
        designation: 'Senior Software Engineer'
      }
    });

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body.name).toBe('Employee User Updated');
    expect(body.designation).toBe('Senior Software Engineer');
  });

  test('Should get all employees', async ({ request }) => {
    const response = await request.get('/api/employees');
    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body.items).toBeDefined();
    const list = body.items;
    expect(Array.isArray(list)).toBe(true);
    const found = list.find(e => e.id === employeeId);
    expect(found).toBeDefined();
  });

  test('Should soft delete employee', async ({ request }) => {
    const response = await request.delete(`/api/employees/${employeeId}`);
    expect(response.status()).toBe(204);
  });

  // ── Negative Tests ─────────────────────────────────────────────────────────

  test('Should return 404 for non-existent employee ID', async ({ request }) => {
    const response = await request.get(`/api/employees/${nonExistentUserId}`);
    expect(response.status()).toBe(404);
  });

  test('Should return 404 when updating non-existent employee ID', async ({ request }) => {
    const response = await request.put(`/api/employees/${nonExistentUserId}`, {
      data: {
        name: 'Non Existent',
        designation: 'Staff'
      }
    });
    expect(response.status()).toBe(404);
  });

  test('Should return 400 when creating employee with mismatching/non-existent user ID', async ({ request }) => {
    // In EmployeeService.cs: CreateAsync checks if ApplicationUser exists
    const response = await request.post(`/api/employees/${nonExistentUserId}`, {
      data: {
        name: 'No User',
        email: 'nouser@test.com',
        departmentId: createdDeptId
      }
    });
    expect(response.status()).toBe(400); // Expecting validation error since user doesn't exist
  });
});
