import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FieldSet from 'Components/FieldSet';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import styles from './Statistics.css';
import formatBytes from 'Utilities/Number/formatBytes';
import formatTimeSpan from 'Utilities/Date/formatTimeSpan';
import moment from 'moment';

function formatValue(val, formatter) {
  if (val === undefined) {
    return 'n/a';
  }

  if (formatter) {
    return formatter(val);
  }

  return val;
}

class Statistics extends Component {

  //
  // Render

  render() {
    const {
      process,
      databaseMain,
      databaseLog,
      commandsExecuted
    } = this.props;

    return (
      <FieldSet legend="Statistics">
        <DescriptionList className={styles.descriptionList}>
          <DescriptionListItem
            title="Up Time"
            data={formatValue(process.startTime, (startTime) => formatTimeSpan(moment().diff(startTime)))}
          />

          <DescriptionListItem
            title="Processor Time"
            data={formatValue(process.totalProcessorTime, formatTimeSpan)}
          />

          <DescriptionListItem
            title="Memory Working Set"
            data={formatValue(process.workingSet, formatBytes)}
          />

          <DescriptionListItem
            title="Memory Virtual Size"
            data={formatValue(process.virtualMemorySize, formatBytes)}
          />

          <DescriptionListItem
            title="Main Database Size"
            data={formatValue(databaseMain.size, formatBytes)}
          />

          <DescriptionListItem
            title="Logs Database Size"
            data={formatValue(databaseLog.size, formatBytes)}
          />

          <DescriptionListItem
            title="Commands Executed"
            data={formatValue(commandsExecuted, formatBytes)}
          />
        </DescriptionList>
      </FieldSet>
    );
  }

}

Statistics.propTypes = {
  process: PropTypes.object,
  databaseMain: PropTypes.object,
  databaseLog: PropTypes.object,
  commandsExecuted: PropTypes.number
};

Statistics.defaultProps = {
  process: {},
  databaseMain: {},
  databaseLog: {}
};

export default Statistics;
