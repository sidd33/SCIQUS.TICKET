import React from 'react';
import { View, Text, StyleSheet, Image, TouchableOpacity } from 'react-native';

interface ProfileAvatarProps {
  firstName: string;
  lastName: string;
  photoUrl?: string | null;
  size?: number;
  onPress?: () => void;
}

const ProfileAvatar: React.FC<ProfileAvatarProps> = ({ 
  firstName, 
  lastName, 
  photoUrl, 
  size = 60,
  onPress
}) => {
  const getInitials = () => {
    const firstInitial = firstName ? firstName.charAt(0).toUpperCase() : '';
    const lastInitial = lastName ? lastName.charAt(0).toUpperCase() : '';
    return `${firstInitial}${lastInitial}` || '?';
  };

  const Container = onPress ? TouchableOpacity : View;

  return (
    <Container 
      style={[
        styles.container, 
        { width: size, height: size, borderRadius: size / 2 }
      ]}
      onPress={onPress}
      activeOpacity={0.8}
    >
      {photoUrl ? (
        <Image 
          source={{ uri: photoUrl }} 
          style={{ width: size, height: size, borderRadius: size / 2 }} 
        />
      ) : (
        <View style={[styles.initialsContainer, { width: size, height: size, borderRadius: size / 2 }]}>
          <Text style={[styles.initialsText, { fontSize: size * 0.4 }]}>
            {getInitials()}
          </Text>
        </View>
      )}
    </Container>
  );
};

const styles = StyleSheet.create({
  container: {
    backgroundColor: '#E5E5EA',
    justifyContent: 'center',
    alignItems: 'center',
    overflow: 'hidden',
  },
  initialsContainer: {
    backgroundColor: '#007AFF', // Apple Blue
    justifyContent: 'center',
    alignItems: 'center',
  },
  initialsText: {
    color: '#FFFFFF',
    fontWeight: '600',
  }
});

export default ProfileAvatar;
