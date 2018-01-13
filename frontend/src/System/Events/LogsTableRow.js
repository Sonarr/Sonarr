import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRowButton from 'Components/Table/TableRowButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import LogsTableDetailsModal from './LogsTableDetailsModal';
import styles from './LogsTableRow.css';

function getIconName(level) {
  switch (level) {
    case 'trace':
    case 'debug':
    case 'info':
      return icons.INFO;
    case 'warn':
      return icons.DANGER;
    case 'error':
      return icons.BUG;
    case 'fatal':
      return icons.FATAL;
    default:
      return icons.UNKNOWN;
  }
}

class LogsTableRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isDetailsModalOpen: false
    };
  }

  //
  // Listeners

  onPress = () => {
    // Don't re-open the modal if it's already open
    if (!this.state.isDetailsModalOpen) {
      this.setState({ isDetailsModalOpen: true });
    }
  }

  onModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      level,
      logger,
      message,
      time,
      exception,
      columns
    } = this.props;

    return (
      <TableRowButton
        overlayContent={true}
        onPress={this.onPress}
      >
        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (name === 'level') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.level}
                >
                  <Icon
                    className={styles[level]}
                    name={getIconName(level)}
                    title={level}
                  />
                </TableRowCell>
              );
            }

            if (name === 'logger') {
              return (
                <TableRowCell key={name}>
                  {logger}
                </TableRowCell>
              );
            }

            if (name === 'message') {
              return (
                <TableRowCell key={name}>
                  {message}
                </TableRowCell>
              );
            }

            if (name === 'time') {
              return (
                <RelativeDateCellConnector
                  key={name}
                  date={time}
                />
              );
            }

            if (name === 'actions') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.actions}
                />
              );
            }

            return null;
          })
        }

        <LogsTableDetailsModal
          isOpen={this.state.isDetailsModalOpen}
          message={message}
          exception={exception}
          onModalClose={this.onModalClose}
        />
      </TableRowButton>
    );
  }

}

LogsTableRow.propTypes = {
  level: PropTypes.string.isRequired,
  logger: PropTypes.string.isRequired,
  message: PropTypes.string.isRequired,
  time: PropTypes.string.isRequired,
  exception: PropTypes.string,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default LogsTableRow;
