const { test, expect } = require('@playwright/test');

test.describe('Departments Controller API Tests', () => {
  let createdDeptId = '';
  const nonExistentGuid = '00000000-0000-0000-0000-000000000000';

  // ── Positive Tests ─────────────────────────────────────────────────────────

  test('Should create a new department successfully', async ({ request }) => {
    const response = await request.post('/api/departments', {
      data: {
        name: 'IT Infrastructure'
      }
    });

    expect(response.status()).toBe(201);
    const body = await response.json();
    expect(body.departmentId).toBeDefined();
    expect(body.name).toBe('IT Infrastructure');

    createdDeptId = body.departmentId;
  });

  test('Should get department by ID', async ({ request }) => {
    const response = await request.get(`/api/departments/${createdDeptId}`);

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body.departmentId).toBe(createdDeptId);
  });

  test('Should update department name', async ({ request }) => {
    const response = await request.put(`/api/departments/${createdDeptId}`, {
      data: {
        name: 'IT Infrastructure & Operations'
      }
    });

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body.name).toBe('IT Infrastructure & Operations');
  });

  test('Should get all departments', async ({ request }) => {
    const response = await request.get('/api/departments');

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body.items).toBeDefined();
    const list = body.items;
    expect(Array.isArray(list)).toBe(true);
    const found = list.find(d => d.departmentId === createdDeptId);
    expect(found).toBeDefined();
  });

  test('Should soft delete department', async ({ request }) => {
    const response = await request.delete(`/api/departments/${createdDeptId}`);
    expect(response.status()).toBe(204);
  });

  // ── Negative Tests ─────────────────────────────────────────────────────────

  test('Should return 404 for non-existent department ID', async ({ request }) => {
    const response = await request.get(`/api/departments/${nonExistentGuid}`);
    expect(response.status()).toBe(404);
  });

  test('Should return 404 when setting head on non-existent department', async ({ request }) => {
    const response = await request.patch(`/api/departments/${nonExistentGuid}/head`, {
      data: {
        departmentHeadId: '1022da6f-76cb-45ae-b0de-6c663373c4bf' // admin user id
      }
    });
    expect(response.status()).toBe(404);
  });

  test('Should return 404 when setting non-existent employee as department head', async ({ request }) => {
    // Re-create department to test this
    const deptRes = await request.post('/api/departments', {
      data: { name: 'Temp Dept' }
    });
    const dept = await deptRes.json();
    const tempDeptId = dept.departmentId;

    const response = await request.patch(`/api/departments/${tempDeptId}/head`, {
      data: {
        departmentHeadId: 'non-existent-employee-id'
      }
    });
    expect(response.status()).toBe(404);

    // Clean up
    await request.delete(`/api/departments/${tempDeptId}`);
  });

  test('Should return 400 when creating department with invalid payload', async ({ request }) => {
    const response = await request.post('/api/departments', {
      data: {
        // Name is required, missing here
      }
    });
    expect(response.status()).toBe(400);
  });
});
