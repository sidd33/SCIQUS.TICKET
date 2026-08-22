export const isAdmin = (user) => {
  if (!user || !user.role) return false;
  const r = Array.isArray(user.role) ? user.role : [user.role];
  return r.includes('Admin') || r.includes('SuperAdmin');
};

export const isDepartmentHead = (user) => {
  if (!user) return false;
  return user.isDepartmentHead || user.role === 'DepartmentHead';
};

export const isEmployee = (user) => {
  if (!user || !user.role) return false;

  const r = Array.isArray(user.role) ? user.role : [user.role];

  return (
    r.includes('Employee') ||
    r.includes('SupportAgent') ||
    r.includes('DepartmentHead') ||
    isAdmin(user)
  );
};

export const isCustomer = (user) => {
  if (!user || !user.role) return false;
  const r = Array.isArray(user.role) ? user.role : [user.role];
  return r.includes('Customer');
};
