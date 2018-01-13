import PropTypes from 'prop-types';
import React from 'react';
import classNames from 'classnames';
import titleCase from 'Utilities/String/titleCase';
import styles from './LegendItem.css';

function LegendItem(props) {
  const {
    name,
    status,
    tooltip,
    colorImpairedMode
  } = props;

  return (
    <div
      className={classNames(
        styles.legendItem,
        styles[status],
        colorImpairedMode && 'colorImpaired'
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
  colorImpairedMode: PropTypes.bool.isRequired
};

export default LegendItem;
