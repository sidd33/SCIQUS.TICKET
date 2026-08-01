import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet, ScrollView, ActivityIndicator, TouchableOpacity, Alert } from 'react-native';
import { useNavigation } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { RootStackParamList } from '../../App';
import { authService, User, authEmitter } from '../services/authService';
import api from '../services/api';
import EmployeeService from '../services/employeeService';
import CustomerService from '../services/customerService';
import AppleButton from '../components/AppleButton';
import ProfileAvatar from '../components/ProfileAvatar';

interface Policy {
  id: string;
  name: string;
  description: string;
}

const ProfileScreen = () => {
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();
  const [user, setUser] = useState<User | null>(null);
  const [photoUrl, setPhotoUrl] = useState<string | null>(null);
  const [policies, setPolicies] = useState<Policy[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchProfileData();

    const listener = () => fetchProfileData();
    authEmitter.on('profile_updated', listener);
    return () => {
      authEmitter.off('profile_updated', listener);
    };
  }, []);

  const fetchProfileData = async () => {
    try {
      setLoading(true);
      const currentUser = await authService.getUser();
      setUser(currentUser);

      if (currentUser?.role !== undefined) {
        if (currentUser.role === 0) {
          try {
            const data = await CustomerService.getById(currentUser.id);
            setPhotoUrl(data.profilePicture || null);
            setUser(prev => prev ? { ...prev, firstName: data.name.split(' ')[0], lastName: data.name.split(' ').slice(1).join(' ') } : null);
          } catch (e) {}
        } else {
          try {
            const data = await EmployeeService.getById(currentUser.id);
            setPhotoUrl(data.photoUrl || null);
            setUser(prev => prev ? { ...prev, firstName: data.firstName, lastName: data.lastName } : null);
          } catch (e) {}
        }

        // Define policies based on the decoded role
        let assignedPolicies: Policy[] = [];
        
        switch(currentUser.role) {
          case 3: // SuperAdmin
            assignedPolicies = [
              { id: '1', name: 'System.Manage', description: 'Full access to all system configurations' },
              { id: '2', name: 'Employees.Manage', description: 'Can create, edit, and delete employee records' },
              { id: '3', name: 'Departments.Manage', description: 'Can manage all departments' },
              { id: '4', name: 'Tickets.ManageAll', description: 'Can view and assign all tickets' }
            ];
            break;
          case 2: // Admin
            assignedPolicies = [
              { id: '1', name: 'Employees.Read', description: 'Can view employee directories' },
              { id: '2', name: 'Tickets.ManageAll', description: 'Can view and assign all tickets' },
              { id: '3', name: 'Customers.Read', description: 'Can view customer profiles' }
            ];
            break;
          case 1: // Employee
            assignedPolicies = [
              { id: '1', name: 'Tickets.ReadAssigned', description: 'Can view tickets assigned to them' },
              { id: '2', name: 'Tickets.Update', description: 'Can update status of assigned tickets' },
              { id: '3', name: 'Profile.Update', description: 'Can update own profile settings' }
            ];
            break;
          case 0: // Customer
            assignedPolicies = [
              { id: '1', name: 'Tickets.Create', description: 'Can create new support tickets' },
              { id: '2', name: 'Tickets.ReadOwn', description: 'Can view status of own tickets' },
              { id: '3', name: 'Profile.Update', description: 'Can update own profile details' }
            ];
            break;
        }
        
        setPolicies(assignedPolicies);
      }
    } catch (error) {
      console.error('Error fetching profile data', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color="#007AFF" />
      </View>
    );
  }

  return (
    <ScrollView style={styles.container}>
      <View style={styles.card}>
        <View style={styles.avatarContainer}>
          <ProfileAvatar 
            firstName={user?.firstName || ''} 
            lastName={user?.lastName || ''} 
            photoUrl={photoUrl} 
            size={80} 
            onPress={() => navigation.navigate('ProfileEdit')}
          />
        </View>
        <TouchableOpacity style={styles.editAvatarButton} onPress={() => navigation.navigate('ProfileEdit')}>
          <Text style={styles.editAvatarText}>Edit Profile</Text>
        </TouchableOpacity>
        
        <Text style={styles.emailText}>{user?.email}</Text>
        <Text style={styles.roleText}>
          {user?.role === 0 ? 'Customer' : user?.role === 1 ? 'Employee' : user?.role === 2 ? 'Admin' : user?.role === 3 ? 'Super Admin' : 'Unknown Role'}
        </Text>
      </View>

      <Text style={styles.sectionTitle}>Your Policies & Permissions</Text>
      <View style={styles.card}>
        <View style={{ width: '100%' }}>
          {policies.length > 0 ? (
            policies.map((policy, index) => (
              <View key={policy.id} style={[styles.policyRow, index === policies.length - 1 && styles.lastPolicyRow]}>
                <Text style={styles.policyName}>{policy.name}</Text>
                <Text style={styles.policyDesc}>{policy.description}</Text>
              </View>
            ))
          ) : (
            <Text style={styles.noPolicies}>No policies assigned.</Text>
          )}
        </View>
      </View>

      <View style={{ marginTop: 24, paddingHorizontal: 16, marginBottom: 40 }}>
        <AppleButton 
          title="Log Out" 
          onPress={() => authEmitter.emit('session_expired')} 
        />
      </View>
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#F2F2F7',
    padding: 16,
  },
  center: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#F2F2F7',
  },
  card: {
    backgroundColor: '#FFFFFF',
    borderRadius: 12,
    padding: 16,
    marginBottom: 20,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 2,
    alignItems: 'center',
  },
  avatarContainer: {
    width: 80,
    height: 80,
    borderRadius: 40,
    backgroundColor: '#E5E5EA',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 12,
  },
  avatarText: {
    fontSize: 32,
    fontWeight: 'bold',
    color: '#8E8E93',
  },
  editAvatarButton: {
    marginBottom: 12,
  },
  editAvatarText: {
    color: '#007AFF',
    fontSize: 14,
    fontWeight: '500',
  },
  emailText: {
    fontSize: 18,
    fontWeight: '600',
    color: '#000000',
    marginBottom: 4,
  },
  roleText: {
    fontSize: 14,
    color: '#8E8E93',
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: '600',
    color: '#000000',
    marginBottom: 12,
    marginLeft: 4,
  },
  policyRow: {
    width: '100%',
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: '#E5E5EA',
  },
  lastPolicyRow: {
    borderBottomWidth: 0,
  },
  policyName: {
    fontSize: 16,
    fontWeight: '500',
    color: '#000000',
    marginBottom: 4,
  },
  policyDesc: {
    fontSize: 14,
    color: '#8E8E93',
  },
  noPolicies: {
    fontSize: 16,
    color: '#8E8E93',
    padding: 20,
    textAlign: 'center',
  },
});

export default ProfileScreen;
