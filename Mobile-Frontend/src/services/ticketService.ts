import api from './api';

export interface Ticket {
  id: string;
  title: string;
  description: string;
  status: number; // 1: NotAssigned, 2: CurrentlyAssigned, 3: InProgress, 4: Resolved, 5: Closed
  priority: number; // 0: Low, 1: Medium, 2: High, 3: Critical
  assignedEmployeeId?: string | null;
  assignedToName?: string | null;
  customerId?: string | null;
  customerName?: string | null;
  createdById: string;
  createdAt: string;
  updatedAt?: string | null;
  slaDeadline?: string | null;
  isSlaBreached?: boolean;
}

export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
}

export interface CreateTicketDto {
  title: string;
  description: string;
  departmentId: string;
  priority: number;
}

export interface UpdateTicketDto {
  title: string;
  description: string;
  status: number;
  priority: number;
  assignedToId?: string | null;
}

export interface AssignTicketDto {
  assignedToId: string;
}

const TicketService = {
  getAll: async (
    pageNumber = 1,
    pageSize = 10,
    searchTerm?: string,
    status?: number,
    priority?: number,
    sortBy?: string,
    sortDesc = false
  ): Promise<PagedResponse<Ticket>> => {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      sortDesc: sortDesc.toString(),
    });
    if (searchTerm) params.append('search', searchTerm);
    if (status !== undefined) params.append('status', status.toString());
    if (priority !== undefined) params.append('priority', priority.toString());
    if (sortBy) params.append('sortBy', sortBy);

    const response = await api.get(`/tickets?${params.toString()}`);
    const data = response.data;
    
    // If backend returns a flat array instead of a PagedResponse, wrap it.
    if (Array.isArray(data)) {
      return {
        items: data,
        pageNumber: pageNumber,
        pageSize: pageSize,
        totalPages: data.length === pageSize ? pageNumber + 1 : pageNumber, // Guess if there's more
        totalCount: data.length
      };
    }
    
    return data as PagedResponse<Ticket>;
  },

  assignTicket: async (id: string, assignedToId: string): Promise<Ticket> => {
    const response = await api.patch<Ticket>(`/tickets/${id}/assign`, { employeeId: assignedToId });
    return response.data;
  },

  getById: async (id: string): Promise<Ticket> => {
    const response = await api.get(`/tickets/${id}`);
    return response.data;
  },

  create: async (data: CreateTicketDto): Promise<Ticket> => {
    const response = await api.post('/tickets', data);
    return response.data;
  },

  update: async (id: string, data: UpdateTicketDto): Promise<Ticket> => {
    const response = await api.put(`/tickets/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/tickets/${id}`);
  },
};

export default TicketService;
