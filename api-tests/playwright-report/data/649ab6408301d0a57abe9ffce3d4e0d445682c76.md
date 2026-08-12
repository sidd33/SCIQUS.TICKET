# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: accounts.spec.js >> Accounts, Contacts, and Addresses API Tests >> Should return 400 when setting Account as its own parent
- Location: tests\accounts.spec.js:180:3

# Error details

```
Error: expect(received).toBe(expected) // Object.is equality

Expected: 201
Received: 500
```

# Test source

```ts
  91  |     expect(body.accountId).toBeDefined();
  92  |     expect(body.accountName).toBe('Acme Corp');
  93  |     accountId = body.accountId;
  94  |   });
  95  | 
  96  |   test('Should update Account details', async ({ request }) => {
  97  |     const response = await request.put(`/api/accounts/${accountId}`, {
  98  |       data: {
  99  |         accountName: 'Acme Corporation',
  100 |         registeredMobileNumber: '9998887777',
  101 |         email: 'support@acme.com'
  102 |       }
  103 |     });
  104 | 
  105 |     expect(response.status()).toBe(200);
  106 |     const body = await response.json();
  107 |     expect(body.accountName).toBe('Acme Corporation');
  108 |     expect(body.email).toBe('support@acme.com');
  109 |   });
  110 | 
  111 |   test('Should add a Contact to the Account', async ({ request }) => {
  112 |     const response = await request.post(`/api/accounts/${accountId}/contacts`, {
  113 |       data: {
  114 |         personName: 'John Doe',
  115 |         email: 'johndoe@acme.com',
  116 |         mobileNumber: '1112223333',
  117 |         designation: 'IT Director',
  118 |         primaryContact: false
  119 |       }
  120 |     });
  121 | 
  122 |     expect(response.status()).toBe(200);
  123 |     const body = await response.json();
  124 |     expect(body.accountContactsId).toBeDefined();
  125 |     expect(body.personName).toBe('John Doe');
  126 |     contactId = body.accountContactsId;
  127 |   });
  128 | 
  129 |   test('Should set Contact as primary contact', async ({ request }) => {
  130 |     const response = await request.put(`/api/accounts/${accountId}/contacts/${contactId}/set-primary`);
  131 |     expect(response.status()).toBe(200);
  132 |   });
  133 | 
  134 |   test('Should add an Address to the Account', async ({ request }) => {
  135 |     const response = await request.post(`/api/accounts/${accountId}/addresses`, {
  136 |       data: {
  137 |         country: 'USA',
  138 |         city: 'New York',
  139 |         state: 'NY',
  140 |         pincode: '10001',
  141 |         addressLine: '123 Broadway Ave',
  142 |         primaryAddress: false
  143 |       }
  144 |     });
  145 | 
  146 |     expect(response.status()).toBe(200);
  147 |     const body = await response.json();
  148 |     expect(body.accountAddressId).toBeDefined();
  149 |     expect(body.city).toBe('New York');
  150 |     addressId = body.accountAddressId;
  151 |   });
  152 | 
  153 |   test('Should set Address as primary address', async ({ request }) => {
  154 |     const response = await request.put(`/api/accounts/${accountId}/addresses/${addressId}/set-primary`);
  155 |     expect(response.status()).toBe(200);
  156 |   });
  157 | 
  158 |   test('Should delete Contact', async ({ request }) => {
  159 |     const response = await request.delete(`/api/accounts/${accountId}/contacts/${contactId}`);
  160 |     expect(response.status()).toBe(200);
  161 |   });
  162 | 
  163 |   test('Should delete Address', async ({ request }) => {
  164 |     const response = await request.delete(`/api/accounts/${accountId}/addresses/${addressId}`);
  165 |     expect(response.status()).toBe(200);
  166 |   });
  167 | 
  168 |   test('Should soft delete Account', async ({ request }) => {
  169 |     const response = await request.delete(`/api/accounts/${accountId}`);
  170 |     expect(response.status()).toBe(200);
  171 |   });
  172 | 
  173 |   // ── Negative Tests ─────────────────────────────────────────────────────────
  174 | 
  175 |   test('Should return 404 for non-existent Account ID', async ({ request }) => {
  176 |     const response = await request.get(`/api/accounts/${nonExistentId}`);
  177 |     expect(response.status()).toBe(404);
  178 |   });
  179 | 
  180 |   test('Should return 400 when setting Account as its own parent', async ({ request }) => {
  181 |     // Re-create account to test this
  182 |     const res = await request.post('/api/accounts', {
  183 |       data: {
  184 |         accountName: 'Self Parent Test',
  185 |         registeredMobileNumber: '9998887777',
  186 |         email: 'test@self.com',
  187 |         createdByUserId: employeeId,
  188 |         accountManagerId: employeeId
  189 |       }
  190 |     });
> 191 |     expect(res.status()).toBe(201);
      |                          ^ Error: expect(received).toBe(expected) // Object.is equality
  192 |     const acc = await res.json();
  193 |     const tempAccId = acc.accountId;
  194 | 
  195 |     // Put own ID as parentAccountId
  196 |     const putRes = await request.put(`/api/accounts/${tempAccId}`, {
  197 |       data: {
  198 |         accountName: 'Self Parent Test',
  199 |         registeredMobileNumber: '9998887777',
  200 |         email: 'test@self.com',
  201 |         parentAccountId: tempAccId
  202 |       }
  203 |     });
  204 |     expect(putRes.status()).toBe(400); // Throws InvalidOperationException which maps to 400 Bad Request
  205 | 
  206 |     // Clean up
  207 |     await request.delete(`/api/accounts/${tempAccId}`);
  208 |   });
  209 | 
  210 |   test('Should return 404 when deleting non-existent contact', async ({ request }) => {
  211 |     const response = await request.delete(`/api/accounts/${accountId}/contacts/${nonExistentId}`);
  212 |     expect(response.status()).toBe(404);
  213 |   });
  214 | 
  215 |   test('Should return 404 when deleting non-existent address', async ({ request }) => {
  216 |     const response = await request.delete(`/api/accounts/${accountId}/addresses/${nonExistentId}`);
  217 |     expect(response.status()).toBe(404);
  218 |   });
  219 | });
  220 | 
```