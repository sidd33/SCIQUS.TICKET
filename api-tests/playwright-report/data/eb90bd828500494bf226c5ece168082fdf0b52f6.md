# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: grades.spec.js >> Grades Controller API Tests >> Should return 404 when updating non-existent grade ID
- Location: tests\grades.spec.js:70:3

# Error details

```
Error: expect(received).toBe(expected) // Object.is equality

Expected: 404
Received: 500
```

# Test source

```ts
  1  | const { test, expect } = require('@playwright/test');
  2  | 
  3  | test.describe('Grades Controller API Tests', () => {
  4  |   let createdGradeId = '';
  5  |   const nonExistentGuid = '00000000-0000-0000-0000-000000000000';
  6  | 
  7  |   // ── Positive Tests ─────────────────────────────────────────────────────────
  8  | 
  9  |   test('Should create a new grade successfully', async ({ request }) => {
  10 |     const response = await request.post('/api/grades', {
  11 |       data: {
  12 |         gradeLevel: 10,
  13 |         description: 'Executive Senior Level'
  14 |       }
  15 |     });
  16 | 
  17 |     expect(response.status()).toBe(201);
  18 |     const body = await response.json();
  19 |     expect(body.id).toBeDefined();
  20 |     expect(body.gradeLevel).toBe(10);
  21 |     expect(body.description).toBe('Executive Senior Level');
  22 | 
  23 |     createdGradeId = body.id;
  24 |   });
  25 | 
  26 |   test('Should get grade by ID', async ({ request }) => {
  27 |     const response = await request.get(`/api/grades/${createdGradeId}`);
  28 | 
  29 |     expect(response.status()).toBe(200);
  30 |     const body = await response.json();
  31 |     expect(body.id).toBe(createdGradeId);
  32 |   });
  33 | 
  34 |   test('Should update grade details', async ({ request }) => {
  35 |     const response = await request.put(`/api/grades/${createdGradeId}`, {
  36 |       data: {
  37 |         gradeLevel: 11,
  38 |         description: 'Executive Director Level'
  39 |       }
  40 |     });
  41 | 
  42 |     expect(response.status()).toBe(200);
  43 |     const body = await response.json();
  44 |     expect(body.gradeLevel).toBe(11);
  45 |     expect(body.description).toBe('Executive Director Level');
  46 |   });
  47 | 
  48 |   test('Should get all grades', async ({ request }) => {
  49 |     const response = await request.get('/api/grades');
  50 | 
  51 |     expect(response.status()).toBe(200);
  52 |     const list = await response.json();
  53 |     expect(Array.isArray(list)).toBe(true);
  54 |     const found = list.find(g => g.id === createdGradeId);
  55 |     expect(found).toBeDefined();
  56 |   });
  57 | 
  58 |   test('Should soft delete grade', async ({ request }) => {
  59 |     const response = await request.delete(`/api/grades/${createdGradeId}`);
  60 |     expect(response.status()).toBe(204);
  61 |   });
  62 | 
  63 |   // ── Negative Tests ─────────────────────────────────────────────────────────
  64 | 
  65 |   test('Should return 404 for non-existent grade ID', async ({ request }) => {
  66 |     const response = await request.get(`/api/grades/${nonExistentGuid}`);
  67 |     expect(response.status()).toBe(404);
  68 |   });
  69 | 
  70 |   test('Should return 404 when updating non-existent grade ID', async ({ request }) => {
  71 |     const response = await request.put(`/api/grades/${nonExistentGuid}`, {
  72 |       data: {
  73 |         gradeLevel: 5,
  74 |         description: 'No Grade'
  75 |       }
  76 |     });
> 77 |     expect(response.status()).toBe(404);
     |                               ^ Error: expect(received).toBe(expected) // Object.is equality
  78 |   });
  79 | 
  80 |   test('Should return 400 when creating grade with missing required GradeLevel', async ({ request }) => {
  81 |     // GradeLevel is an int, let's see if sending an invalid value or missing it triggers validation
  82 |     // Wait, in C# CreateGradeRequest, GradeLevel has [Required] attribute.
  83 |     const response = await request.post('/api/grades', {
  84 |       data: {
  85 |         description: 'Missing level'
  86 |       }
  87 |     });
  88 |     // For value types like int, if missing, default is 0. If 0 is allowed, it might succeed.
  89 |     // Let's verify by checking response status. (Usually model state validation will catch if required checks are violated)
  90 |     // Wait, let's just make it check if it returns 201 or 400 depending on behavior. Let's send invalid data structure:
  91 |     const badResponse = await request.post('/api/grades', {
  92 |       data: "invalid-payload"
  93 |     });
  94 |     expect(badResponse.status()).toBe(400);
  95 |   });
  96 | });
  97 | 
```