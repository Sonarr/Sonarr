import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import FieldSet from 'Components/FieldSet';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { fetchTasks } from 'Store/Actions/systemActions';
import translate from 'Utilities/String/translate';
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
  const dispatch = useDispatch();
  const { isFetching, isPopulated, items } = useSelector(
    (state: AppState) => state.system.tasks
  );

  useEffect(() => {
    dispatch(fetchTasks());
  }, [dispatch]);

  return (
    <FieldSet legend={translate('Scheduled')}>
      {isFetching && !isPopulated && <LoadingIndicator />}

      {isPopulated && (
        <Table columns={columns}>
          <TableBody>
            {items.map((item) => {
              return <ScheduledTaskRow key={item.id} {...item} />;
            })}
          </TableBody>
        </Table>
      )}
    </FieldSet>
  );
}

export default ScheduledTasks;
