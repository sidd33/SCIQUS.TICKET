import React, { useCallback, useState } from 'react';
import { View, Text, StyleSheet, FlatList, TouchableOpacity, RefreshControl, SafeAreaView, TextInput } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { RootStackParamList } from '../../App';
import TicketService, { Ticket } from '../services/ticketService';
import GlassCard from '../components/GlassCard';
import { Plus, Search, AlertCircle, Clock } from 'lucide-react-native';
import { authService, User } from '../services/authService';

type TicketListScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, 'TicketList' | 'TicketDetail' | 'TicketEdit'>;

interface Props {
  navigation: TicketListScreenNavigationProp;
}

const TicketListScreen: React.FC<Props> = ({ navigation }) => {
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [viewMode, setViewMode] = useState<'queue' | 'master'>('queue');
  const [currentUser, setCurrentUser] = useState<User | null>(null);

  useFocusEffect(
    useCallback(() => {
      authService.getUser().then(setCurrentUser);
    }, [])
  );

  const fetchTickets = async (pageNum: number = 1, shouldRefresh: boolean = false) => {
    if (loading && !shouldRefresh) return;
    
    try {
      if (shouldRefresh) setRefreshing(true);
      else setLoading(true);
      
      const response = await TicketService.getAll(pageNum, 10, searchQuery);
      
      if (pageNum === 1) {
        setTickets(response.items || []);
      } else {
        setTickets(prev => [...prev, ...(response.items || [])]);
      }
      
      setHasMore(pageNum < response.totalPages);
      setPage(pageNum);
    } catch (error) {
      console.error('Failed to fetch tickets:', error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  const handleRefresh = () => {
    fetchTickets(1, true);
  };

  const handleLoadMore = () => {
    if (hasMore && !loading) {
      fetchTickets(page + 1);
    }
  };

  useFocusEffect(
    useCallback(() => {
      fetchTickets(1, true);
    }, [searchQuery]) // Re-fetch when search changes
  );

  const handleSearch = (text: string) => {
    setSearchQuery(text);
  };

  React.useLayoutEffect(() => {
    navigation.setOptions({
      headerRight: () => (
        <TouchableOpacity onPress={() => navigation.navigate('TicketEdit', {})} style={styles.headerButton}>
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

  const getStatusColor = (status: number) => {
    switch (status) {
      case 1: return '#8E8E93'; // Gray
      case 2: return '#AF52DE'; // Purple
      case 3: return '#007AFF'; // Blue
      case 4: return '#34C759'; // Green
      case 5: return '#1C1C1E'; // Dark
      default: return '#007AFF';
    }
  };

  const getPriorityColor = (priority: number) => {
    switch (priority) {
      case 0: return '#34C759'; // Low
      case 1: return '#FFCC00'; // Medium
      case 2: return '#FF9500'; // High
      case 3: return '#FF3B30'; // Critical
      default: return '#8E8E93';
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
      onPress={() => navigation.navigate('TicketDetail', { id: item.id })}
    >
      <GlassCard style={styles.card}>
        <View style={styles.cardHeader}>
          {getSLAIndicator(item)}
          <Text style={[styles.title, { flex: 1, marginLeft: 6 }]} numberOfLines={1}>{item.title}</Text>
          <View style={[styles.priorityBadge, { backgroundColor: getPriorityColor(item.priority) }]}>
            <Text style={styles.priorityText}>Priority {item.priority}</Text>
          </View>
        </View>
        <Text style={styles.description} numberOfLines={2}>{item.description}</Text>
        <View style={styles.cardFooter}>
          <Text style={[styles.statusText, { color: getStatusColor(item.status) }]}>{getStatusText(item.status)}</Text>
          {item.slaDeadline && (
            <View style={styles.slaBadge}>
              <Clock color={item.isSlaBreached ? '#FF3B30' : '#8E8E93'} size={14} />
              <Text style={[styles.slaText, item.isSlaBreached && styles.slaTextBreached]}>
                {new Date(item.slaDeadline.endsWith('Z') ? item.slaDeadline : `${item.slaDeadline}Z`).toLocaleDateString()}
              </Text>
            </View>
          )}
        </View>
      </GlassCard>
    </TouchableOpacity>
  );

  const filteredTickets = viewMode === 'queue' && currentUser?.role === 1
    ? tickets.filter(t => t.assignedEmployeeId === currentUser.id)
    : tickets;

  return (
    <SafeAreaView style={styles.container}>
      {currentUser?.role === 2 && (
        <View style={styles.toggleContainer}>
          <TouchableOpacity 
            style={[styles.toggleButton, viewMode === 'queue' && styles.toggleButtonActive]}
            onPress={() => setViewMode('queue')}
          >
            <Text style={[styles.toggleText, viewMode === 'queue' && styles.toggleTextActive]}>My Queue</Text>
          </TouchableOpacity>
          <TouchableOpacity 
            style={[styles.toggleButton, viewMode === 'master' && styles.toggleButtonActive]}
            onPress={() => setViewMode('master')}
          >
            <Text style={[styles.toggleText, viewMode === 'master' && styles.toggleTextActive]}>Master View</Text>
          </TouchableOpacity>
        </View>
      )}
      <View style={styles.searchContainer}>
        <Search color="#8E8E93" size={20} style={styles.searchIcon} />
        <TextInput
          style={styles.searchInput}
          placeholder="Search tickets..."
          value={searchQuery}
          onChangeText={handleSearch}
          placeholderTextColor="#8E8E93"
          clearButtonMode="while-editing"
        />
      </View>
      <FlatList
        data={filteredTickets}
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
              <Text style={styles.emptyText}>No tickets found</Text>
            </View>
          ) : null
        }
      />
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#F2F2F7',
  },
  toggleContainer: {
    flexDirection: 'row',
    marginHorizontal: 16,
    marginTop: 16,
    backgroundColor: '#E5E5EA',
    borderRadius: 8,
    padding: 2,
  },
  toggleButton: {
    flex: 1,
    paddingVertical: 8,
    alignItems: 'center',
    borderRadius: 6,
  },
  toggleButtonActive: {
    backgroundColor: '#FFFFFF',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.1,
    shadowRadius: 1,
    elevation: 2,
  },
  toggleText: {
    fontSize: 13,
    fontWeight: '500',
    color: '#8E8E93',
  },
  toggleTextActive: {
    color: '#000000',
    fontWeight: '600',
  },
  searchContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#E5E5EA',
    margin: 16,
    marginBottom: 0,
    borderRadius: 10,
    paddingHorizontal: 8,
    height: 36,
  },
  searchIcon: {
    marginRight: 6,
  },
  searchInput: {
    flex: 1,
    fontSize: 17,
    color: '#000000',
    height: '100%',
    padding: 0,
  },
  listContainer: {
    padding: 16,
  },
  card: {
    padding: 16,
    marginBottom: 12,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  title: {
    fontSize: 17,
    fontWeight: '600',
    color: '#000000',
    flex: 1,
    marginRight: 8,
  },
  priorityBadge: {
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
  },
  priorityText: {
    color: '#FFFFFF',
    fontSize: 12,
    fontWeight: '600',
  },
  description: {
    fontSize: 15,
    color: '#8E8E93',
    marginBottom: 12,
    lineHeight: 20,
  },
  cardFooter: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  statusText: {
    fontSize: 13,
    fontWeight: '600',
  },
  slaBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    backgroundColor: '#F2F2F7',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
  },
  slaText: {
    fontSize: 12,
    color: '#8E8E93',
    fontWeight: '500',
  },
  slaTextBreached: {
    color: '#FF3B30',
    fontWeight: '600',
  },
  headerButton: {
    marginRight: 8,
  },
  emptyContainer: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    paddingTop: 60,
  },
  emptyText: {
    fontSize: 17,
    color: '#8E8E93',
  },
  slaDot: {
    width: 10,
    height: 10,
    borderRadius: 5,
  },
});

export default TicketListScreen;
