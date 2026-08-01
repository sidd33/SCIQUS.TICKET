import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet, ScrollView, ActivityIndicator, Alert, SafeAreaView } from 'react-native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import TicketService, { Ticket } from '../services/ticketService';
import GlassCard from '../components/GlassCard';
import AppleButton from '../components/AppleButton';
import SLACountdown from '../components/SLACountdown';

interface Props {
  route: any;
  navigation: any;
}

const CustomerTicketDetailScreen: React.FC<Props> = ({ route, navigation }) => {
  const { id } = route.params;
  const [ticket, setTicket] = useState<Ticket | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchTicket();
  }, [id]);

  const fetchTicket = async () => {
    try {
      const data = await TicketService.getById(id);
      setTicket(data);
    } catch (error) {
      console.error('Failed to fetch ticket:', error);
      Alert.alert('Error', 'Could not load ticket details.');
    } finally {
      setLoading(false);
    }
  };

  const handleCloseTicket = async () => {
    if (!ticket) return;
    try {
      setLoading(true);
      await TicketService.update(ticket.id, {
        title: ticket.title,
        description: ticket.description,
        status: 5, // Closed
        priority: ticket.priority,
        assignedEmployeeId: ticket.assignedEmployeeId
      });
      Alert.alert('Success', 'Ticket has been successfully closed. Thank you!');
      navigation.goBack();
    } catch (error) {
      console.error('Failed to close ticket:', error);
      Alert.alert('Error', 'Failed to close the ticket.');
      setLoading(false);
    }
  };

  const getStatusText = (status: number) => {
    switch (status) {
      case 1: return 'Not Assigned';
      case 2: return 'Currently Assigned';
      case 3: return 'In Progress';
      case 4: return 'Resolved';
      case 5: return 'Closed';
      default: return 'Unknown';
    }
  };

  if (loading || !ticket) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color="#007AFF" />
      </View>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      {ticket.slaDeadline && (
        <View style={{ paddingHorizontal: 16, paddingTop: 16 }}>
          <SLACountdown deadline={ticket.slaDeadline} isBreached={ticket.isSlaBreached} />
        </View>
      )}
      <ScrollView contentContainerStyle={styles.content}>
        <GlassCard style={styles.card}>
          <Text style={styles.title}>{ticket.title}</Text>
          <View style={styles.badgeContainer}>
            <View style={styles.badge}>
              <Text style={styles.badgeText}>{getStatusText(ticket.status)}</Text>
            </View>
          </View>
          <Text style={styles.sectionTitle}>Description</Text>
          <Text style={styles.description}>{ticket.description}</Text>

          {ticket.assignedToName && (
            <View style={{ marginTop: 16 }}>
              <Text style={styles.sectionTitle}>Assigned To</Text>
              <Text style={styles.description}>{ticket.assignedToName}</Text>
            </View>
          )}
        </GlassCard>

        {ticket.status === 4 && (
          <View style={styles.actionContainer}>
            <Text style={styles.actionText}>
              The support team has marked this issue as Resolved. Does everything look good on your end?
            </Text>
            <AppleButton 
              title="Confirm & Close Ticket" 
              onPress={handleCloseTicket} 
            />
          </View>
        )}
      </ScrollView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#F2F2F7' },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  content: { padding: 16, gap: 16 },
  card: { padding: 20 },
  title: { fontSize: 24, fontWeight: '700', color: '#000000', marginBottom: 12 },
  badgeContainer: { flexDirection: 'row', gap: 8, marginBottom: 20 },
  badge: { backgroundColor: '#E5E5EA', paddingHorizontal: 10, paddingVertical: 4, borderRadius: 12 },
  badgeText: { fontSize: 13, fontWeight: '500', color: '#000000' },
  sectionTitle: { fontSize: 17, fontWeight: '600', color: '#000000', marginBottom: 8 },
  description: { fontSize: 15, color: '#3A3A3C', lineHeight: 22 },
  actionContainer: { marginTop: 16, backgroundColor: '#FFFFFF', padding: 20, borderRadius: 14 },
  actionText: { fontSize: 15, color: '#3A3A3C', textAlign: 'center', marginBottom: 16, lineHeight: 20 },
});

export default CustomerTicketDetailScreen;
