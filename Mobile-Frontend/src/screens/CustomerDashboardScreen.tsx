import React, { useCallback, useState } from 'react';
import { View, Text, StyleSheet, FlatList, TouchableOpacity, RefreshControl, SafeAreaView, ActivityIndicator } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import TicketService, { Ticket } from '../services/ticketService';
import GlassCard from '../components/GlassCard';
import { Plus } from 'lucide-react-native';

interface Props {
  navigation: any;
}

const CustomerDashboardScreen: React.FC<Props> = ({ navigation }) => {
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);

  const fetchMyTickets = async (pageNum: number = 1, shouldRefresh: boolean = false) => {
    if (loading && !shouldRefresh) return;
    
    try {
      if (shouldRefresh) setRefreshing(true);
      else setLoading(true);
      
      // TODO: Backend needs an endpoint /customers/me/tickets or similar.
      // For now, we fetch all tickets and assume the backend filters them by the logged-in customer's ID,
      // or we pass a specific parameter if supported.
      const response = await TicketService.getAll(pageNum, 10);
      
      if (pageNum === 1) {
        setTickets(response.items || []);
      } else {
        setTickets(prev => [...prev, ...(response.items || [])]);
      }
      
      setHasMore(pageNum < response.totalPages);
      setPage(pageNum);
    } catch (error) {
      console.error('Failed to fetch customer tickets:', error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  const handleRefresh = () => {
    fetchMyTickets(1, true);
  };

  const handleLoadMore = () => {
    if (hasMore && !loading) {
      fetchMyTickets(page + 1);
    }
  };

  useFocusEffect(
    useCallback(() => {
      fetchMyTickets(1, true);
    }, [])
  );

  React.useLayoutEffect(() => {
    navigation.setOptions({
      headerRight: () => (
        <TouchableOpacity onPress={() => navigation.navigate('CustomerTicketCreate')} style={styles.headerButton}>
          <Plus color="#007AFF" size={24} />
        </TouchableOpacity>
      ),
    });
  }, [navigation]);

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

  const getSLAIndicator = (item: Ticket) => {
    if (!item.slaDeadline) return null;
    const now = new Date().getTime();
    const safeDeadline = item.slaDeadline.endsWith('Z') ? item.slaDeadline : `${item.slaDeadline}Z`;
    const target = new Date(safeDeadline).getTime();
    if (item.isSlaBreached || target < now) return <View style={[styles.slaDot, { backgroundColor: '#FF3B30' }]} />;
    if (target - now < 30 * 60 * 1000) return <View style={[styles.slaDot, { backgroundColor: '#FF9500' }]} />;
    return <View style={[styles.slaDot, { backgroundColor: '#34C759' }]} />;
  };

  const renderItem = ({ item }: { item: Ticket }) => (
    <TouchableOpacity
      activeOpacity={0.7}
      onPress={() => navigation.navigate('CustomerTicketDetail', { id: item.id })}
    >
      <GlassCard style={styles.card}>
        <View style={styles.cardHeader}>
          <View style={{ flexDirection: 'row', alignItems: 'center', flex: 1 }}>
            {getSLAIndicator(item)}
            <Text style={styles.title} numberOfLines={1}>{item.title}</Text>
          </View>
        </View>
        <Text style={styles.description} numberOfLines={2}>{item.description}</Text>
        <View style={styles.cardFooter}>
          <Text style={styles.statusText}>{getStatusText(item.status)}</Text>
        </View>
      </GlassCard>
    </TouchableOpacity>
  );

  return (
    <SafeAreaView style={styles.container}>
      <FlatList
        data={tickets}
        keyExtractor={(item) => item.id}
        renderItem={renderItem}
        contentContainerStyle={styles.listContainer}
        onEndReached={handleLoadMore}
        onEndReachedThreshold={0.5}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={handleRefresh} tintColor="#007AFF" />
        }
        ListEmptyComponent={
          !loading ? (
            <View style={styles.emptyContainer}>
              <Text style={styles.emptyText}>You haven't raised any tickets yet.</Text>
            </View>
          ) : (
            <ActivityIndicator size="large" color="#007AFF" style={{ marginTop: 20 }} />
          )
        }
      />
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#F2F2F7' },
  listContainer: { padding: 16 },
  card: { padding: 16, marginBottom: 12 },
  cardHeader: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 8 },
  title: { fontSize: 17, fontWeight: '600', color: '#000000', flex: 1 },
  description: { fontSize: 15, color: '#8E8E93', marginBottom: 12, lineHeight: 20 },
  cardFooter: { flexDirection: 'row', justifyContent: 'flex-start' },
  statusText: { fontSize: 13, color: '#007AFF', fontWeight: '500' },
  headerButton: { marginRight: 8 },
  emptyContainer: { flex: 1, alignItems: 'center', justifyContent: 'center', paddingTop: 60 },
  emptyText: { fontSize: 17, color: '#8E8E93' },
  slaDot: { width: 10, height: 10, borderRadius: 5, marginRight: 8 },
});

export default CustomerDashboardScreen;
