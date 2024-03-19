import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import FieldSet from 'Components/FieldSet';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { fetchCommands } from 'Store/Actions/commandActions';
import translate from 'Utilities/String/translate';
import QueuedTaskRow from './QueuedTaskRow';

const columns = [
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
    isVisible: true,
  },
];

export default function QueuedTasks() {
  const dispatch = useDispatch();
  const { isFetching, isPopulated, items } = useSelector(
    (state: AppState) => state.commands
  );

  useEffect(() => {
    dispatch(fetchCommands());
  }, [dispatch]);

  return (
    <FieldSet legend={translate('Queue')}>
      {isFetching && !isPopulated && <LoadingIndicator />}

      {isPopulated && (
        <Table columns={columns}>
          <TableBody>
            {items.map((item) => {
              return <QueuedTaskRow key={item.id} {...item} />;
            })}
          </TableBody>
        </Table>
      )}
    </FieldSet>
  );
}
