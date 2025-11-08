import React from 'react';
import FieldSet from 'Components/FieldSet';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import translate from 'Utilities/String/translate';
import useTasks from '../useTasks';
import ScheduledTaskRow from './ScheduledTaskRow';

const columns: Column[] = [
  {
    name: 'name',
    label: () => translate('Name'),
    isVisible: true,
  },
  {
    name: 'interval',
    label: () => translate('Interval'),
    isVisible: true,
  },
  {
    name: 'lastExecution',
    label: () => translate('LastExecution'),
    isVisible: true,
  },
  {
    name: 'lastDuration',
    label: () => translate('LastDuration'),
    isVisible: true,
  },
  {
    name: 'nextExecution',
    label: () => translate('NextExecution'),
    isVisible: true,
  },
  {
    name: 'actions',
    label: '',
    isVisible: true,
  },
];

function ScheduledTasks() {
  const { data: tasks, isLoading, error } = useTasks();

  if (error) {
    return (
      <FieldSet legend={translate('Scheduled')}>
        <div>Error loading tasks: {error.message}</div>
      </FieldSet>
    );
  }

  return (
    <FieldSet legend={translate('Scheduled')}>
      {isLoading && <LoadingIndicator />}

      {tasks.length > 0 && (
        <Table columns={columns}>
          <TableBody>
            {tasks.map((task) => {
              return <ScheduledTaskRow key={task.id} {...task} />;
            })}
          </TableBody>
        </Table>
      )}

      {!isLoading && tasks.length === 0 && <div>No scheduled tasks found.</div>}
    </FieldSet>
  );
}

export default ScheduledTasks;
