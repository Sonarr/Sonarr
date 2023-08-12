import PropTypes from 'prop-types';
import React from 'react';
import FieldSet from 'Components/FieldSet';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import translate from 'Utilities/String/translate';
import ScheduledTaskRowConnector from './ScheduledTaskRowConnector';

const columns = [
  {
    name: 'name',
    label: () => translate('Name'),
    isVisible: true
  },
  {
    name: 'interval',
    label: () => translate('Interval'),
    isVisible: true
  },
  {
    name: 'lastExecution',
    label: () => translate('LastExecution'),
    isVisible: true
  },
  {
    name: 'lastDuration',
    label: () => translate('LastDuration'),
    isVisible: true
  },
  {
    name: 'nextExecution',
    label: () => translate('NextExecution'),
    isVisible: true
  },
  {
    name: 'actions',
    isVisible: true
  }
];

function ScheduledTasks(props) {
  const {
    isFetching,
    isPopulated,
    items
  } = props;

  return (
    <FieldSet legend={translate('Scheduled')}>
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
                    <ScheduledTaskRowConnector
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

ScheduledTasks.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  items: PropTypes.array.isRequired
};

export default ScheduledTasks;
