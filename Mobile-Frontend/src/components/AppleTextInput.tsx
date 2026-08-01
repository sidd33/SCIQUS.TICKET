import React from 'react';
import { TextInput, StyleSheet, View, Text, TextInputProps } from 'react-native';

interface AppleTextInputProps extends TextInputProps {
  label?: string;
  error?: string;
}

const AppleTextInput: React.FC<AppleTextInputProps> = ({ label, error, ...props }) => {
  return (
    <View style={styles.container}>
      {!!label && <Text style={styles.label}>{label}</Text>}
      <TextInput
        style={[styles.input, error ? styles.inputError : null, props.style]}
        placeholderTextColor="#C7C7CC"
        {...props}
      />
      {!!error && <Text style={styles.errorText}>{error}</Text>}
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    marginVertical: 10,
  },
  label: {
    fontSize: 13,
    color: '#8E8E93',
    textTransform: 'uppercase',
    marginBottom: 6,
    marginLeft: 12,
    fontWeight: '500',
    letterSpacing: -0.1,
  },
  input: {
    backgroundColor: '#FFFFFF',
    height: 50,
    borderRadius: 12,
    paddingHorizontal: 16,
    fontSize: 17,
    color: '#000000',
    borderWidth: 1,
    borderColor: '#E5E5EA',
  },
  inputError: {
    borderColor: '#FF3B30',
  },
  errorText: {
    color: '#FF3B30',
    fontSize: 13,
    marginTop: 6,
    marginLeft: 12,
  },
});

export default AppleTextInput;
