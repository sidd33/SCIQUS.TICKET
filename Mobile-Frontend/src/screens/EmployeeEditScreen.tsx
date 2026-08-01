import React, { useEffect, useState } from 'react';
import { View, StyleSheet, SafeAreaView, ScrollView, Alert, KeyboardAvoidingView, Platform, Switch, Text, TouchableOpacity, Modal, FlatList } from 'react-native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { RouteProp } from '@react-navigation/native';
import { RootStackParamList } from '../../App';
import EmployeeService from '../services/employeeService';
import AppleTextInput from '../components/AppleTextInput';
import AppleButton from '../components/AppleButton';
import ProfileAvatar from '../components/ProfileAvatar';
import ImagePicker from 'react-native-image-crop-picker';

type EmployeeEditScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, 'EmployeeEdit'>;
type EmployeeEditScreenRouteProp = RouteProp<RootStackParamList, 'EmployeeEdit'>;

interface Props {
  navigation: EmployeeEditScreenNavigationProp;
  route: EmployeeEditScreenRouteProp;
}

const EmployeeEditScreen: React.FC<Props> = ({ navigation, route }) => {
  const isEditing = !!route.params.id;
  const id = route.params.id;

  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [department, setDepartment] = useState('');
  const [role, setRole] = useState(1);
  const [isActive, setIsActive] = useState(true);
  const [photoUrl, setPhotoUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [currentUser, setCurrentUser] = useState<any>(null);
  const [isRolePickerVisible, setRolePickerVisible] = useState(false);

  const roles = [
    { id: 1, name: 'Employee' },
    { id: 2, name: 'Admin' },
    { id: 3, name: 'HR Manager' },
    { id: 4, name: 'IT Support' }
  ];

  const handlePhotoSelect = async () => {
    try {
      const image = await ImagePicker.openPicker({
        width: 400,
        height: 400,
        cropping: true,
        cropperCircleOverlay: true,
        includeBase64: true,
      });
      if (image.data) {
        setPhotoUrl(`data:${image.mime};base64,${image.data}`);
      }
    } catch (error) {
      if ((error as any).message !== 'User cancelled image selection') {
        Alert.alert('Error', 'Failed to pick image');
      }
    }
  };

  useEffect(() => {
    const fetchUser = async () => {
      const user = await import('../services/authService').then(m => m.authService.getUser());
      setCurrentUser(user);
    };
    fetchUser();
    
    navigation.setOptions({ title: isEditing ? 'Edit Employee' : 'New Employee' });

    if (isEditing && id) {
      fetchEmployee(id);
    }
  }, [isEditing, id]);

  const fetchEmployee = async (employeeId: string) => {
    try {
      const data = await EmployeeService.getById(employeeId);
      setFirstName(data.firstName);
      setLastName(data.lastName);
      setEmail(data.email);
      setDepartment(data.department?.name || '');
      setRole(data.role);
      setIsActive(data.isActive);
      if (data.photoUrl) setPhotoUrl(data.photoUrl);
    } catch (error) {
      Alert.alert('Error', 'Failed to fetch employee details');
      navigation.goBack();
    }
  };

  const handleSave = async () => {
    if (!firstName || !lastName || !email) {
      Alert.alert('Validation Error', 'First Name, Last Name, and Email are required.');
      return;
    }
    
    if (!isEditing && (!password || password.length < 6)) {
      Alert.alert('Validation Error', 'Password is required and must be at least 6 characters.');
      return;
    }

    const validDepartments = ['Software', 'Sales', 'Marketing', 'HR', 'Finance', 'Operations'];
    let finalDept: string | undefined = undefined;
    
    if (department.trim() !== '') {
      const matched = validDepartments.find(d => d.toLowerCase() === department.trim().toLowerCase());
      if (!matched) {
        Alert.alert('Validation Error', `Department must be one of:\n${validDepartments.join(', ')}`);
        return;
      }
      finalDept = matched;
    }

    setLoading(true);
    try {
      if (isEditing && id) {
        await EmployeeService.update(id, {
          firstName,
          lastName,
          email,
          department: finalDept,
          role,
          isActive,
          photoUrl: photoUrl || undefined,
        });
      } else {
        await EmployeeService.create({
          firstName,
          lastName,
          email,
          password,
          department: finalDept,
          role,
          photoUrl: photoUrl || undefined,
        });
      }
      navigation.goBack();
    } catch (error) {
      Alert.alert('Error', `Failed to ${isEditing ? 'update' : 'create'} employee`);
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
          <View style={styles.avatarContainer}>
            <ProfileAvatar 
              firstName={firstName} 
              lastName={lastName} 
              photoUrl={photoUrl} 
              size={100} 
              onPress={handlePhotoSelect} 
            />
            <Text style={styles.avatarHint}>Tap to change photo</Text>
          </View>

          <View style={styles.formGroup}>
            <AppleTextInput
              label="First Name"
              placeholder="John"
              value={firstName}
              onChangeText={setFirstName}
            />
            <AppleTextInput
              label="Last Name"
              placeholder="Doe"
              value={lastName}
              onChangeText={setLastName}
            />
            <AppleTextInput
              label="Email"
              placeholder="john.doe@example.com"
              value={email}
              onChangeText={setEmail}
              keyboardType="email-address"
              autoCapitalize="none"
            />
            {!isEditing && (
              <AppleTextInput
                label="Password"
                placeholder="Required (min 6 chars)"
                value={password}
                onChangeText={setPassword}
                secureTextEntry
                autoCapitalize="none"
              />
            )}
            <AppleTextInput
              label="Department (Optional)"
              placeholder="Engineering"
              value={department}
              onChangeText={setDepartment}
            />
            
            {currentUser && currentUser.role === 2 && (
              <View>
                <Text style={styles.inputLabel}>Role</Text>
                <TouchableOpacity 
                  style={styles.pickerButton} 
                  onPress={() => setRolePickerVisible(true)}
                >
                  <Text style={styles.pickerButtonText}>
                    {roles.find(r => r.id === role)?.name || 'Select Role'}
                  </Text>
                </TouchableOpacity>

                <Modal
                  visible={isRolePickerVisible}
                  transparent
                  animationType="slide"
                  onRequestClose={() => setRolePickerVisible(false)}
                >
                  <View style={styles.modalOverlay}>
                    <View style={styles.modalContent}>
                      <View style={styles.modalHeader}>
                        <Text style={styles.modalTitle}>Select Role</Text>
                        <TouchableOpacity onPress={() => setRolePickerVisible(false)}>
                          <Text style={styles.modalDone}>Done</Text>
                        </TouchableOpacity>
                      </View>
                      <FlatList
                        data={roles}
                        keyExtractor={item => item.id.toString()}
                        renderItem={({ item }) => (
                          <TouchableOpacity 
                            style={styles.modalItem}
                            onPress={() => {
                              setRole(item.id);
                              setRolePickerVisible(false);
                            }}
                          >
                            <Text style={styles.modalItemText}>{item.name}</Text>
                            {role === item.id && <Text style={styles.modalItemCheck}>✓</Text>}
                          </TouchableOpacity>
                        )}
                      />
                    </View>
                  </View>
                </Modal>
              </View>
            )}

            {isEditing && currentUser && currentUser.role === 2 && (
              <View style={styles.switchRow}>
                <Text style={styles.switchLabel}>Active Account</Text>
                <Switch
                  value={isActive}
                  onValueChange={setIsActive}
                  trackColor={{ false: '#E5E5EA', true: '#34C759' }}
                />
              </View>
            )}
          </View>

          <AppleButton
            title={isEditing ? 'Save Changes' : 'Create Employee'}
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
  avatarContainer: {
    alignItems: 'center',
    marginVertical: 20,
  },
  avatarHint: {
    marginTop: 8,
    color: '#007AFF',
    fontSize: 13,
  },
  formGroup: {
    marginBottom: 24,
  },
  switchRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    backgroundColor: '#FFFFFF',
    padding: 16,
    borderRadius: 12,
    marginTop: 10,
    borderWidth: 1,
    borderColor: '#E5E5EA',
  },
  switchLabel: {
    fontSize: 17,
    color: '#000000',
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

export default EmployeeEditScreen;
