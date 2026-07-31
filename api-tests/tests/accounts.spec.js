const { test, expect } = require('@playwright/test');

test.describe('Accounts, Contacts, and Addresses API Tests', () => {
  let createdGradeId = '';
  let createdDeptId = '';
  let employeeId = '';
  let testEmail = '';
  let accountId = '';
  let contactId = '';
  let addressId = '';
  const nonExistentId = '00000000-0000-0000-0000-000000000000';

  test.beforeAll(async ({ request }) => {
    // 1. Create a Grade
    const gradeRes = await request.post('/api/grades', {
      data: { gradeLevel: 8, description: 'Manager Level' }
    });
    expect(gradeRes.status()).toBe(201);
    const grade = await gradeRes.json();
    createdGradeId = grade.id;

    // 2. Create a Department
    const deptRes = await request.post('/api/departments', {
      data: { name: 'Customer Success' }
    });
    expect(deptRes.status()).toBe(201);
    const dept = await deptRes.json();
    createdDeptId = dept.departmentId;

    // 3. Register a user
    testEmail = `user_acc_${Date.now()}@test.com`;
    const regRes = await request.post('/api/auth/register', {
      data: {
        fullName: 'Account Owner',
        email: testEmail,
        password: 'Password@123',
        confirmPassword: 'Password@123',
        phoneNumber: '5551234567',
        role: 'Employee'
      }
    });
    expect(regRes.status()).toBe(200);

    // Login and retrieve userId
    const loginRes = await request.post('/api/auth/login', {
      data: { email: testEmail, password: 'Password@123' }
    });
    expect(loginRes.status()).toBe(200);
    const loginBody = await loginRes.json();
    
    const meRes = await request.get('/api/auth/me', {
      headers: { 'Authorization': `Bearer ${loginBody.accessToken}` }
    });
    expect(meRes.status()).toBe(200);
    const meBody = await meRes.json();
    const userId = meBody.id;

    // 4. Create Employee record
    const empRes = await request.post(`/api/employees/${userId}`, {
      data: {
        name: 'Account Owner',
        registeredMobileNumber: '5551234567',
        email: testEmail,
        employeeId: `EMP-A-${Date.now()}`,
        designation: 'Account Manager',
        departmentId: createdDeptId,
        gradeId: createdGradeId
      }
    });
    expect(empRes.status()).toBe(201);
    const empBody = await empRes.json();
    employeeId = empBody.id;
  });

  // ── Positive Tests ─────────────────────────────────────────────────────────

  test('Should create an Account successfully', async ({ request }) => {
    const response = await request.post('/api/accounts', {
      data: {
        accountName: 'Acme Corp',
        registeredMobileNumber: '9998887777',
        email: 'billing@acme.com',
        website: 'https://acme.com',
        createdByUserId: employeeId,
        accountManagerId: employeeId
      }
    });

    expect(response.status()).toBe(201);
    const body = await response.json();
    expect(body.accountId).toBeDefined();
    expect(body.accountName).toBe('Acme Corp');
    accountId = body.accountId;
  });

  test('Should update Account details', async ({ request }) => {
    const response = await request.put(`/api/accounts/${accountId}`, {
      data: {
        accountName: 'Acme Corporation',
        registeredMobileNumber: '9998887777',
        email: 'support@acme.com'
      }
    });

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body.accountName).toBe('Acme Corporation');
    expect(body.email).toBe('support@acme.com');
  });

  test('Should add a Contact to the Account', async ({ request }) => {
    const response = await request.post(`/api/accounts/${accountId}/contacts`, {
      data: {
        personName: 'John Doe',
        email: 'johndoe@acme.com',
        mobileNumber: '1112223333',
        designation: 'IT Director',
        primaryContact: false
      }
    });

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body.accountContactsId).toBeDefined();
    expect(body.personName).toBe('John Doe');
    contactId = body.accountContactsId;
  });

  test('Should set Contact as primary contact', async ({ request }) => {
    const response = await request.put(`/api/accounts/${accountId}/contacts/${contactId}/set-primary`);
    expect(response.status()).toBe(200);
  });

  test('Should add an Address to the Account', async ({ request }) => {
    const response = await request.post(`/api/accounts/${accountId}/addresses`, {
      data: {
        country: 'USA',
        city: 'New York',
        state: 'NY',
        pincode: '10001',
        addressLine: '123 Broadway Ave',
        primaryAddress: false
      }
    });

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body.accountAddressId).toBeDefined();
    expect(body.city).toBe('New York');
    addressId = body.accountAddressId;
  });

  test('Should set Address as primary address', async ({ request }) => {
    const response = await request.put(`/api/accounts/${accountId}/addresses/${addressId}/set-primary`);
    expect(response.status()).toBe(200);
  });

  test('Should delete Contact', async ({ request }) => {
    const response = await request.delete(`/api/accounts/${accountId}/contacts/${contactId}`);
    expect(response.status()).toBe(200);
  });

  test('Should delete Address', async ({ request }) => {
    const response = await request.delete(`/api/accounts/${accountId}/addresses/${addressId}`);
    expect(response.status()).toBe(200);
  });

  test('Should soft delete Account', async ({ request }) => {
    const response = await request.delete(`/api/accounts/${accountId}`);
    expect(response.status()).toBe(200);
  });

  // ── Negative Tests ─────────────────────────────────────────────────────────

  test('Should return 404 for non-existent Account ID', async ({ request }) => {
    const response = await request.get(`/api/accounts/${nonExistentId}`);
    expect(response.status()).toBe(404);
  });

  test('Should return 400 when setting Account as its own parent', async ({ request }) => {
    // Re-create account to test this
    const res = await request.post('/api/accounts', {
      data: {
        accountName: 'Self Parent Test',
        registeredMobileNumber: '9998887777',
        email: 'test@self.com',
        createdByUserId: employeeId,
        accountManagerId: employeeId
      }
    });
    expect(res.status()).toBe(201);
    const acc = await res.json();
    const tempAccId = acc.accountId;

    // Put own ID as parentAccountId
    const putRes = await request.put(`/api/accounts/${tempAccId}`, {
      data: {
        accountName: 'Self Parent Test',
        registeredMobileNumber: '9998887777',
        email: 'test@self.com',
        parentAccountId: tempAccId
      }
    });
    expect(putRes.status()).toBe(400); // Throws InvalidOperationException which maps to 400 Bad Request

    // Clean up
    await request.delete(`/api/accounts/${tempAccId}`);
  });

  test('Should return 404 when deleting non-existent contact', async ({ request }) => {
    const response = await request.delete(`/api/accounts/${accountId}/contacts/${nonExistentId}`);
    expect(response.status()).toBe(404);
  });

  test('Should return 404 when deleting non-existent address', async ({ request }) => {
    const response = await request.delete(`/api/accounts/${accountId}/addresses/${nonExistentId}`);
    expect(response.status()).toBe(404);
  });
});
