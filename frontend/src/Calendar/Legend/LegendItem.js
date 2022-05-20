import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import titleCase from 'Utilities/String/titleCase';
import styles from './LegendItem.css';

function LegendItem(props) {
  const {
    name,
    status,
    tooltip,
    isAgendaView,
    fullColorEvents,
    colorImpairedMode
  } = props;

  return (
    <div
      className={classNames(
        styles.legendItem,
        styles[status],
        colorImpairedMode && 'colorImpaired',
        fullColorEvents && !isAgendaView && 'fullColor'
      )}
      title={tooltip}
    >
      {name ? name : titleCase(status)}
    </div>
  );
}

LegendItem.propTypes = {
  name: PropTypes.string,
  status: PropTypes.string.isRequired,
  tooltip: PropTypes.string.isRequired,
  isAgendaView: PropTypes.bool.isRequired,
  fullColorEvents: PropTypes.bool.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired
};

export default LegendItem;
