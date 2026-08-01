import React, { useEffect, useState } from 'react';
import { ActivityIndicator, View } from 'react-native';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { LogOut, Users, Ticket as TicketIcon, User as UserIcon } from 'lucide-react-native';

import HomeScreen from './src/screens/HomeScreen';
import EmployeeDetailScreen from './src/screens/EmployeeDetailScreen';
import EmployeeEditScreen from './src/screens/EmployeeEditScreen';
import LoginScreen from './src/screens/LoginScreen';
import TicketListScreen from './src/screens/TicketListScreen';
import TicketDetailScreen from './src/screens/TicketDetailScreen';
import TicketEditScreen from './src/screens/TicketEditScreen';
import ProfileScreen from './src/screens/ProfileScreen';
import ProfileEditScreen from './src/screens/ProfileEditScreen';
import CustomerRegisterScreen from './src/screens/CustomerRegisterScreen';
import CustomerDashboardScreen from './src/screens/CustomerDashboardScreen';
import CustomerTicketCreateScreen from './src/screens/CustomerTicketCreateScreen';
import CustomerTicketDetailScreen from './src/screens/CustomerTicketDetailScreen';
import { authService, authEmitter } from './src/services/authService';

export type RootStackParamList = {
  Home: undefined;
  EmployeeDetail: { id: string };
  EmployeeEdit: { id?: string };
  TicketList: undefined;
  TicketDetail: { id: string };
  TicketEdit: { id?: string };
  ProfileEdit: undefined;
};

export type TabParamList = {
  EmployeesTab: undefined;
  TicketsTab: undefined;
  ProfileTab: undefined;
};

const Stack = createNativeStackNavigator<RootStackParamList>();
const Tab = createBottomTabNavigator<TabParamList>();

function EmployeeStack() {
  return (
    <Stack.Navigator
      initialRouteName="Home"
      screenOptions={{
        headerStyle: { backgroundColor: '#F2F2F7' },
        headerShadowVisible: false,
        headerTintColor: '#007AFF',
        headerTitleStyle: { fontWeight: '600', color: '#000000' },
        contentStyle: { backgroundColor: '#F2F2F7' }
      }}
    >
      <Stack.Screen 
        name="Home" 
        component={HomeScreen} 
        options={{ title: 'Employees', headerLargeTitle: true }} 
      />
      <Stack.Screen 
        name="EmployeeDetail" 
        component={EmployeeDetailScreen} 
        options={{ title: 'Details' }} 
      />
      <Stack.Screen 
        name="EmployeeEdit" 
        component={EmployeeEditScreen} 
        options={{ title: 'Edit Employee', presentation: 'modal' }} 
      />
    </Stack.Navigator>
  );
}

function TicketStack() {
  return (
    <Stack.Navigator
      screenOptions={{
        headerStyle: { backgroundColor: '#F2F2F7' },
        headerShadowVisible: false,
        headerTintColor: '#007AFF',
        headerTitleStyle: { fontWeight: '600', color: '#000000' },
        contentStyle: { backgroundColor: '#F2F2F7' }
      }}
    >
      <Stack.Screen 
        name="TicketList" 
        component={TicketListScreen} 
        options={{ title: 'Tickets', headerLargeTitle: true }} 
      />
      <Stack.Screen 
        name="TicketDetail" 
        component={TicketDetailScreen} 
        options={{ title: 'Ticket Details' }} 
      />
      <Stack.Screen 
        name="TicketEdit" 
        component={TicketEditScreen} 
        options={{ title: 'Edit Ticket', presentation: 'modal' }} 
      />
    </Stack.Navigator>
  );
}

export type ProfileStackParamList = {
  Profile: undefined;
  ProfileEdit: undefined;
};

const PStack = createNativeStackNavigator<ProfileStackParamList>();

function ProfileNavigator() {
  return (
    <PStack.Navigator
      screenOptions={{
        headerStyle: { backgroundColor: '#F2F2F7' },
        headerShadowVisible: false,
        headerTintColor: '#007AFF',
        headerTitleStyle: { fontWeight: '600', color: '#000000' },
        contentStyle: { backgroundColor: '#F2F2F7' }
      }}
    >
      <PStack.Screen 
        name="Profile" 
        component={ProfileScreen} 
        options={{ title: 'Profile', headerLargeTitle: true }} 
      />
      <PStack.Screen 
        name="ProfileEdit" 
        component={ProfileEditScreen} 
        options={{ title: 'Edit Profile', presentation: 'modal' }} 
      />
    </PStack.Navigator>
  );
}

