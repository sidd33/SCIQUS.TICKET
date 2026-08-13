import { useEffect, useState } from "react";
import { Plus, Search, Pencil, Trash2, Users, X, ChevronLeft, ChevronRight } from "lucide-react";
import api, { resolveImageUrl } from "../../api/axios";
import { isAdmin } from "../../auth/roles";
import "./Employees.scss";

const ROLE_OPTIONS = [
  { value: 1, label: "Employee" },
  { value: 2, label: "Administrator" },
];
const ROLE_LABELS = { 1: "Employee", 2: "Administrator", 3: "Super Admin" };

const AVATAR_PALETTE = ["#4361ee", "#22c55e", "#f59e0b", "#0ea5e9", "#ec4899", "#8b5cf6"];

const EMPTY_FORM = { firstName: "", lastName: "", email: "", password: "", departmentId: "", role: 1 };

export default function Employees() {
  const currentUser = JSON.parse(localStorage.getItem("user") || "null");
  const canManage = isAdmin(currentUser);

  const [employees, setEmployees] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [loading, setLoading] = useState(true);

  const [search, setSearch] = useState("");
  const [departmentFilter, setDepartmentFilter] = useState("");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const PAGE_SIZE = 5;

  const [showModal, setShowModal] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [formData, setFormData] = useState(EMPTY_FORM);
  const [formError, setFormError] = useState("");
  const [fieldErrors, setFieldErrors] = useState({});

  useEffect(() => { fetchDepartments(); }, []);
  useEffect(() => { fetchEmployees(); }, [page, search, departmentFilter]);
  useEffect(() => { setPage(1); }, [search, departmentFilter]);

  async function fetchDepartments() {
    const res = await api.get('/departments', { params: { pageSize: 100 } });
    setDepartments(res.data.items);
  }

  async function fetchEmployees() {
    setLoading(true);
    try {
      const res = await api.get("/employees", {
        params: {
          pageNumber: page,
          pageSize: PAGE_SIZE,
          searchTerm: search || undefined, // EmployeeQueryParameters.SearchTerm
          departmentId: departmentFilter || undefined,
        },
      });
      const mappedEmployees = (res.data.items || []).map((emp) => {
  const nameParts = (emp.name || "").trim().split(/\s+/);

  return {
    ...emp,
    firstName: nameParts[0] || "",
    lastName: nameParts.slice(1).join(" ") || "",
    profilePicture: emp.profileImageUrl || null,
  };
});

setEmployees(mappedEmployees);
setTotalPages(res.data.totalPages || 1);
    } catch (err) {
      console.log(err);
    } finally {
      setLoading(false);
    }
  }

  function openCreateModal() {
    setEditingId(null);
    setFormData(EMPTY_FORM);
    setFormError(""); setFieldErrors({});
    setShowModal(true);
  }

  function openEditModal(emp) {
    setEditingId(emp.id);
    setFormData({
      firstName: emp.firstName,
      lastName: emp.lastName,
      email: emp.email,
      password: "",
      departmentId: emp.departmentId || "", // flat field, not emp.department.id
      role: emp.role,
    });
    setFormError(""); setFieldErrors({});
    setShowModal(true);
  }

  function closeModal() {
    setShowModal(false); setEditingId(null); setFormData(EMPTY_FORM); setFieldErrors({});
  }

  function handleChange(e) {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: name === "role" ? Number(value) : value });
    if (fieldErrors[name]) {
      setFieldErrors((prev) => { const n = { ...prev }; delete n[name]; return n; });
    }
  }

  function validateClientSide() {
    const errors = {};
    if (!formData.firstName.trim()) errors.firstName = "First name is required.";
    if (!formData.lastName.trim()) errors.lastName = "Last name is required.";
    if (!formData.email.trim()) errors.email = "Email is required.";
    if (!editingId) {
      if (!formData.password) errors.password = "Password is required.";
      else if (formData.password.length < 6) errors.password = "Password must be at least 6 characters.";
    }
    if (!formData.departmentId) errors.departmentId = "Department is required.";
    return errors;
  }

  function mapBackendErrors(errors) {
    const mapped = {};
    Object.entries(errors).forEach(([field, messages]) => {
      const key = field.charAt(0).toLowerCase() + field.slice(1);
      mapped[key] = Array.isArray(messages) ? messages[0] : messages;
    });
    return mapped;
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setFormError(""); setFieldErrors({});
    const clientErrors = validateClientSide();
    if (Object.keys(clientErrors).length > 0) { setFieldErrors(clientErrors); return; }

    try {
      if (editingId) {
        // UpdateEmployeeDto: FirstName, LastName, Email, IsActive — no DepartmentId
        await api.put(`/employees/${editingId}`, {
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          isActive: true,
        });
        // Department change is a separate endpoint
        await api.put(`/employees/${editingId}/department`, {
          departmentId: formData.departmentId,
        });
      } else {
        await api.post("/employees", formData);
      }
      closeModal();
      fetchEmployees();
    } catch (err) {
      const backendErrors = err.response?.data?.errors;
      if (backendErrors) setFieldErrors(mapBackendErrors(backendErrors));
      else setFormError(err.response?.data?.message || "Unable to save employee.");
    }
  }

  async function handleDelete(id) {
    if (!window.confirm("Delete this employee?")) return;
    try {
      await api.delete(`/employees/${id}`);
      fetchEmployees();
    } catch (err) {
      alert(err.response?.data?.message);
    }
  }

  return (
    <div className="employees-page">
      <div className="employees-header">
        <div>
          <h1>Employees</h1>
          <p>Manage everyone across your organization.</p>
        </div>
        {canManage && (
          <button className="add-btn" onClick={openCreateModal}>
            <Plus size={18} /> Add Employee
          </button>
        )}
      </div>

      <div className="toolbar">
        <div className="search-box">
          <Search size={18} />
          <input placeholder="Search employees..." value={search} onChange={(e) => setSearch(e.target.value)} />
        </div>

        <select value={departmentFilter} onChange={(e) => setDepartmentFilter(e.target.value)}>
          <option value="">All Departments</option>
          {departments.map((d) => (
            <option key={d.id} value={d.id}>{d.name}</option>
          ))}
        </select>
      </div>

      {loading ? (
        <div className="loading-state"><Users size={42} /><p>Loading employees...</p></div>
      ) : employees.length === 0 ? (
        <div className="empty-state"><Users size={48} /><h3>No Employees Found</h3><p>Try changing your search or filters.</p></div>
      ) : (
        <div className="employee-table-card">
          <div className="employee-table">
            <div className="table-row table-head">
              <span>Employee</span><span>Department</span><span>Role</span><span>Actions</span>
            </div>

            {employees.map((emp, i) => {
              const photoUrl = resolveImageUrl(emp.profilePicture); // ProfilePicture, not profilePictureUrl
              const avatarColor = AVATAR_PALETTE[i % AVATAR_PALETTE.length];
              return (
                <div className="table-row" key={emp.id}>
                  <div className="cell employee-cell">
                    <div className="avatar" style={photoUrl ? { backgroundImage: `url(${photoUrl})`, backgroundSize: "cover", backgroundPosition: "center" } : { background: avatarColor }}>
                      {!photoUrl && (<>{emp.firstName[0]}{emp.lastName[0]}</>)}
                    </div>
                    <div className="employee-info">
                      <strong>{emp.firstName} {emp.lastName}</strong>
                      <span>{emp.email}</span>
                    </div>
                  </div>

                  <div className="cell"><span className="dept-badge">{emp.departmentName || 'None'}</span></div>

                  <div className="cell">
                    <span className={`role-badge ${emp.role === 2 || emp.role === 3 ? "role-admin" : "role-employee"}`}>
                      {ROLE_LABELS[emp.role]}
                    </span>
                  </div>

                  <div className="cell actions-cell">
                    {(canManage || currentUser?.id === emp.id) && (
                      <button className="edit-btn" onClick={() => openEditModal(emp)}><Pencil size={14} />Edit</button>
                    )}
                    {canManage && (
                      <button className="delete-btn" onClick={() => handleDelete(emp.id)}><Trash2 size={14} />Delete</button>
                    )}
                  </div>
                </div>
              );
            })}
          </div>

          <div className="table-footer">
            <span>Page {page} of {totalPages}</span>
            <div className="pagination">
              <button disabled={page === 1} onClick={() => setPage((p) => p - 1)}><ChevronLeft size={16} /></button>
              <button disabled={page === totalPages} onClick={() => setPage((p) => p + 1)}><ChevronRight size={16} /></button>
            </div>
          </div>
        </div>
      )}

      {showModal && (
        <div className="modal-overlay" onClick={closeModal}>
          <div className="employee-modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>{editingId ? "Edit Employee" : "Add Employee"}</h2>
              <button className="close-btn" onClick={closeModal}><X size={20} /></button>
            </div>

            {formError && <div className="form-error">{formError}</div>}

            <form onSubmit={handleSubmit} noValidate>
              <div className="field-group">
                <input name="firstName" placeholder="First Name" value={formData.firstName} onChange={handleChange} className={fieldErrors.firstName ? "input-error" : ""} />
                {fieldErrors.firstName && <span className="field-error-text">{fieldErrors.firstName}</span>}
              </div>

              <div className="field-group">
                <input name="lastName" placeholder="Last Name" value={formData.lastName} onChange={handleChange} className={fieldErrors.lastName ? "input-error" : ""} />
                {fieldErrors.lastName && <span className="field-error-text">{fieldErrors.lastName}</span>}
              </div>

              <div className="field-group">
                <input type="email" name="email" placeholder="Email" value={formData.email} onChange={handleChange} className={fieldErrors.email ? "input-error" : ""} />
                {fieldErrors.email && <span className="field-error-text">{fieldErrors.email}</span>}
              </div>

              {!editingId && (
                <div className="field-group">
                  <input type="password" name="password" placeholder="Password" value={formData.password} onChange={handleChange} className={fieldErrors.password ? "input-error" : ""} />
                  {fieldErrors.password && <span className="field-error-text">{fieldErrors.password}</span>}
                </div>
              )}

              <div className="field-group">
                <select name="departmentId" value={formData.departmentId} onChange={handleChange} className={fieldErrors.departmentId ? "input-error" : ""}>
                  <option value="">Select department</option>
                  {departments.map((d) => (<option key={d.id} value={d.id}>{d.name}</option>))}
                </select>
                {fieldErrors.departmentId && <span className="field-error-text">{fieldErrors.departmentId}</span>}
              </div>

              {!editingId && (
                <div className="field-group">
                  <select name="role" value={formData.role} onChange={handleChange}>
                    {ROLE_OPTIONS.map((r) => (<option key={r.value} value={r.value}>{r.label}</option>))}
                  </select>
                </div>
              )}

              <div className="modal-actions">
                <button type="button" className="cancel-btn" onClick={closeModal}>Cancel</button>
                <button type="submit" className="save-btn">{editingId ? "Save Changes" : "Create Employee"}</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}