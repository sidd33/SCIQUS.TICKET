import React, { useState } from 'react';
import { View, Text, StyleSheet, Alert, SafeAreaView, KeyboardAvoidingView, Platform, ScrollView, TouchableOpacity, ActivityIndicator } from 'react-native';
import AppleTextInput from '../components/AppleTextInput';
import AppleButton from '../components/AppleButton';
import TicketService from '../services/ticketService';
import departmentService from '../services/departmentService';
import { ChevronDown, Check } from 'lucide-react-native';

interface Props {
  navigation: any;
}

const PRIORITIES = [
  { id: 1, label: 'Low', color: '#34C759' },
  { id: 2, label: 'Medium', color: '#FFCC00' },
  { id: 3, label: 'High', color: '#FF9500' },
  { id: 4, label: 'Critical', color: '#FF3B30' },
];

const CustomerTicketCreateScreen: React.FC<Props> = ({ navigation }) => {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState<number>(1);
  const [departmentId, setDepartmentId] = useState<string>('');
  const [departments, setDepartments] = useState<any[]>([]);
  const [showPriorityDropdown, setShowPriorityDropdown] = useState(false);
  const [showDeptDropdown, setShowDeptDropdown] = useState(false);
  const [loading, setLoading] = useState(false);

  React.useEffect(() => {
    departmentService.getAll().then(res => {
      setDepartments(res.items);
      if (res.items.length > 0) {
        setDepartmentId(res.items[0].id);
      }
    }).catch(console.error);
  }, []);

  const handleCreate = async () => {
    if (!title.trim() || !description.trim() || !departmentId) {
      Alert.alert('Error', 'Please enter a title, description, and select a department.');
      return;
    }

    setLoading(true);
    try {
      await TicketService.create({
        title,
        description,
        departmentId,
        priority,
      });
      navigation.goBack();
    } catch (error) {
      console.error('Failed to create ticket:', error);
      Alert.alert('Error', 'Failed to create ticket. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const selectedPriority = PRIORITIES.find(p => p.id === priority);

  return (
    <SafeAreaView style={styles.safeArea}>
      <KeyboardAvoidingView 
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        style={styles.container}
      >
        <ScrollView contentContainerStyle={styles.content}>
          <Text style={styles.sectionTitle}>What issue are you facing?</Text>
          
          <AppleTextInput
            placeholder="Brief Title"
            value={title}
            onChangeText={setTitle}
          />
          
          <AppleTextInput
            placeholder="Detailed Description"
            value={description}
            onChangeText={setDescription}
            multiline
            numberOfLines={4}
            textAlignVertical="top"
            style={styles.textArea}
          />

          <View style={styles.dropdownContainer}>
            <Text style={styles.label}>Which Department?</Text>
            <TouchableOpacity 
              style={styles.dropdownButton}
              onPress={() => setShowDeptDropdown(!showDeptDropdown)}
            >
              <Text style={styles.dropdownButtonText}>
                {departments.find(d => d.id === departmentId)?.name || 'Select Department'}
              </Text>
              <ChevronDown color="#8E8E93" size={20} />
            </TouchableOpacity>

            {showDeptDropdown && (
              <View style={styles.dropdownList}>
                {departments.map((d) => (
                  <TouchableOpacity
                    key={d.id}
                    style={styles.dropdownItem}
                    onPress={() => {
                      setDepartmentId(d.id);
                      setShowDeptDropdown(false);
                    }}
                  >
                    <Text style={styles.dropdownItemText}>{d.name}</Text>
                    {departmentId === d.id && <Check color="#007AFF" size={20} />}
                  </TouchableOpacity>
                ))}
              </View>
            )}
          </View>

          <View style={styles.dropdownContainer}>
            <Text style={styles.label}>How urgent is this?</Text>
            <TouchableOpacity 
              style={styles.dropdownButton}
              onPress={() => setShowPriorityDropdown(!showPriorityDropdown)}
            >
              <View style={styles.dropdownValue}>
                <View style={[styles.priorityDot, { backgroundColor: selectedPriority?.color }]} />
                <Text style={styles.dropdownButtonText}>{selectedPriority?.label}</Text>
              </View>
              <ChevronDown color="#8E8E93" size={20} />
            </TouchableOpacity>

            {showPriorityDropdown && (
              <View style={styles.dropdownList}>
                {PRIORITIES.map((p) => (
                  <TouchableOpacity
                    key={p.id}
                    style={styles.dropdownItem}
                    onPress={() => {
                      setPriority(p.id);
                      setShowPriorityDropdown(false);
                    }}
                  >
                    <View style={styles.dropdownValue}>
                      <View style={[styles.priorityDot, { backgroundColor: p.color }]} />
                      <Text style={styles.dropdownItemText}>{p.label}</Text>
                    </View>
                    {priority === p.id && <Check color="#007AFF" size={20} />}
                  </TouchableOpacity>
                ))}
              </View>
            )}
          </View>
          
          <View style={styles.buttonContainer}>
            {loading ? (
              <ActivityIndicator size="large" color="#007AFF" />
            ) : (
              <AppleButton title="Submit Ticket" onPress={handleCreate} />
            )}
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  safeArea: { flex: 1, backgroundColor: '#F2F2F7' },
  container: { flex: 1 },
  content: { padding: 16, gap: 16 },
  sectionTitle: { fontSize: 20, fontWeight: '600', color: '#000000', marginBottom: 8 },
  textArea: { 
    height: 100, 
    paddingTop: 14,
    paddingBottom: 14,
    paddingHorizontal: 16,
    backgroundColor: '#FFFFFF',
    borderRadius: 12,
    borderWidth: 1,
    borderColor: '#E5E5EA',
    fontSize: 17,
    color: '#000000',
  },
  dropdownContainer: { gap: 8 },
  label: { fontSize: 13, textTransform: 'uppercase', color: '#8E8E93', paddingLeft: 16, marginTop: 8 },
  dropdownButton: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    backgroundColor: '#FFFFFF',
    padding: 16,
    borderRadius: 10,
  },
  dropdownValue: { flexDirection: 'row', alignItems: 'center', gap: 8 },
  priorityDot: { width: 12, height: 12, borderRadius: 6 },
  dropdownButtonText: { fontSize: 17, color: '#000000' },
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
  dropdownItemText: { fontSize: 17, color: '#000000' },
  buttonContainer: { marginTop: 24, paddingHorizontal: 16 },
});

export default CustomerTicketCreateScreen;
