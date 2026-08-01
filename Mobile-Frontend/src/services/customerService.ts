import api from './api';

export interface Customer {
  id: string;
  name: string;
  email: string;
  profilePicture?: string;
}

export interface UpdateCustomerDto {
  name: string;
  email: string;
  password?: string;
}

const CustomerService = {
  getById: async (id: string): Promise<Customer> => {
    const response = await api.get<Customer>(`/Customers/${id}`);
    return response.data;
  },

  update: async (id: string, data: UpdateCustomerDto): Promise<Customer> => {
    const response = await api.put<Customer>(`/Customers/${id}`, data);
    return response.data;
  },

  uploadProfilePicture: async (id: string, photoUri: string, mimeType: string): Promise<string> => {
    const formData = new FormData();
    formData.append('File', {
      uri: photoUri,
      type: mimeType,
      name: 'profile.jpg',
    } as any);

    const response = await api.post(`/Customers/${id}/profile-picture`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data.profilePicture;
  }
};

export default CustomerService;
