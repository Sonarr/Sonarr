import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import VirtualTableRowCell from './VirtualTableRowCell';
import styles from './VirtualTableSelectCell.css';

export function virtualTableSelectCellRenderer(cellProps) {
  const {
    cellKey,
    rowData,
    columnData,
    ...otherProps
  } = cellProps;

  return (
    <VirtualTableSelectCell
      key={cellKey}
      id={rowData.name}
      isSelected={rowData.isSelected}
      {...columnData}
      {...otherProps}
    />
  );
}

class VirtualTableSelectCell extends Component {

  //
  // Listeners

  onChange = ({ value, shiftKey }) => {
    const {
      id,
      onSelectedChange
    } = this.props;

    onSelectedChange({ id, value, shiftKey });
  }

  //
  // Render

  render() {
    const {
      inputClassName,
      id,
      isSelected,
      isDisabled,
      ...otherProps
    } = this.props;

    return (
      <VirtualTableRowCell
        className={styles.cell}
        {...otherProps}
      >
        <CheckInput
          className={inputClassName}
          name={id.toString()}
          value={isSelected}
          isDisabled={isDisabled}
          onChange={this.onChange}
        />
      </VirtualTableRowCell>
    );
  }
}

VirtualTableSelectCell.propTypes = {
  inputClassName: PropTypes.string.isRequired,
  id: PropTypes.oneOfType([PropTypes.number, PropTypes.string]).isRequired,
  isSelected: PropTypes.bool.isRequired,
  isDisabled: PropTypes.bool.isRequired,
  onSelectedChange: PropTypes.func.isRequired
};

VirtualTableSelectCell.defaultProps = {
  inputClassName: styles.input,
  isSelected: false
};

export default VirtualTableSelectCell;
