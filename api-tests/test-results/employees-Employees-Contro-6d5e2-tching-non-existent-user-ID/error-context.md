# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: employees.spec.js >> Employees Controller API Tests >> Should return 400 when creating employee with mismatching/non-existent user ID
- Location: tests\employees.spec.js:137:3

# Error details

```
Error: expect(received).toBe(expected) // Object.is equality

Expected: 400
Received: 201
```

# Test source

```ts
  46  |     });
  47  |     expect(loginRes.status()).toBe(200);
  48  |     const loginBody = await loginRes.json();
  49  |     newUserToken = loginBody.accessToken;
  50  | 
  51  |     const meRes = await request.get('/api/auth/me', {
  52  |       headers: { 'Authorization': `Bearer ${newUserToken}` }
  53  |     });
  54  |     expect(meRes.status()).toBe(200);
  55  |     const meBody = await meRes.json();
  56  |     newUserId = meBody.id;
  57  |   });
  58  | 
  59  |   // ── Positive Tests ─────────────────────────────────────────────────────────
  60  | 
  61  |   test('Should create an Employee for the registered user', async ({ request }) => {
  62  |     const response = await request.post(`/api/employees/${newUserId}`, {
  63  |       data: {
  64  |         name: 'Employee User',
  65  |         registeredMobileNumber: '9876543210',
  66  |         email: testEmail,
  67  |         employeeId: 'EMP-T1001',
  68  |         designation: 'Software Engineer',
  69  |         departmentId: createdDeptId,
  70  |         gradeId: createdGradeId,
  71  |         profileImageUrl: 'http://test.com/img.png'
  72  |       }
  73  |     });
  74  | 
  75  |     expect(response.status()).toBe(201);
  76  |     const body = await response.json();
  77  |     expect(body.id).toBe(newUserId);
  78  |     expect(body.name).toBe('Employee User');
  79  |     employeeId = body.id;
  80  |   });
  81  | 
  82  |   test('Should get employee by ID', async ({ request }) => {
  83  |     const response = await request.get(`/api/employees/${employeeId}`);
  84  |     expect(response.status()).toBe(200);
  85  |     const body = await response.json();
  86  |     expect(body.id).toBe(employeeId);
  87  |     expect(body.designation).toBe('Software Engineer');
  88  |   });
  89  | 
  90  |   test('Should update employee details', async ({ request }) => {
  91  |     const response = await request.put(`/api/employees/${employeeId}`, {
  92  |       data: {
  93  |         name: 'Employee User Updated',
  94  |         designation: 'Senior Software Engineer'
  95  |       }
  96  |     });
  97  | 
  98  |     expect(response.status()).toBe(200);
  99  |     const body = await response.json();
  100 |     expect(body.name).toBe('Employee User Updated');
  101 |     expect(body.designation).toBe('Senior Software Engineer');
  102 |   });
  103 | 
  104 |   test('Should get all employees', async ({ request }) => {
  105 |     const response = await request.get('/api/employees');
  106 |     expect(response.status()).toBe(200);
  107 |     const body = await response.json();
  108 |     expect(body.items).toBeDefined();
  109 |     const list = body.items;
  110 |     expect(Array.isArray(list)).toBe(true);
  111 |     const found = list.find(e => e.id === employeeId);
  112 |     expect(found).toBeDefined();
  113 |   });
  114 | 
  115 |   test('Should soft delete employee', async ({ request }) => {
  116 |     const response = await request.delete(`/api/employees/${employeeId}`);
  117 |     expect(response.status()).toBe(204);
  118 |   });
  119 | 
  120 |   // ── Negative Tests ─────────────────────────────────────────────────────────
  121 | 
  122 |   test('Should return 404 for non-existent employee ID', async ({ request }) => {
  123 |     const response = await request.get(`/api/employees/${nonExistentUserId}`);
  124 |     expect(response.status()).toBe(404);
  125 |   });
  126 | 
  127 |   test('Should return 404 when updating non-existent employee ID', async ({ request }) => {
  128 |     const response = await request.put(`/api/employees/${nonExistentUserId}`, {
  129 |       data: {
  130 |         name: 'Non Existent',
  131 |         designation: 'Staff'
  132 |       }
  133 |     });
  134 |     expect(response.status()).toBe(404);
  135 |   });
  136 | 
  137 |   test('Should return 400 when creating employee with mismatching/non-existent user ID', async ({ request }) => {
  138 |     // In EmployeeService.cs: CreateAsync checks if ApplicationUser exists
  139 |     const response = await request.post(`/api/employees/${nonExistentUserId}`, {
  140 |       data: {
  141 |         name: 'No User',
  142 |         email: 'nouser@test.com',
  143 |         departmentId: createdDeptId
  144 |       }
  145 |     });
> 146 |     expect(response.status()).toBe(400); // Expecting validation error since user doesn't exist
      |                               ^ Error: expect(received).toBe(expected) // Object.is equality
  147 |   });
  148 | });
  149 | 
```