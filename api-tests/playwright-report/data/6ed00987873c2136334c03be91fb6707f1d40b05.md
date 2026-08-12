# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: auth.spec.js >> Auth Controller API Tests >> Should fail login with incorrect password
- Location: tests\auth.spec.js:98:3

# Error details

```
Error: expect(received).toBe(expected) // Object.is equality

Expected: 400
Received: 500
```

# Test source

```ts
  6   |   const testEmail = `user_${Date.now()}@test.com`;
  7   |   const testPassword = 'Password@123';
  8   | 
  9   |   // ── Positive Tests ─────────────────────────────────────────────────────────
  10  | 
  11  |   test('Should login admin successfully and return JWT', async ({ request }) => {
  12  |     const response = await request.post('/api/auth/login', {
  13  |       data: {
  14  |         email: 'admin@sciqustickets.com',
  15  |         password: 'Admin@123'
  16  |       }
  17  |     });
  18  | 
  19  |     expect(response.status()).toBe(200);
  20  |     const body = await response.json();
  21  |     expect(body.accessToken).toBeDefined();
  22  |     expect(body.refreshToken).toBeDefined();
  23  | 
  24  |     accessToken = body.accessToken;
  25  |     refreshToken = body.refreshToken;
  26  |   });
  27  | 
  28  |   test('Should get currently logged in user info (me)', async ({ request }) => {
  29  |     const response = await request.get('/api/auth/me', {
  30  |       headers: {
  31  |         'Authorization': `Bearer ${accessToken}`
  32  |       }
  33  |     });
  34  | 
  35  |     expect(response.status()).toBe(200);
  36  |     const body = await response.json();
  37  |     expect(body.email).toBe('admin@sciqustickets.com');
  38  |   });
  39  | 
  40  |   test('Should register a new user successfully', async ({ request }) => {
  41  |     const response = await request.post('/api/auth/register', {
  42  |       data: {
  43  |         fullName: 'Test User',
  44  |         email: testEmail,
  45  |         password: testPassword,
  46  |         confirmPassword: testPassword,
  47  |         phoneNumber: '1234567890',
  48  |         role: 'Employee'
  49  |       }
  50  |     });
  51  | 
  52  |     expect(response.status()).toBe(200);
  53  |     const body = await response.json();
  54  |     expect(body).toBeDefined();
  55  |   });
  56  | 
  57  |   test('Should login newly registered user successfully', async ({ request }) => {
  58  |     const response = await request.post('/api/auth/login', {
  59  |       data: {
  60  |         email: testEmail,
  61  |         password: testPassword
  62  |       }
  63  |     });
  64  | 
  65  |     expect(response.status()).toBe(200);
  66  |     const body = await response.json();
  67  |     expect(body.accessToken).toBeDefined();
  68  |   });
  69  | 
  70  |   test('Should refresh token using refresh token', async ({ request }) => {
  71  |     const response = await request.post('/api/auth/refresh', {
  72  |       data: {
  73  |         accessToken: accessToken,
  74  |         refreshToken: refreshToken
  75  |       }
  76  |     });
  77  | 
  78  |     expect(response.status()).toBe(200);
  79  |     const body = await response.json();
  80  |     expect(body.accessToken).toBeDefined();
  81  |   });
  82  | 
  83  |   test('Should revoke refresh token successfully', async ({ request }) => {
  84  |     const response = await request.post('/api/auth/revoke', {
  85  |       headers: {
  86  |         'Authorization': `Bearer ${accessToken}`
  87  |       },
  88  |       data: {
  89  |         refreshToken: refreshToken
  90  |       }
  91  |     });
  92  | 
  93  |     expect(response.status() === 204 || response.status() === 200).toBeTruthy();
  94  |   });
  95  | 
  96  |   // ── Negative & Boundary Tests ──────────────────────────────────────────────
  97  | 
  98  |   test('Should fail login with incorrect password', async ({ request }) => {
  99  |     const response = await request.post('/api/auth/login', {
  100 |       data: {
  101 |         email: 'admin@sciqustickets.com',
  102 |         password: 'WrongPassword'
  103 |       }
  104 |     });
  105 | 
> 106 |     expect(response.status()).toBe(400); // Usually 400 Bad Request or 401 Unauthorized depending on API response
      |                               ^ Error: expect(received).toBe(expected) // Object.is equality
  107 |   });
  108 | 
  109 |   test('Should fail registration with mismatching passwords', async ({ request }) => {
  110 |     const response = await request.post('/api/auth/register', {
  111 |       data: {
  112 |         fullName: 'Fail User',
  113 |         email: `fail_${Date.now()}@test.com`,
  114 |         password: 'Password@123',
  115 |         confirmPassword: 'DifferentPassword@123',
  116 |         phoneNumber: '1234567890',
  117 |         role: 'Employee'
  118 |       }
  119 |     });
  120 | 
  121 |     expect(response.status()).toBe(400);
  122 |     const body = await response.json();
  123 |     expect(JSON.stringify(body)).toContain('match');
  124 |   });
  125 | 
  126 |   test('Should fail registration with invalid email format', async ({ request }) => {
  127 |     const response = await request.post('/api/auth/register', {
  128 |       data: {
  129 |         fullName: 'Fail User',
  130 |         email: 'invalid-email-format',
  131 |         password: 'Password@123',
  132 |         confirmPassword: 'Password@123',
  133 |         phoneNumber: '1234567890',
  134 |         role: 'Employee'
  135 |       }
  136 |     });
  137 | 
  138 |     expect(response.status()).toBe(400);
  139 |   });
  140 | 
  141 |   test('Should deny access to me endpoint without authorization token', async ({ request }) => {
  142 |     const response = await request.get('/api/auth/me');
  143 |     expect(response.status()).toBe(401);
  144 |   });
  145 | });
  146 | 
```