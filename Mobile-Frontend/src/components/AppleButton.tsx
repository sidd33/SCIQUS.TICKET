import React from 'react';
import { TouchableOpacity, Text, StyleSheet, ViewStyle, TextStyle, ActivityIndicator } from 'react-native';

interface AppleButtonProps {
  title: string;
  onPress: () => void;
  variant?: 'primary' | 'secondary' | 'danger';
  style?: ViewStyle;
  textStyle?: TextStyle;
  isLoading?: boolean;
  disabled?: boolean;
}

const AppleButton: React.FC<AppleButtonProps> = ({
  title,
  onPress,
  variant = 'primary',
  style,
  textStyle,
  isLoading = false,
  disabled = false,
}) => {
  const getBackgroundColor = () => {
    if (disabled) return '#E5E5EA';
    switch (variant) {
      case 'primary': return '#007AFF';
      case 'secondary': return '#F2F2F7';
      case 'danger': return '#FF3B30';
      default: return '#007AFF';
    }
  };

  const getTextColor = () => {
    if (disabled) return '#8E8E93';
    switch (variant) {
      case 'primary': return '#FFFFFF';
      case 'secondary': return '#007AFF';
      case 'danger': return '#FFFFFF';
      default: return '#FFFFFF';
    }
  };

  return (
    <TouchableOpacity
      style={[
        styles.button,
        { backgroundColor: getBackgroundColor() },
        style,
      ]}
      onPress={onPress}
      disabled={disabled || isLoading}
      activeOpacity={0.8}
    >
      {isLoading ? (
        <ActivityIndicator color={getTextColor()} />
      ) : (
        <Text style={[styles.text, { color: getTextColor() }, textStyle]}>
          {title}
        </Text>
      )}
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  button: {
    height: 50,
    borderRadius: 14,
    justifyContent: 'center',
    alignItems: 'center',
    marginVertical: 8,
    paddingHorizontal: 20,
  },
  text: {
    fontSize: 17,
    fontWeight: '600',
    letterSpacing: -0.4,
  },
});

export default AppleButton;
