import api from './api';

export interface Department {
  id: string;
  name: string;
  description: string;
}

export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
}

const DepartmentService = {
  getAll: async (
    pageNumber = 1,
    pageSize = 100,
    searchTerm?: string
  ): Promise<PagedResponse<Department>> => {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
    });
    if (searchTerm) params.append('search', searchTerm);

    const response = await api.get<PagedResponse<Department>>(`/departments?${params.toString()}`);
    return response.data;
  },
};

export default DepartmentService;
