import React, { useState } from 'react';
import { View, Text, StyleSheet, Alert, SafeAreaView, KeyboardAvoidingView, Platform, ActivityIndicator, TouchableOpacity } from 'react-native';
import AppleTextInput from '../components/AppleTextInput';
import AppleButton from '../components/AppleButton';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { authService } from '../services/authService';

interface CustomerRegisterScreenProps {
  navigation: any;
}

const CustomerRegisterScreen: React.FC<CustomerRegisterScreenProps> = ({ navigation }) => {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);

  const handleRegister = async () => {
    if (!firstName || !lastName || !email || !password) {
      Alert.alert('Error', 'Please fill in all fields.');
      return;
    }

    setLoading(true);
    try {
      await authService.register(firstName, lastName, email, password);
      Alert.alert('Success', 'Customer registered successfully. You can now login.');
      navigation.goBack();
    } catch (error: any) {
      Alert.alert('Registration Failed', error.response?.data?.message || 'Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <KeyboardAvoidingView 
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        style={styles.container}
      >
        <View style={styles.content}>
          <Text style={styles.title}>Create Account</Text>
          <Text style={styles.subtitle}>Register as a new Customer</Text>

          <View style={styles.form}>
            <AppleTextInput
              value={firstName}
              onChangeText={setFirstName}
              placeholder="First Name"
            />
            <AppleTextInput
              value={lastName}
              onChangeText={setLastName}
              placeholder="Last Name"
            />
            <AppleTextInput
              value={email}
              onChangeText={setEmail}
              placeholder="Email"
              keyboardType="email-address"
              autoCapitalize="none"
              autoComplete="email"
            />
            <AppleTextInput
              value={password}
              onChangeText={setPassword}
              placeholder="Password"
              secureTextEntry
            />
            
            <View style={styles.buttonContainer}>
              {loading ? (
                <ActivityIndicator size="large" color="#007AFF" />
              ) : (
                <AppleButton title="Register" onPress={handleRegister} />
              )}
            </View>
            
            <TouchableOpacity onPress={() => navigation.goBack()} style={styles.linkContainer}>
              <Text style={styles.linkText}>Already have an account? Sign In</Text>
            </TouchableOpacity>
          </View>
        </View>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: '#F2F2F7',
  },
  container: {
    flex: 1,
  },
  content: {
    flex: 1,
    justifyContent: 'center',
    paddingHorizontal: 24,
  },
  title: {
    fontSize: 34,
    fontWeight: '700',
    color: '#000000',
    marginBottom: 8,
    letterSpacing: 0.41,
  },
  subtitle: {
    fontSize: 17,
    color: '#8E8E93',
    marginBottom: 32,
  },
  form: {
    gap: 16,
  },
  buttonContainer: {
    marginTop: 16,
    height: 50,
    justifyContent: 'center',
  },
  linkContainer: {
    marginTop: 16,
    alignItems: 'center',
  },
  linkText: {
    color: '#007AFF',
    fontSize: 15,
    fontWeight: '500',
  }
});

export default CustomerRegisterScreen;
