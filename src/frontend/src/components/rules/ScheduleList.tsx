import { Card, Text, Button, Badge } from '@fluentui/react-components';
import { Play20Regular, Delete20Regular, Clock20Regular } from '@fluentui/react-icons';
import { Schedule, ScheduleType } from '../../types/schedule';

interface ScheduleListProps {
  schedules: Schedule[];
  onExecute: (id: string) => void;
  onDelete: (id: string) => void;
}

export const ScheduleList = ({ schedules, onExecute, onDelete }: ScheduleListProps) => {
  const formatCron = (cron: string) => {
    const parts = cron.split(' ');
    if (parts[0] === '*/5') return 'Every 5 minutes';
    if (parts[1] === '*') return 'Hourly';
    if (parts[2] === '*') return 'Daily';
    return cron;
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
      {schedules.map((schedule) => (
        <Card key={schedule.id} style={{ padding: '16px' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <div style={{ flex: 1 }}>
              <div style={{ display: 'flex', gap: '8px', alignItems: 'center', marginBottom: '8px' }}>
                <Text weight="semibold">{schedule.name}</Text>
                <Badge appearance="outline">{ScheduleType[schedule.type]}</Badge>
                {!schedule.isEnabled && <Badge color="subtle">Disabled</Badge>}
              </div>
              <div style={{ display: 'flex', gap: '16px', fontSize: '12px', color: '#666' }}>
                <span><Clock20Regular /> {formatCron(schedule.cronExpression)}</span>
                {schedule.nextRunAt && <span>Next: {new Date(schedule.nextRunAt).toLocaleString()}</span>}
                {schedule.lastRunAt && <span>Last: {new Date(schedule.lastRunAt).toLocaleString()}</span>}
              </div>
            </div>
            <div style={{ display: 'flex', gap: '8px' }}>
              <Button icon={<Play20Regular />} onClick={() => onExecute(schedule.id)}>Run Now</Button>
              <Button appearance="subtle" icon={<Delete20Regular />} onClick={() => onDelete(schedule.id)} />
            </div>
          </div>
        </Card>
      ))}
    </div>
  );
};
