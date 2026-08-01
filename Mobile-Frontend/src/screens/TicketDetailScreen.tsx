import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet, SafeAreaView, ScrollView, Alert, TouchableOpacity } from 'react-native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { RouteProp, useIsFocused } from '@react-navigation/native';
import { RootStackParamList } from '../../App';
import TicketService, { Ticket } from '../services/ticketService';
import EmployeeService, { Employee } from '../services/employeeService';
import { authService, User } from '../services/authService';
import GlassCard from '../components/GlassCard';
import AppleButton from '../components/AppleButton';
import SLACountdown from '../components/SLACountdown';
import { ChevronDown, Check } from 'lucide-react-native';

type TicketDetailScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, 'TicketDetail'>;
type TicketDetailScreenRouteProp = RouteProp<RootStackParamList, 'TicketDetail'>;

interface Props {
  navigation: TicketDetailScreenNavigationProp;
  route: TicketDetailScreenRouteProp;
}

const TicketDetailScreen: React.FC<Props> = ({ navigation, route }) => {
  const { id } = route.params;
  const [ticket, setTicket] = useState<Ticket | null>(null);
  const [loading, setLoading] = useState(true);
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [showAssignDropdown, setShowAssignDropdown] = useState(false);
  const isFocused = useIsFocused();

  useEffect(() => {
    if (isFocused) {
      fetchTicket();
      checkUserAndFetchEmployees();
    }
  }, [id, isFocused]);

  const checkUserAndFetchEmployees = async () => {
    try {
      const user = await authService.getUser();
      setCurrentUser(user);
      if (user?.role === 2) {
        const empResponse = await EmployeeService.getAll(1, 100);
        setEmployees(empResponse.items || []);
      }
    } catch (e) {
      console.error(e);
    }
  };

  React.useLayoutEffect(() => {
    navigation.setOptions({
      headerRight: () => (
        <TouchableOpacity onPress={() => navigation.navigate('TicketEdit', { id })}>
          <Text style={styles.editButton}>Edit</Text>
        </TouchableOpacity>
      ),
    });
  }, [navigation, id]);

  const fetchTicket = async () => {
    try {
      setLoading(true);
      const data = await TicketService.getById(id);
      setTicket(data);
    } catch (error) {
      Alert.alert('Error', 'Failed to load ticket details');
      navigation.goBack();
    } finally {
      setLoading(false);
    }
  };

  const handleReassign = async (assignedToId: string) => {
    if (!ticket) return;
    try {
      setLoading(true);
      await TicketService.assignTicket(ticket.id, assignedToId);
      setShowAssignDropdown(false);
      await fetchTicket();
      Alert.alert('Success', 'Ticket reassigned successfully.');
    } catch (error) {
      Alert.alert('Error', 'Failed to reassign ticket.');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    Alert.alert('Delete Ticket', 'Are you sure you want to delete this ticket?', [
      { text: 'Cancel', style: 'cancel' },
      { 
        text: 'Delete', 
        style: 'destructive',
        onPress: async () => {
          try {
            await TicketService.delete(id);
            navigation.goBack();
          } catch (error) {
            Alert.alert('Error', 'Failed to delete ticket');
          }
        }
      }
    ]);
  };

  const getStatusText = (status: number) => {
    switch (status) {
      case 1: return 'Not Assigned';
      case 2: return 'Currently Assigned';
      case 3: return 'In Progress';
      case 4: return 'Resolved';
      case 5: return 'Closed';
      default: return 'Unknown';
    }
  };

  const getStatusColor = (status: number) => {
    switch (status) {
      case 1: return '#8E8E93'; // Gray
      case 2: return '#AF52DE'; // Purple
      case 3: return '#007AFF'; // Blue
      case 4: return '#34C759'; // Green
      case 5: return '#1C1C1E'; // Dark
      default: return '#007AFF';
    }
  };

  const getPriorityText = (priority: number) => {
    switch (priority) {
      case 1: return 'Low';
      case 2: return 'Medium';
      case 3: return 'High';
      case 4: return 'Critical';
      default: return 'Unknown';
    }
  };

  const getPriorityColor = (priority: number) => {
    switch (priority) {
      case 1: return '#34C759'; // Low
      case 2: return '#FFCC00'; // Medium
      case 3: return '#FF9500'; // High
      case 4: return '#FF3B30'; // Critical
      default: return '#8E8E93';
    }
  };

  if (loading || !ticket) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.center}>
          <Text style={styles.loadingText}>Loading...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      {ticket.slaDeadline && (
        <View style={{ paddingHorizontal: 16, paddingTop: 16 }}>
          <SLACountdown deadline={ticket.slaDeadline} isBreached={ticket.isSlaBreached} />
        </View>
      )}
      <ScrollView contentContainerStyle={styles.content}>
        <GlassCard style={styles.card}>
          <View style={styles.header}>
            <Text style={styles.title}>{ticket.title}</Text>
          </View>

          <View style={styles.badgeContainer}>
            <View style={[styles.badge, { backgroundColor: getStatusColor(ticket.status) + '20' }]}>
              <Text style={[styles.badgeText, { color: getStatusColor(ticket.status) }]}>{getStatusText(ticket.status)}</Text>
            </View>
            <View style={[styles.badge, { backgroundColor: getPriorityColor(ticket.priority) + '20' }]}>
              <Text style={[styles.badgeText, { color: getPriorityColor(ticket.priority) }]}>{getPriorityText(ticket.priority)}</Text>
            </View>
          </View>
          
          <View style={styles.section}>
            <Text style={styles.label}>Description</Text>
            <Text style={styles.value}>{ticket.description}</Text>
          </View>
          <View style={styles.section}>
            <Text style={styles.label}>Department</Text>
            <Text style={styles.value}>{ticket.departmentName || 'Unassigned'}</Text>
          </View>

          <View style={styles.section}>
            <Text style={styles.label}>Created At</Text>
            <Text style={styles.value}>{new Date(ticket.createdAt.endsWith('Z') ? ticket.createdAt : `${ticket.createdAt}Z`).toLocaleString()}</Text>
          </View>

          {currentUser?.role === 2 && (
            <View style={styles.section}>
              <Text style={styles.label}>Assigned To</Text>
              <TouchableOpacity 
                style={styles.dropdownButton}
                onPress={() => setShowAssignDropdown(!showAssignDropdown)}
              >
                <Text style={styles.dropdownButtonText}>
                  {ticket.assignedEmployeeId 
                    ? employees.find(e => e.id === ticket.assignedEmployeeId)?.firstName + ' ' + employees.find(e => e.id === ticket.assignedEmployeeId)?.lastName
                    : 'Unassigned'
                  }
                </Text>
                <ChevronDown color="#8E8E93" size={20} />
              </TouchableOpacity>

              {showAssignDropdown && (
                <View style={styles.dropdownList}>
                  {employees.map((emp) => (
                    <TouchableOpacity
                      key={emp.id}
                      style={styles.dropdownItem}
                      onPress={() => handleReassign(emp.id)}
                    >
                      <Text style={[styles.dropdownItemText, ticket.assignedEmployeeId === emp.id && styles.activeItemText]}>{emp.firstName} {emp.lastName}</Text>
                      {ticket.assignedEmployeeId === emp.id && <Check color="#007AFF" size={20} />}
                    </TouchableOpacity>
                  ))}
                </View>
              )}
            </View>
          )}
        </GlassCard>

        <AppleButton 
          title="Delete Ticket" 
          onPress={handleDelete} 
          style={styles.deleteButton} 
          textStyle={styles.deleteButtonText} 
        />
      </ScrollView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#F2F2F7',
  },
  slaBanner: {
    backgroundColor: '#FF3B30',
    padding: 16,
    alignItems: 'center',
    justifyContent: 'center',
  },
  slaBannerText: {
    color: '#FFFFFF',
    fontWeight: '800',
    fontSize: 16,
    marginBottom: 4,
  },
  slaBannerSubtext: {
    color: '#FFFFFF',
    fontSize: 13,
    fontWeight: '500',
    textAlign: 'center',
  },
  center: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  loadingText: {
    fontSize: 17,
    color: '#8E8E93',
  },
  content: {
    padding: 16,
  },
  card: {
    padding: 20,
  },
  header: {
    marginBottom: 20,
    paddingBottom: 16,
    borderBottomWidth: StyleSheet.hairlineWidth,
    borderBottomColor: '#C6C6C8',
  },
  title: {
    fontSize: 22,
    fontWeight: '700',
    color: '#000000',
  },
  badgeContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
    marginBottom: 20,
  },
  badge: {
    paddingHorizontal: 10,
    paddingVertical: 4,
    borderRadius: 12,
  },
  badgeText: {
    fontSize: 13,
    fontWeight: '600',
  },
  section: {
    marginBottom: 16,
  },
  row: {
    flexDirection: 'row',
    marginBottom: 16,
  },
  column: {
    flex: 1,
  },
  label: {
    fontSize: 13,
    color: '#8E8E93',
    marginBottom: 4,
    textTransform: 'uppercase',
  },
  value: {
    fontSize: 17,
    color: '#000000',
  },
  editButton: {
    color: '#007AFF',
    fontSize: 17,
    fontWeight: '600',
  },
  dropdownButton: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    backgroundColor: '#FFFFFF',
    padding: 16,
    borderRadius: 10,
    marginTop: 8,
  },
  dropdownButtonText: {
    fontSize: 17,
    color: '#000000',
  },
  dropdownList: {
    backgroundColor: '#FFFFFF',
    borderRadius: 10,
    marginTop: 8,
    overflow: 'hidden',
  },
  dropdownItem: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: 16,
    borderBottomWidth: StyleSheet.hairlineWidth,
    borderBottomColor: '#C6C6C8',
  },
  dropdownItemText: {
    fontSize: 17,
    color: '#000000',
  },
  deleteButton: {
    marginTop: 24,
    backgroundColor: '#FF3B30',
  },
  deleteButtonText: {
    color: '#FFFFFF',
  },
});

export default TicketDetailScreen;
