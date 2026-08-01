import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet, SafeAreaView, ActivityIndicator, Alert, ScrollView } from 'react-native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { RouteProp } from '@react-navigation/native';
import { RootStackParamList } from '../../App';
import EmployeeService, { Employee } from '../services/employeeService';
import GlassCard from '../components/GlassCard';
import AppleButton from '../components/AppleButton';
import ProfileAvatar from '../components/ProfileAvatar';
import { Briefcase, Mail, User, Calendar } from 'lucide-react-native';
import { authService, User as AuthUser } from '../services/authService';

type EmployeeDetailScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, 'EmployeeDetail'>;
type EmployeeDetailScreenRouteProp = RouteProp<RootStackParamList, 'EmployeeDetail'>;

interface Props {
  navigation: EmployeeDetailScreenNavigationProp;
  route: EmployeeDetailScreenRouteProp;
}

const EmployeeDetailScreen: React.FC<Props> = ({ navigation, route }) => {
  const { id } = route.params;
  const [employee, setEmployee] = useState<Employee | null>(null);
  const [currentUser, setCurrentUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    authService.getUser().then(setCurrentUser);
    fetchEmployee();
  }, [id]);

  const fetchEmployee = async () => {
    try {
      const data = await EmployeeService.getById(id);
      setEmployee(data);
      navigation.setOptions({ title: `${data.firstName} ${data.lastName}` });
    } catch (error) {
      Alert.alert('Error', 'Failed to fetch employee details');
      navigation.goBack();
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = () => {
    Alert.alert('Delete Employee', 'Are you sure you want to delete this employee?', [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Delete',
        style: 'destructive',
        onPress: async () => {
          setDeleting(true);
          try {
            await EmployeeService.delete(id);
            navigation.goBack();
          } catch (error) {
            Alert.alert('Error', 'Failed to delete employee');
            setDeleting(false);
          }
        },
      },
    ]);
  };

  if (loading || !employee) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color="#007AFF" />
      </View>
    );
  }

  const roleName = employee.role === 2 ? 'Admin' : 'Employee';

  return (
    <SafeAreaView style={styles.container}>
      <ScrollView contentContainerStyle={styles.scrollContent}>
        <GlassCard style={styles.card}>
          <ProfileAvatar 
            firstName={employee.firstName} 
            lastName={employee.lastName} 
            photoUrl={employee.photoUrl} 
            size={80} 
          />
          <View style={{ height: 16 }} />
          <Text style={styles.nameHeader}>{employee.firstName} {employee.lastName}</Text>
          <Text style={styles.roleHeader}>{roleName}</Text>
        </GlassCard>

        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Contact Information</Text>
          <GlassCard style={styles.infoCard}>
            <View style={styles.infoRow}>
              <Mail size={20} color="#8E8E93" />
              <Text style={styles.infoText}>{employee.email}</Text>
            </View>
            <View style={[styles.infoRow, styles.noBorder]}>
              <User size={20} color="#8E8E93" />
              <Text style={styles.infoText}>Status: {employee.isActive ? 'Active' : 'Inactive'}</Text>
            </View>
          </GlassCard>
        </View>

        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Work</Text>
          <GlassCard style={styles.infoCard}>
            <View style={styles.infoRow}>
              <Briefcase size={20} color="#8E8E93" />
              <Text style={styles.infoText}>{employee.department?.name || 'No Department Assigned'}</Text>
            </View>
            <View style={[styles.infoRow, styles.noBorder]}>
              <Calendar size={20} color="#8E8E93" />
              <Text style={styles.infoText}>Joined: {new Date(employee.createdAt).toLocaleDateString()}</Text>
            </View>
          </GlassCard>
        </View>

        {currentUser?.role === 2 && (
          <View style={styles.actions}>
            <AppleButton
              title="Edit Employee"
              onPress={() => navigation.navigate('EmployeeEdit', { id })}
              variant="secondary"
            />
            <AppleButton
              title="Delete Employee"
              onPress={handleDelete}
              variant="danger"
              isLoading={deleting}
            />
          </View>
        )}
      </ScrollView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#F2F2F7',
  },
  center: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  scrollContent: {
    padding: 16,
  },
  card: {
    alignItems: 'center',
    paddingVertical: 30,
  },
  avatarLarge: {
    width: 80,
    height: 80,
    borderRadius: 40,
    backgroundColor: '#E5E5EA',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 16,
  },
  avatarLargeText: {
    fontSize: 28,
    fontWeight: '600',
    color: '#3A3A3C',
  },
  nameHeader: {
    fontSize: 24,
    fontWeight: '700',
    color: '#000000',
    marginBottom: 4,
  },
  roleHeader: {
    fontSize: 16,
    color: '#007AFF',
    fontWeight: '500',
  },
  section: {
    marginTop: 24,
  },
  sectionTitle: {
    fontSize: 13,
    fontWeight: '600',
    color: '#8E8E93',
    textTransform: 'uppercase',
    marginBottom: 8,
    marginLeft: 12,
  },
  infoCard: {
    padding: 0,
    overflow: 'hidden',
  },
  infoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 16,
    borderBottomWidth: StyleSheet.hairlineWidth,
    borderBottomColor: '#E5E5EA',
    backgroundColor: '#FFFFFF',
  },
  noBorder: {
    borderBottomWidth: 0,
  },
  infoText: {
    fontSize: 16,
    color: '#000000',
    marginLeft: 12,
  },
  actions: {
    marginTop: 32,
    marginBottom: 20,
  },
});

export default EmployeeDetailScreen;
