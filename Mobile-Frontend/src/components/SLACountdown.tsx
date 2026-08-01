import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { Clock, AlertTriangle } from 'lucide-react-native';

interface Props {
  deadline: string;
  isBreached: boolean;
}

const SLACountdown: React.FC<Props> = ({ deadline, isBreached }) => {
  const [timeLeft, setTimeLeft] = useState<string>('');
  const [urgency, setUrgency] = useState<'normal' | 'warning' | 'breached'>('normal');

  useEffect(() => {
    const calculateTimeLeft = () => {
      const now = new Date().getTime();
      const safeDeadline = deadline.endsWith('Z') ? deadline : `${deadline}Z`;
      const target = new Date(safeDeadline).getTime();
      const difference = target - now;

      if (isBreached || difference < 0) {
        setUrgency('breached');
        const breachAmount = Math.abs(difference);
        setTimeLeft(`SLA Breached by ${formatDuration(breachAmount)}`);
        return;
      }

      if (difference < 30 * 60 * 1000) { // Less than 30 mins
        setUrgency('warning');
      } else {
        setUrgency('normal');
      }

      setTimeLeft(`${formatDuration(difference)} remaining`);
    };

    calculateTimeLeft();
    const timer = setInterval(calculateTimeLeft, 1000);

    return () => clearInterval(timer);
  }, [deadline, isBreached]);

  const formatDuration = (ms: number) => {
    const hours = Math.floor(ms / (1000 * 60 * 60));
    const minutes = Math.floor((ms % (1000 * 60 * 60)) / (1000 * 60));
    const seconds = Math.floor((ms % (1000 * 60)) / 1000);

    if (hours > 0) {
      return `${hours}h ${minutes}m ${seconds}s`;
    }
    return `${minutes}m ${seconds}s`;
  };

  const getColors = () => {
    switch (urgency) {
      case 'breached': return { bg: '#FFEBEB', text: '#FF3B30', icon: '#FF3B30' };
      case 'warning': return { bg: '#FFF5E5', text: '#FF9500', icon: '#FF9500' };
      default: return { bg: '#E8F5E9', text: '#34C759', icon: '#34C759' };
    }
  };

  const colors = getColors();

  return (
    <View style={[styles.container, { backgroundColor: colors.bg }]}>
      {urgency === 'breached' ? (
        <AlertTriangle color={colors.icon} size={16} />
      ) : (
        <Clock color={colors.icon} size={16} />
      )}
      <Text style={[styles.text, { color: colors.text }]}>{timeLeft}</Text>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 8,
    paddingHorizontal: 12,
    borderRadius: 8,
    gap: 6,
    marginBottom: 16,
    alignSelf: 'flex-start',
  },
  text: {
    fontSize: 14,
    fontWeight: '600',
  }
});

export default SLACountdown;
