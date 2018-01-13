import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import styles from './SeriesIndexOverviewInfoRow.css';

function SeriesIndexOverviewInfoRow(props) {
  const {
    title,
    iconName,
    label
  } = props;

  return (
    <div
      className={styles.infoRow}
      title={title}
    >
      <Icon
        className={styles.icon}
        name={iconName}
        size={14}
      />

      {label}
    </div>
  );
}

SeriesIndexOverviewInfoRow.propTypes = {
  title: PropTypes.string,
  iconName: PropTypes.object.isRequired,
  label: PropTypes.string.isRequired
};

export default SeriesIndexOverviewInfoRow;
