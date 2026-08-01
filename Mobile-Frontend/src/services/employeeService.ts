import api from './api';

export interface Employee {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  department?: {
    id: string;
    name: string;
    description: string;
  };
  role: number; // 1 for Employee, 2 for Admin
  isActive: boolean;
  photoUrl?: string;
  createdAt: string;
}

export interface CreateEmployeeDto {
  firstName: string;
  lastName: string;
  email: string;
  department?: string;
  role?: number;
  photoUrl?: string;
  password?: string;
}

export interface UpdateEmployeeDto {
  firstName: string;
  lastName: string;
  email: string;
  department?: string;
  role?: number;
  isActive?: boolean;
  photoUrl?: string;
}

export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
}

const EmployeeService = {
  getAll: async (
    pageNumber = 1,
    pageSize = 10,
    searchTerm?: string,
    sortBy?: string,
    sortDesc = false
  ): Promise<PagedResponse<Employee>> => {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      sortDesc: sortDesc.toString(),
    });
    if (searchTerm) params.append('search', searchTerm);
    if (sortBy) params.append('sortBy', sortBy);

    const response = await api.get<PagedResponse<Employee>>(`/Employees?${params.toString()}`);
    return response.data;
  },

  getById: async (id: string): Promise<Employee> => {
    const response = await api.get<Employee>(`/Employees/${id}`);
    return response.data;
  },

  create: async (data: CreateEmployeeDto): Promise<Employee> => {
    const response = await api.post<Employee>('/Employees', data);
    return response.data;
  },

  update: async (id: string, data: UpdateEmployeeDto): Promise<Employee> => {
    const response = await api.put<Employee>(`/Employees/${id}`, data);
    return response.data;
  },

  updateDepartment: async (id: string, department: string): Promise<Employee> => {
    const response = await api.put<Employee>(`/Employees/${id}/department`, { departmentId: department });
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/Employees/${id}`);
  },

  uploadProfilePicture: async (id: string, photoUri: string, mimeType: string): Promise<string> => {
    const formData = new FormData();
    formData.append('File', {
      uri: photoUri,
      type: mimeType,
      name: 'profile.jpg',
    } as any);

    const response = await api.post(`/Employees/${id}/profile-picture`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data.profilePicture;
  }
};

export default EmployeeService;
