const { test, expect } = require('@playwright/test');

test.describe('Auth Controller API Tests', () => {
  let accessToken = '';
  let refreshToken = '';
  const testEmail = `user_${Date.now()}@test.com`;
  const testPassword = 'Password@123';

  // ── Positive Tests ─────────────────────────────────────────────────────────

  test('Should login admin successfully and return JWT', async ({ request }) => {
    const response = await request.post('/api/auth/login', {
      data: {
        email: 'admin@sciqustickets.com',
        password: 'Admin@123'
      }
    });

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body.accessToken).toBeDefined();
    expect(body.refreshToken).toBeDefined();

    accessToken = body.accessToken;
    refreshToken = body.refreshToken;
  });

  test('Should get currently logged in user info (me)', async ({ request }) => {
    const response = await request.get('/api/auth/me', {
      headers: {
        'Authorization': `Bearer ${accessToken}`
      }
    });

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body.email).toBe('admin@sciqustickets.com');
  });

  test('Should register a new user successfully', async ({ request }) => {
    const response = await request.post('/api/auth/register', {
      data: {
        fullName: 'Test User',
        email: testEmail,
        password: testPassword,
        confirmPassword: testPassword,
        phoneNumber: '1234567890',
        role: 'Employee'
      }
    });

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body).toBeDefined();
  });

  test('Should login newly registered user successfully', async ({ request }) => {
    const response = await request.post('/api/auth/login', {
      data: {
        email: testEmail,
        password: testPassword
      }
    });

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body.accessToken).toBeDefined();
  });

  test('Should refresh token using refresh token', async ({ request }) => {
    const response = await request.post('/api/auth/refresh', {
      data: {
        accessToken: accessToken,
        refreshToken: refreshToken
      }
    });

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body.accessToken).toBeDefined();
  });

  test('Should revoke refresh token successfully', async ({ request }) => {
    const response = await request.post('/api/auth/revoke', {
      headers: {
        'Authorization': `Bearer ${accessToken}`
      },
      data: {
        refreshToken: refreshToken
      }
    });

    expect(response.status() === 204 || response.status() === 200).toBeTruthy();
  });

  // ── Negative & Boundary Tests ──────────────────────────────────────────────

  test('Should fail login with incorrect password', async ({ request }) => {
    const response = await request.post('/api/auth/login', {
      data: {
        email: 'admin@sciqustickets.com',
        password: 'WrongPassword'
      }
    });

    expect(response.status()).toBe(400); // Usually 400 Bad Request or 401 Unauthorized depending on API response
  });

  test('Should fail registration with mismatching passwords', async ({ request }) => {
    const response = await request.post('/api/auth/register', {
      data: {
        fullName: 'Fail User',
        email: `fail_${Date.now()}@test.com`,
        password: 'Password@123',
        confirmPassword: 'DifferentPassword@123',
        phoneNumber: '1234567890',
        role: 'Employee'
      }
    });

    expect(response.status()).toBe(400);
    const body = await response.json();
    expect(JSON.stringify(body)).toContain('match');
  });

  test('Should fail registration with invalid email format', async ({ request }) => {
    const response = await request.post('/api/auth/register', {
      data: {
        fullName: 'Fail User',
        email: 'invalid-email-format',
        password: 'Password@123',
        confirmPassword: 'Password@123',
        phoneNumber: '1234567890',
        role: 'Employee'
      }
    });

    expect(response.status()).toBe(400);
  });

  test('Should deny access to me endpoint without authorization token', async ({ request }) => {
    const response = await request.get('/api/auth/me');
    expect(response.status()).toBe(401);
  });
});
