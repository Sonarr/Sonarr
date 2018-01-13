import PropTypes from 'prop-types';
import React from 'react';
import classNames from 'classnames';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import CheckInput from 'Components/Form/CheckInput';
import styles from './TableOptionsColumn.css';

function TableOptionsColumn(props) {
  const {
    name,
    label,
    isVisible,
    isModifiable,
    isDragging,
    connectDragSource,
    onVisibleChange
  } = props;

  return (
    <div className={isModifiable ? undefined : styles.notDragable}>
      <div
        className={classNames(
          styles.column,
          isDragging && styles.isDragging
        )}
      >
        <label
          className={styles.label}
        >
          <CheckInput
            containerClassName={styles.checkContainer}
            name={name}
            value={isVisible}
            isDisabled={isModifiable === false}
            onChange={onVisibleChange}
          />
          {label}
        </label>

        {
          !!connectDragSource &&
            connectDragSource(
              <div className={styles.dragHandle}>
                <Icon
                  className={styles.dragIcon}
                  name={icons.REORDER}
                />
              </div>
            )
        }
      </div>
    </div>
  );
}

TableOptionsColumn.propTypes = {
  name: PropTypes.string.isRequired,
  label: PropTypes.string.isRequired,
  isVisible: PropTypes.bool.isRequired,
  isModifiable: PropTypes.bool.isRequired,
  index: PropTypes.number.isRequired,
  isDragging: PropTypes.bool,
  connectDragSource: PropTypes.func,
  onVisibleChange: PropTypes.func.isRequired
};

export default TableOptionsColumn;
