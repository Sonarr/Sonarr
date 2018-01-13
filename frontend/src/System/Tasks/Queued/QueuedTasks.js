import PropTypes from 'prop-types';
import React from 'react';
import FieldSet from 'Components/FieldSet';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import QueuedTaskRowConnector from './QueuedTaskRowConnector';

const columns = [
  {
    name: 'trigger',
    label: '',
    isVisible: true
  },
  {
    name: 'commandName',
    label: 'Name',
    isVisible: true
  },
  {
    name: 'queued',
    label: 'Queued',
    isVisible: true
  },
  {
    name: 'started',
    label: 'Started',
    isVisible: true
  },
  {
    name: 'ended',
    label: 'Ended',
    isVisible: true
  },
  {
    name: 'duration',
    label: 'Duration',
    isVisible: true
  },
  {
    name: 'actions',
    isVisible: true
  }
];

function QueuedTasks(props) {
  const {
    isFetching,
    isPopulated,
    items
  } = props;

  return (
    <FieldSet legend="Queue">
      {
        isFetching && !isPopulated &&
        <LoadingIndicator />
      }

      {
        isPopulated &&
        <Table
          columns={columns}
        >
          <TableBody>
            {
              items.map((item) => {
                return (
                  <QueuedTaskRowConnector
                    key={item.id}
                    {...item}
                  />
                );
              })
            }
          </TableBody>
        </Table>
      }
    </FieldSet>
  );
}

QueuedTasks.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  items: PropTypes.array.isRequired
};

export default QueuedTasks;
