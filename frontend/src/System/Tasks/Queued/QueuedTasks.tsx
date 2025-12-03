import React from 'react';
import { useCommands } from 'Commands/useCommands';
import FieldSet from 'Components/FieldSet';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import translate from 'Utilities/String/translate';
import QueuedTaskRow from './QueuedTaskRow';

const columns: Column[] = [
  {
    name: 'trigger',
    label: '',
    isVisible: true,
  },
  {
    name: 'commandName',
    label: () => translate('Name'),
    isVisible: true,
  },
  {
    name: 'queued',
    label: () => translate('Queued'),
    isVisible: true,
  },
  {
    name: 'started',
    label: () => translate('Started'),
    isVisible: true,
  },
  {
    name: 'ended',
    label: () => translate('Ended'),
    isVisible: true,
  },
  {
    name: 'duration',
    label: () => translate('Duration'),
    isVisible: true,
  },
  {
    name: 'actions',
    label: '',
    isVisible: true,
  },
];

export default function QueuedTasks() {
  const { data: commands, isLoading, isError } = useCommands();

  if (isLoading) {
    return (
      <FieldSet legend={translate('Queue')}>
        <LoadingIndicator />
      </FieldSet>
    );
  }

  if (isError) {
    return (
      <FieldSet legend={translate('Queue')}>
        <div>Error loading commands</div>
      </FieldSet>
    );
  }

  return (
    <FieldSet legend={translate('Queue')}>
      <Table columns={columns}>
        <TableBody>
          {commands.map((item) => {
            return <QueuedTaskRow key={item.id} {...item} />;
          })}
        </TableBody>
      </Table>
    </FieldSet>
  );
}