function MainTabs({ handleLogout, userRole }: { handleLogout: () => void, userRole: number | null }) {
  return (
    <Tab.Navigator
      initialRouteName={userRole === 2 ? "ProfileTab" : "EmployeesTab"}
      screenOptions={{
        tabBarStyle: { backgroundColor: '#ffffff', borderTopWidth: 0, elevation: 0 },
        tabBarActiveTintColor: '#007AFF',
        tabBarInactiveTintColor: '#8E8E93',
        headerShown: false,
      }}
    >
      <Tab.Screen 
        name="EmployeesTab" 
        component={EmployeeStack} 
        options={{
          title: 'Employees',
          tabBarIcon: ({ color, size }) => <Users color={color} size={size} />
        }}
      />
      <Tab.Screen 
        name="TicketsTab" 
        component={TicketStack} 
        options={{
          title: 'Tickets',
          tabBarIcon: ({ color, size }) => <TicketIcon color={color} size={size} />
        }}
      /> 
      <Tab.Screen 
        name="ProfileTab" 
        component={ProfileNavigator} 
        options={{
          title: 'Profile',
          tabBarIcon: ({ color, size }) => <UserIcon color={color} size={size} />
        }}
      /> 
    </Tab.Navigator>
  );
}

export type CustomerStackParamList = {
  CustomerDashboard: undefined;
  CustomerTicketCreate: undefined;
  CustomerTicketDetail: { id: string };
};

const CustomerStack = createNativeStackNavigator<CustomerStackParamList>();

function CustomerTicketNavigator() {
  return (
    <CustomerStack.Navigator
      screenOptions={{
        headerStyle: { backgroundColor: '#F2F2F7' },
        headerShadowVisible: false,
        headerTintColor: '#007AFF',
        headerTitleStyle: { fontWeight: '600', color: '#000000' },
        contentStyle: { backgroundColor: '#F2F2F7' }
      }}
    >
      <CustomerStack.Screen 
        name="CustomerDashboard" 
        component={CustomerDashboardScreen} 
        options={{ title: 'My Tickets', headerLargeTitle: true }} 
      />
      <CustomerStack.Screen 
        name="CustomerTicketCreate" 
        component={CustomerTicketCreateScreen} 
        options={{ title: 'New Ticket', presentation: 'modal' }} 
      />
      <CustomerStack.Screen 
        name="CustomerTicketDetail" 
        component={CustomerTicketDetailScreen} 
        options={{ title: 'Ticket Details' }} 
      />
    </CustomerStack.Navigator>
  );
}

function CustomerTabs({ handleLogout }: { handleLogout: () => void }) {
  return (
    <Tab.Navigator
      initialRouteName="TicketsTab"
      screenOptions={{
        tabBarStyle: { backgroundColor: '#ffffff', borderTopWidth: 0, elevation: 0 },
        tabBarActiveTintColor: '#007AFF',
        tabBarInactiveTintColor: '#8E8E93',
        headerShown: false,
      }}
    >
      <Tab.Screen 
        name="TicketsTab" 
        component={CustomerTicketNavigator} 
        options={{
          title: 'My Tickets',
          tabBarIcon: ({ color, size }) => <TicketIcon color={color} size={size} />
        }}
      />
      <Tab.Screen 
        name="ProfileTab" 
        component={ProfileNavigator} 
        options={{
          title: 'Profile',
          tabBarIcon: ({ color, size }) => <UserIcon color={color} size={size} />
        }}
      /> 
    </Tab.Navigator>
  );
}

export type AuthStackParamList = {
  Login: undefined;
  CustomerRegister: undefined;
};

const AuthStack = createNativeStackNavigator<AuthStackParamList>();

function AuthNavigator({ onLoginSuccess }: { onLoginSuccess: () => void }) {
  return (
    <AuthStack.Navigator screenOptions={{ headerShown: false }}>
      <AuthStack.Screen name="Login">
        {(props) => <LoginScreen {...props} onLoginSuccess={onLoginSuccess} />}
      </AuthStack.Screen>
      <AuthStack.Screen name="CustomerRegister" component={CustomerRegisterScreen} />
    </AuthStack.Navigator>
  );
}

// ... MainTabs ...

function App(): React.JSX.Element {
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
  const [loading, setLoading] = useState(true);
  const [userRole, setUserRole] = useState<number | null>(null);

  useEffect(() => {
    checkAuth();
    
    const onSessionExpired = () => {
      handleLogout();
    };
    
    authEmitter.on('session_expired', onSessionExpired);
    return () => {
      authEmitter.off('session_expired', onSessionExpired);
    };
  }, []);

  const checkAuth = async () => {
    const token = await authService.getToken();
    if (token) {
      const user = await authService.getUser();
      setUserRole(user?.role !== undefined ? user.role : 1);
      setIsAuthenticated(true);
    } else {
      setIsAuthenticated(false);
    }
    setLoading(false);
  };

  const handleLoginSuccess = async () => {
    const user = await authService.getUser();
    setUserRole(user?.role !== undefined ? user.role : 1);
    setIsAuthenticated(true);
  };

  const handleLogout = async () => {
    await authService.logout();
    setIsAuthenticated(false);
    setUserRole(null);
  };

  if (loading) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
        <ActivityIndicator size="large" color="#007AFF" />
      </View>
    );
  }

  return (
    <NavigationContainer>
      {isAuthenticated ? (
        userRole === 0 ? (
          <CustomerTabs handleLogout={handleLogout} />
        ) : (
          <MainTabs handleLogout={handleLogout} userRole={userRole} />
        )
      ) : (
        <AuthNavigator onLoginSuccess={handleLoginSuccess} />
      )}
    </NavigationContainer>
  );
}

export default App;
