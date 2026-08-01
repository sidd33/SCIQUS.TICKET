import React, { useEffect, useState } from 'react';
import { View, StyleSheet, SafeAreaView, ScrollView, Alert, KeyboardAvoidingView, Platform } from 'react-native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { RootStackParamList } from '../../App';
import EmployeeService from '../services/employeeService';
import CustomerService from '../services/customerService';
import AppleTextInput from '../components/AppleTextInput';
import AppleButton from '../components/AppleButton';
import ProfileAvatar from '../components/ProfileAvatar';
import ImagePicker from 'react-native-image-crop-picker';
import { authService, authEmitter } from '../services/authService';

type ProfileEditScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, 'ProfileEdit'>;

interface Props {
  navigation: ProfileEditScreenNavigationProp;
}

const ProfileEditScreen: React.FC<Props> = ({ navigation }) => {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [photoUrl, setPhotoUrl] = useState<string | null>(null);
  const [photoFile, setPhotoFile] = useState<{ uri: string, mime: string } | null>(null);
  const [loading, setLoading] = useState(false);
  const [currentUser, setCurrentUser] = useState<any>(null);

  useEffect(() => {
    const fetchUser = async () => {
      const user = await authService.getUser();
      setCurrentUser(user);
      
      if (user) {
        if (user.role === 0) {
          // Customer
          try {
            const data = await CustomerService.getById(user.id);
            setFirstName(data.name.split(' ')[0] || '');
            setLastName(data.name.split(' ').slice(1).join(' ') || '');
            setEmail(data.email);
            setPhotoUrl(data.profilePicture || null);
          } catch (e) {}
        } else {
          // Employee
          try {
            const data = await EmployeeService.getById(user.id);
            setFirstName(data.firstName);
            setLastName(data.lastName);
            setEmail(data.email);
            setPhotoUrl(data.photoUrl || null);
          } catch (e) {}
        }
      }
    };
    fetchUser();
  }, []);

  const handlePhotoSelect = async () => {
    try {
      const image = await ImagePicker.openPicker({
        width: 400,
        height: 400,
        cropping: true,
        cropperCircleOverlay: true,
      });
      if (image.path) {
        setPhotoUrl(image.path); // for immediate display
        setPhotoFile({ uri: image.path, mime: image.mime });
      }
    } catch (error) {
      if ((error as any).message !== 'User cancelled image selection') {
        Alert.alert('Error', 'Failed to pick image');
      }
    }
  };

  const handleSave = async () => {
    if (!firstName || !email) {
      Alert.alert('Validation Error', 'First Name and Email are required.');
      return;
    }

    setLoading(true);
    try {
      let updatedPhotoUrl = photoUrl;

      if (currentUser.role === 0) {
        // Update Customer
        if (photoFile) {
          updatedPhotoUrl = await CustomerService.uploadProfilePicture(currentUser.id, photoFile.uri, photoFile.mime);
        }
        await CustomerService.update(currentUser.id, {
          name: `${firstName} ${lastName}`.trim(),
          email,
          password: password ? password : undefined,
        });
      } else {
        // Update Employee
        if (photoFile) {
          updatedPhotoUrl = await EmployeeService.uploadProfilePicture(currentUser.id, photoFile.uri, photoFile.mime);
        }
        await EmployeeService.update(currentUser.id, {
          firstName,
          lastName,
          email,
          // Role and Department are deliberately omitted so they cannot be changed here
        });
      }
      
      // Emit an event so the Profile page can refetch user details/photo instantly
      authEmitter.emit('profile_updated');
      
      Alert.alert('Success', 'Profile updated successfully!', [
        { text: 'OK', onPress: () => navigation.goBack() }
      ]);
    } catch (error) {
      Alert.alert('Error', 'Failed to update profile');
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
              size={120} 
              onPress={handlePhotoSelect} 
            />
          </View>

          <View style={styles.formGroup}>
            <AppleTextInput
              label="First Name"
              value={firstName}
              onChangeText={setFirstName}
            />
            <AppleTextInput
              label="Last Name"
              value={lastName}
              onChangeText={setLastName}
            />
            <AppleTextInput
              label="Email"
              value={email}
              onChangeText={setEmail}
              keyboardType="email-address"
              autoCapitalize="none"
              editable={false} // Prevent changing email to avoid token issues
            />
            {currentUser?.role === 0 && (
              <AppleTextInput
                label="New Password (Optional)"
                value={password}
                onChangeText={setPassword}
                secureTextEntry
                autoCapitalize="none"
                placeholder="Leave blank to keep current"
              />
            )}
          </View>

          <AppleButton
            title="Save Profile"
            onPress={handleSave}
            isLoading={loading}
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
    marginVertical: 24,
  },
  formGroup: {
    marginBottom: 24,
  },
});

export default ProfileEditScreen;
