const baseUrl = 'http://localhost:5239';

async function runManualTests() {
  console.log('==================================================');
  console.log('           MANUAL API TEST RUNNER                 ');
  console.log('==================================================\n');

  let adminToken = '';
  let employeeId = '';
  let gradeId = '';
  let deptId = '';
  const testEmail = `manual_user_${Date.now()}@test.com`;
  const testPassword = 'Password@123';

  // Helper function to send requests and log details
  async function sendRequest(method, path, body = null, headers = {}) {
    const url = `${baseUrl}${path}`;
    console.log(`[REQUEST] ${method} ${url}`);
    if (body) {
      console.log(`Payload: ${JSON.stringify(body, null, 2)}`);
    }

    const options = {
      method,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        ...headers
      }
    };
    if (body) {
      options.body = JSON.stringify(body);
    }

    try {
      const response = await fetch(url, options);
      console.log(`[RESPONSE] Status: ${response.status} ${response.statusText}`);
      
      const text = await response.text();
      let json = null;
      if (text) {
        try {
          json = JSON.parse(text);
          console.log(`Response Body:\n${JSON.stringify(json, null, 2)}`);
        } catch (e) {
          console.log(`Response Body (Raw): ${text}`);
        }
      }
      console.log('--------------------------------------------------\n');
      return { status: response.status, body: json, text };
    } catch (error) {
      console.error(`[ERROR] Request failed: ${error.message}`);
      console.log('--------------------------------------------------\n');
      return { status: 500, error };
    }
  }

  // 1. LOGIN ADMIN (Positive)
  console.log('--- TEST 1: Login Admin User ---');
  const loginRes = await sendRequest('POST', '/api/auth/login', {
    email: 'admin@sciqustickets.com',
    password: 'Admin@123'
  });
  if (loginRes.status === 200 && loginRes.body.accessToken) {
    adminToken = loginRes.body.accessToken;
    console.log('✅ Admin Login Succeeded!');
  } else {
    console.log('❌ Admin Login Failed!');
  }

  // 2. GET CURRENT USER (Positive)
  console.log('--- TEST 2: Get Current Logged-in User Info ---');
  const meRes = await sendRequest('GET', '/api/auth/me', null, {
    'Authorization': `Bearer ${adminToken}`
  });
  if (meRes.status === 200) {
    console.log('✅ Get Current User (me) Succeeded!');
  } else {
    console.log('❌ Get Current User (me) Failed!');
  }

  // 3. REGISTER NEW EMPLOYEE USER (Positive)
  console.log('--- TEST 3: Register New User ---');
  const regRes = await sendRequest('POST', '/api/auth/register', {
    fullName: 'Manual Test Employee',
    email: testEmail,
    password: testPassword,
    confirmPassword: testPassword,
    phoneNumber: '9998887776',
    role: 'Employee'
  });

  // 4. CREATE A GRADE (Positive)
  console.log('--- TEST 4: Create a Grade ---');
  const gradeRes = await sendRequest('POST', '/api/grades', {
    gradeLevel: 6,
    description: 'Manual Level 6'
  });
  if (gradeRes.status === 201) {
    gradeId = gradeRes.body.id;
    console.log('✅ Grade Created Successfully!');
  }

  // 5. CREATE A DEPARTMENT (Positive)
  console.log('--- TEST 5: Create a Department ---');
  const deptRes = await sendRequest('POST', '/api/departments', {
    name: 'Manual Quality Assurance'
  });
  if (deptRes.status === 201) {
    deptId = deptRes.body.departmentId;
    console.log('✅ Department Created Successfully!');
  }

  // Login as new user to get User ID
  console.log('--- LOGIN NEW USER to get ID ---');
  const loginNewRes = await sendRequest('POST', '/api/auth/login', {
    email: testEmail,
    password: testPassword
  });
  let newUserId = '';
  if (loginNewRes.status === 200 && loginNewRes.body.user) {
    newUserId = loginNewRes.body.user.id;
  }

  // 6. CREATE AN EMPLOYEE (Positive)
  console.log('--- TEST 6: Create an Employee Record ---');
  const empRes = await sendRequest('POST', `/api/employees/${newUserId}`, {
    name: 'Manual Test Employee',
    registeredMobileNumber: '9998887776',
    email: testEmail,
    employeeId: `EMP-M-${Date.now()}`,
    designation: 'QA Automation Engineer',
    departmentId: deptId,
    gradeId: gradeId
  });
  if (empRes.status === 201) {
    employeeId = empRes.body.id;
    console.log('✅ Employee Created Successfully!');
  }

  // 7. CREATE AN ACCOUNT (Should Fail due to PK-FK constraint on ApplicationUser)
  console.log('--- TEST 7: Create an Account (FK Constraint Bug Demonstration) ---');
  const accRes = await sendRequest('POST', '/api/accounts', {
    accountName: 'Manual Acme Corp',
    registeredMobileNumber: '1234567890',
    email: 'billing@manualacme.com',
    createdByUserId: employeeId,
    accountManagerId: employeeId
  });
  if (accRes.status === 500) {
    console.log('❌ Account Creation Failed (as expected due to the domain bug)!');
  }

  // 8. NEGATIVE TEST: Login with wrong password
  console.log('--- TEST 8: Login with Incorrect Password (Demonstrates 500 error due to unhandled exception) ---');
  await sendRequest('POST', '/api/auth/login', {
    email: 'admin@sciqustickets.com',
    password: 'wrong_password'
  });

  // 9. NEGATIVE TEST: Update non-existent grade ID (Demonstrates 500 due to unhandled KeyNotFoundException)
  console.log('--- TEST 9: Update Non-existent Grade (Demonstrates 500 due to unhandled KeyNotFoundException) ---');
  await sendRequest('PUT', '/api/grades/00000000-0000-0000-0000-000000000000', {
    gradeLevel: 10,
    description: 'Ghost Grade'
  });

  // 10. NEGATIVE TEST: Set non-existent head on department (Demonstrates 500 due to DB FK constraint violation)
  console.log('--- TEST 10: Set Non-existent Head on Department (Demonstrates 500 due to DB FK constraint) ---');
  await sendRequest('PATCH', `/api/departments/${deptId}/head`, {
    departmentHeadId: 'non-existent-employee-id'
  });

  console.log('==================================================');
  console.log('          MANUAL TEST RUN COMPLETE                ');
  console.log('==================================================');
}

runManualTests();
