import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity } from 'react-native';
import { ChevronRight, Briefcase } from 'lucide-react-native';
import { Employee } from '../services/employeeService';
import ProfileAvatar from './ProfileAvatar';

interface EmployeeListItemProps {
  employee: Employee;
  onPress: () => void;
}

const EmployeeListItem: React.FC<EmployeeListItemProps> = ({ employee, onPress }) => {
  return (
    <TouchableOpacity style={styles.container} onPress={onPress} activeOpacity={0.7}>
      <View style={{ marginRight: 12 }}>
        <ProfileAvatar 
          firstName={employee.firstName} 
          lastName={employee.lastName} 
          photoUrl={employee.photoUrl} 
          size={48} 
        />
      </View>
      <View style={styles.info}>
        <Text style={styles.name}>{employee.firstName} {employee.lastName}</Text>
        {!!employee.department && (
          <View style={styles.departmentRow}>
            <Briefcase size={14} color="#8E8E93" />
            <Text style={styles.department}>{employee.department.name}</Text>
          </View>
        )}
      </View>
      <View style={styles.accessory}>
        {!employee.isActive && (
          <View style={styles.inactiveBadge}>
            <Text style={styles.inactiveText}>Inactive</Text>
          </View>
        )}
        <ChevronRight size={20} color="#C7C7CC" />
      </View>
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 12,
    paddingHorizontal: 16,
    backgroundColor: '#FFFFFF',
    borderBottomWidth: StyleSheet.hairlineWidth,
    borderBottomColor: '#C6C6C8',
  },
  avatar: {
    width: 48,
    height: 48,
    borderRadius: 24,
    backgroundColor: '#E5E5EA',
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: 12,
  },
  avatarText: {
    fontSize: 18,
    fontWeight: '600',
    color: '#3A3A3C',
  },
  info: {
    flex: 1,
    justifyContent: 'center',
  },
  name: {
    fontSize: 17,
    fontWeight: '500',
    color: '#000000',
    marginBottom: 4,
  },
  departmentRow: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  department: {
    fontSize: 14,
    color: '#8E8E93',
    marginLeft: 4,
  },
  accessory: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  inactiveBadge: {
    backgroundColor: '#FF3B30',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
    marginRight: 8,
  },
  inactiveText: {
    color: '#FFFFFF',
    fontSize: 11,
    fontWeight: '600',
  },
});

export default EmployeeListItem;
