import React, { useEffect, useState } from 'react';
import { View, StyleSheet, SafeAreaView, ScrollView, Alert, KeyboardAvoidingView, Platform, Text, TouchableOpacity, Modal, FlatList, ActivityIndicator } from 'react-native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { RouteProp } from '@react-navigation/native';
import { RootStackParamList } from '../../App';
import TicketService from '../services/ticketService';
import EmployeeService, { Employee } from '../services/employeeService';
import DepartmentService, { Department } from '../services/departmentService';
import AppleTextInput from '../components/AppleTextInput';
import AppleButton from '../components/AppleButton';

type TicketEditScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, 'TicketEdit'>;
type TicketEditScreenRouteProp = RouteProp<RootStackParamList, 'TicketEdit'>;

interface Props {
  navigation: TicketEditScreenNavigationProp;
  route: TicketEditScreenRouteProp;
}

const TicketEditScreen: React.FC<Props> = ({ navigation, route }) => {
  const isEditing = !!route.params.id;
  const id = route.params.id;

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState(1);
  const [status, setStatus] = useState(1);
  const [assignedEmployeeId, setAssignedEmployeeId] = useState<string | null>(null);
  const [originalAssigneeId, setOriginalAssigneeId] = useState<string | null>(null);
  const [departmentId, setDepartmentId] = useState<string>('');
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [isAssigneePickerVisible, setAssigneePickerVisible] = useState(false);
  const [isPriorityPickerVisible, setPriorityPickerVisible] = useState(false);
  const [isStatusPickerVisible, setStatusPickerVisible] = useState(false);
  const [isDepartmentPickerVisible, setDepartmentPickerVisible] = useState(false);
  const [loading, setLoading] = useState(false);
  const [loadingEmployees, setLoadingEmployees] = useState(false);
  const [loadingDepartments, setLoadingDepartments] = useState(false);

  const priorities = [
    { id: 1, label: 'Low' },
    { id: 2, label: 'Medium' },
    { id: 3, label: 'High' },
    { id: 4, label: 'Critical' },
  ];

  const statuses = [
    { id: 1, label: 'Not Assigned' },
    { id: 2, label: 'Currently Assigned' },
    { id: 3, label: 'In Progress' },
    { id: 4, label: 'Resolved' },
    { id: 5, label: 'Closed' },
  ];

  useEffect(() => {
    navigation.setOptions({ title: isEditing ? 'Edit Ticket' : 'New Ticket' });

    fetchEmployees();
    fetchDepartments();

    if (isEditing && id) {
      fetchTicket(id);
    }
  }, [isEditing, id]);

  const fetchEmployees = async () => {
    try {
      setLoadingEmployees(true);
      const response = await EmployeeService.getAll();
      setEmployees(response.items || []);
    } catch (error) {
      console.warn('Failed to fetch employees');
    } finally {
      setLoadingEmployees(false);
    }
  };

  const fetchDepartments = async () => {
    try {
      setLoadingDepartments(true);
      const response = await DepartmentService.getAll();
      setDepartments(response.items || []);
      if (!isEditing && response.items && response.items.length > 0) {
        setDepartmentId(response.items[0].id);
      }
    } catch (error) {
      console.warn('Failed to fetch departments');
    } finally {
      setLoadingDepartments(false);
    }
  };

  const fetchTicket = async (ticketId: string) => {
    try {
      const data = await TicketService.getById(ticketId);
      setTitle(data.title);
      setDescription(data.description);
      setPriority(data.priority);
      setStatus(data.status);
      if (data.department?.id) {
        setDepartmentId(data.department.id);
      }
      if (data.assignedEmployeeId) {
        setAssignedEmployeeId(data.assignedEmployeeId);
        setOriginalAssigneeId(data.assignedEmployeeId);
      }
    } catch (error) {
      Alert.alert('Error', 'Failed to fetch ticket details');
      navigation.goBack();
    }
  };

  const handleSave = async () => {
    if (!title || !description || (!isEditing && !departmentId)) {
      Alert.alert('Validation Error', 'Title, Description, and Department are required.');
      return;
    }

    setLoading(true);
    try {
      if (isEditing && id) {
        await TicketService.update(id, {
          title,
          description,
          priority: priority,
          status: status,
          assignedToId: assignedEmployeeId || undefined,
        });
        
        if (assignedEmployeeId && assignedEmployeeId !== originalAssigneeId) {
          await TicketService.assignTicket(id, assignedEmployeeId);
        }
      } else {
        await TicketService.create({
          title,
          description,
          priority: priority,
          departmentId: departmentId,
          assignedToId: assignedEmployeeId || undefined,
        });
      }
      navigation.goBack();
    } catch (error) {
      console.error(error);
      Alert.alert('Error', `Failed to ${isEditing ? 'update' : 'create'} ticket`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      <KeyboardAvoidingView 
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        style={{ flex: 1 }}
      >
        <ScrollView contentContainerStyle={styles.scrollContent} keyboardShouldPersistTaps="handled">
          <View style={styles.formGroup}>
            <AppleTextInput
              label="Title"
              placeholder="Enter ticket title"
              value={title}
              onChangeText={setTitle}
            />
            <AppleTextInput
              label="Description"
              placeholder="Describe the issue..."
              value={description}
              onChangeText={setDescription}
              multiline
              numberOfLines={4}
              textAlignVertical="top"
              style={{
                height: 100, 
                paddingTop: 14,
                paddingBottom: 14,
                paddingHorizontal: 16,
                fontSize: 17,
                color: '#000000',
              }}
            />

            {!isEditing && (
              <View>
                <Text style={styles.inputLabel}>Department</Text>
                <TouchableOpacity 
                  style={styles.pickerButton} 
                  onPress={() => setDepartmentPickerVisible(true)}
                >
                  <Text style={styles.pickerButtonText}>
                    {departments.find(d => d.id === departmentId)?.name || 'Select Department'}
                  </Text>
                </TouchableOpacity>

                <Modal
                  visible={isDepartmentPickerVisible}
                  transparent
                  animationType="slide"
                  onRequestClose={() => setDepartmentPickerVisible(false)}
                >
                  <View style={styles.modalOverlay}>
                    <View style={styles.modalContent}>
                      <View style={styles.modalHeader}>
                        <Text style={styles.modalTitle}>Select Department</Text>
                        <TouchableOpacity onPress={() => setDepartmentPickerVisible(false)}>
                          <Text style={styles.modalCloseText}>Done</Text>
                        </TouchableOpacity>
                      </View>
                      {loadingDepartments ? (
                        <ActivityIndicator style={{ padding: 20 }} />
                      ) : (
                        <FlatList
                          data={departments}
                          keyExtractor={(item) => item.id}
                          renderItem={({ item }) => (
                            <TouchableOpacity
                              style={styles.modalItem}
                              onPress={() => {
                                setDepartmentId(item.id);
                                setDepartmentPickerVisible(false);
                              }}
                            >
                              <Text style={[
                                styles.modalItemText,
                                departmentId === item.id && styles.modalItemTextSelected
                              ]}>
                                {item.name}
                              </Text>
                            </TouchableOpacity>
                          )}
                        />
                      )}
                    </View>
                  </View>
                </Modal>
              </View>
            )}

            <View>
              <Text style={styles.inputLabel}>Priority</Text>
              <TouchableOpacity 
                style={styles.pickerButton} 
                onPress={() => setPriorityPickerVisible(true)}
              >
                <Text style={styles.pickerButtonText}>
                  {priorities.find(p => p.id === priority)?.label || 'Select Priority'}
                </Text>
              </TouchableOpacity>

              <Modal
                visible={isPriorityPickerVisible}
                transparent
                animationType="slide"
                onRequestClose={() => setPriorityPickerVisible(false)}
              >
                <View style={styles.modalOverlay}>
                  <View style={styles.modalContent}>
                    <View style={styles.modalHeader}>
                      <Text style={styles.modalTitle}>Select Priority</Text>
                      <TouchableOpacity onPress={() => setPriorityPickerVisible(false)}>
                        <Text style={styles.modalDone}>Done</Text>
                      </TouchableOpacity>
                    </View>
                    <FlatList
                      data={priorities}
                      keyExtractor={item => item.id.toString()}
                      renderItem={({ item }) => (
                        <TouchableOpacity 
                          style={styles.modalItem}
                          onPress={() => {
                            setPriority(item.id);
                            setPriorityPickerVisible(false);
                          }}
                        >
                          <Text style={styles.modalItemText}>{item.label}</Text>
                          {priority === item.id && <Text style={styles.modalItemCheck}>✓</Text>}
                        </TouchableOpacity>
                      )}
                    />
                  </View>
                </View>
              </Modal>
            </View>

            {isEditing && (
              <View>
                <Text style={styles.inputLabel}>Status</Text>
                <TouchableOpacity 
                  style={styles.pickerButton} 
                  onPress={() => setStatusPickerVisible(true)}
                >
                  <Text style={styles.pickerButtonText}>
                    {statuses.find(s => s.id === status)?.label || 'Select Status'}
                  </Text>
                </TouchableOpacity>

                <Modal
                  visible={isStatusPickerVisible}
                  transparent
                  animationType="slide"
                  onRequestClose={() => setStatusPickerVisible(false)}
                >
                  <View style={styles.modalOverlay}>
                    <View style={styles.modalContent}>
                      <View style={styles.modalHeader}>
                        <Text style={styles.modalTitle}>Select Status</Text>
                        <TouchableOpacity onPress={() => setStatusPickerVisible(false)}>
                          <Text style={styles.modalDone}>Done</Text>
                        </TouchableOpacity>
                      </View>
                      <FlatList
                        data={statuses}
                        keyExtractor={item => item.id.toString()}
                        renderItem={({ item }) => (
                          <TouchableOpacity 
                            style={styles.modalItem}
                            onPress={() => {
                              setStatus(item.id);
                              setStatusPickerVisible(false);
                            }}
                          >
                            <Text style={styles.modalItemText}>{item.label}</Text>
                            {status === item.id && <Text style={styles.modalItemCheck}>✓</Text>}
                          </TouchableOpacity>
                        )}
                      />
                    </View>
                  </View>
                </Modal>
              </View>
            )}

            <View>
              <Text style={styles.inputLabel}>Assignee</Text>
              <TouchableOpacity 
                style={styles.pickerButton} 
                onPress={() => setAssigneePickerVisible(true)}
              >
                <Text style={styles.pickerButtonText}>
                  {assignedEmployeeId 
                    ? employees.find(e => e.id === assignedEmployeeId)?.firstName + ' ' + employees.find(e => e.id === assignedEmployeeId)?.lastName 
                    : 'Unassigned'}
                </Text>
              </TouchableOpacity>

              <Modal
                visible={isAssigneePickerVisible}
                transparent
                animationType="slide"
                onRequestClose={() => setAssigneePickerVisible(false)}
              >
                <View style={styles.modalOverlay}>
                  <View style={styles.modalContent}>
                    <View style={styles.modalHeader}>
                      <Text style={styles.modalTitle}>Select Assignee</Text>
                      <TouchableOpacity onPress={() => setAssigneePickerVisible(false)}>
                        <Text style={styles.modalDone}>Done</Text>
                      </TouchableOpacity>
                    </View>
                    {loadingEmployees ? (
                      <ActivityIndicator size="large" color="#007AFF" style={{ margin: 20 }} />
                    ) : (
                      <FlatList
                        data={[{ id: null, firstName: 'Unassigned', lastName: '' } as any, ...employees]}
                        keyExtractor={item => item.id ? item.id.toString() : 'unassigned'}
                        renderItem={({ item }) => (
                          <TouchableOpacity 
                            style={styles.modalItem}
                            onPress={() => {
                              setAssignedEmployeeId(item.id);
                              setAssigneePickerVisible(false);
                            }}
                          >
                            <Text style={styles.modalItemText}>{item.firstName} {item.lastName}</Text>
                            {assignedEmployeeId === item.id && <Text style={styles.modalItemCheck}>✓</Text>}
                          </TouchableOpacity>
                        )}
                      />
                    )}
                  </View>
                </View>
              </Modal>
            </View>
          </View>

          <AppleButton
            title={isEditing ? 'Save Changes' : 'Create Ticket'}
            onPress={handleSave}
            isLoading={loading}
            style={styles.saveButton}
          />
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#F2F2F7',
  },
  scrollContent: {
    padding: 16,
  },
  formGroup: {
    marginBottom: 24,
  },
  saveButton: {
    marginTop: 16,
  },
  inputLabel: {
    fontSize: 15,
    color: '#8E8E93',
    marginTop: 12,
    marginBottom: 4,
    marginLeft: 4,
  },
  pickerButton: {
    backgroundColor: '#FFFFFF',
    padding: 14,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: '#E5E5EA',
  },
  pickerButtonText: {
    fontSize: 17,
    color: '#000000',
  },
  modalOverlay: {
    flex: 1,
    justifyContent: 'flex-end',
    backgroundColor: 'rgba(0,0,0,0.4)',
  },
  modalContent: {
    backgroundColor: '#FFFFFF',
    borderTopLeftRadius: 16,
    borderTopRightRadius: 16,
    paddingBottom: 30,
    maxHeight: '50%',
  },
  modalHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: 16,
    borderBottomWidth: 1,
    borderBottomColor: '#E5E5EA',
  },
  modalTitle: {
    fontSize: 18,
    fontWeight: '600',
  },
  modalDone: {
    fontSize: 17,
    color: '#007AFF',
    fontWeight: '600',
  },
  modalItem: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingVertical: 16,
    paddingHorizontal: 20,
    borderBottomWidth: 1,
    borderBottomColor: '#F2F2F7',
  },
  modalItemText: {
    fontSize: 17,
    color: '#000000',
  },
  modalItemCheck: {
    fontSize: 17,
    color: '#007AFF',
    fontWeight: 'bold',
  },
});

export default TicketEditScreen;
