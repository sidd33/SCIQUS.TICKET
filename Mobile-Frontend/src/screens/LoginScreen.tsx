import React, { useState } from 'react';
import { View, Text, StyleSheet, Alert, SafeAreaView, KeyboardAvoidingView, Platform, ActivityIndicator, TouchableOpacity } from 'react-native';
import { authService } from '../services/authService';
import AppleTextInput from '../components/AppleTextInput';
import AppleButton from '../components/AppleButton';
import { useNavigation } from '@react-navigation/native';

interface LoginScreenProps {
  onLoginSuccess: () => void;
}

const LoginScreen: React.FC<LoginScreenProps> = ({ onLoginSuccess }) => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const navigation = useNavigation<any>();

  const handleLogin = async () => {
    if (!email || !password) {
      Alert.alert('Error', 'Please enter both email and password.');
      return;
    }

    setLoading(true);
    try {
      await authService.login(email, password);
      onLoginSuccess();
    } catch (error: any) {
      Alert.alert('Login Failed', error.response?.data?.message || 'Please check your credentials and try again.');
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
          <Text style={styles.title}>Welcome Back</Text>
          <Text style={styles.subtitle}>Sign in to continue</Text>

          <View style={styles.form}>
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
              autoComplete="password"
            />
            
            <View style={styles.buttonContainer}>
              {loading ? (
                <ActivityIndicator size="large" color="#007AFF" />
              ) : (
                <AppleButton title="Sign In" onPress={handleLogin} />
              )}
            </View>
            
            <TouchableOpacity onPress={() => (navigation as any).navigate('CustomerRegister')} style={styles.linkContainer}>
              <Text style={styles.linkText}>Are you a customer? Register here</Text>
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
    marginBottom: 48,
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

export default LoginScreen;
