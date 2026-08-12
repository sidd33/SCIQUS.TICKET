# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: departments.spec.js >> Departments Controller API Tests >> Should return 404 when setting non-existent employee as department head
- Location: tests\departments.spec.js:77:3

# Error details

```
Error: expect(received).toBe(expected) // Object.is equality

Expected: 404
Received: 500
```

# Test source

```ts
  1   | const { test, expect } = require('@playwright/test');
  2   | 
  3   | test.describe('Departments Controller API Tests', () => {
  4   |   let createdDeptId = '';
  5   |   const nonExistentGuid = '00000000-0000-0000-0000-000000000000';
  6   | 
  7   |   // ── Positive Tests ─────────────────────────────────────────────────────────
  8   | 
  9   |   test('Should create a new department successfully', async ({ request }) => {
  10  |     const response = await request.post('/api/departments', {
  11  |       data: {
  12  |         name: 'IT Infrastructure'
  13  |       }
  14  |     });
  15  | 
  16  |     expect(response.status()).toBe(201);
  17  |     const body = await response.json();
  18  |     expect(body.departmentId).toBeDefined();
  19  |     expect(body.name).toBe('IT Infrastructure');
  20  | 
  21  |     createdDeptId = body.departmentId;
  22  |   });
  23  | 
  24  |   test('Should get department by ID', async ({ request }) => {
  25  |     const response = await request.get(`/api/departments/${createdDeptId}`);
  26  | 
  27  |     expect(response.status()).toBe(200);
  28  |     const body = await response.json();
  29  |     expect(body.departmentId).toBe(createdDeptId);
  30  |   });
  31  | 
  32  |   test('Should update department name', async ({ request }) => {
  33  |     const response = await request.put(`/api/departments/${createdDeptId}`, {
  34  |       data: {
  35  |         name: 'IT Infrastructure & Operations'
  36  |       }
  37  |     });
  38  | 
  39  |     expect(response.status()).toBe(200);
  40  |     const body = await response.json();
  41  |     expect(body.name).toBe('IT Infrastructure & Operations');
  42  |   });
  43  | 
  44  |   test('Should get all departments', async ({ request }) => {
  45  |     const response = await request.get('/api/departments');
  46  | 
  47  |     expect(response.status()).toBe(200);
  48  |     const body = await response.json();
  49  |     expect(body.items).toBeDefined();
  50  |     const list = body.items;
  51  |     expect(Array.isArray(list)).toBe(true);
  52  |     const found = list.find(d => d.departmentId === createdDeptId);
  53  |     expect(found).toBeDefined();
  54  |   });
  55  | 
  56  |   test('Should soft delete department', async ({ request }) => {
  57  |     const response = await request.delete(`/api/departments/${createdDeptId}`);
  58  |     expect(response.status()).toBe(204);
  59  |   });
  60  | 
  61  |   // ── Negative Tests ─────────────────────────────────────────────────────────
  62  | 
  63  |   test('Should return 404 for non-existent department ID', async ({ request }) => {
  64  |     const response = await request.get(`/api/departments/${nonExistentGuid}`);
  65  |     expect(response.status()).toBe(404);
  66  |   });
  67  | 
  68  |   test('Should return 404 when setting head on non-existent department', async ({ request }) => {
  69  |     const response = await request.patch(`/api/departments/${nonExistentGuid}/head`, {
  70  |       data: {
  71  |         departmentHeadId: '1022da6f-76cb-45ae-b0de-6c663373c4bf' // admin user id
  72  |       }
  73  |     });
  74  |     expect(response.status()).toBe(404);
  75  |   });
  76  | 
  77  |   test('Should return 404 when setting non-existent employee as department head', async ({ request }) => {
  78  |     // Re-create department to test this
  79  |     const deptRes = await request.post('/api/departments', {
  80  |       data: { name: 'Temp Dept' }
  81  |     });
  82  |     const dept = await deptRes.json();
  83  |     const tempDeptId = dept.departmentId;
  84  | 
  85  |     const response = await request.patch(`/api/departments/${tempDeptId}/head`, {
  86  |       data: {
  87  |         departmentHeadId: 'non-existent-employee-id'
  88  |       }
  89  |     });
> 90  |     expect(response.status()).toBe(404);
      |                               ^ Error: expect(received).toBe(expected) // Object.is equality
  91  | 
  92  |     // Clean up
  93  |     await request.delete(`/api/departments/${tempDeptId}`);
  94  |   });
  95  | 
  96  |   test('Should return 400 when creating department with invalid payload', async ({ request }) => {
  97  |     const response = await request.post('/api/departments', {
  98  |       data: {
  99  |         // Name is required, missing here
  100 |       }
  101 |     });
  102 |     expect(response.status()).toBe(400);
  103 |   });
  104 | });
  105 | 
```