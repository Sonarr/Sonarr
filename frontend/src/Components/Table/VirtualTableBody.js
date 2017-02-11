import React from 'react';
import { Grid } from 'react-virtualized';
import styles from './VirtualTableBody.css';

class VirtualTableBody extends Grid {

  //
  // Render

  render() {
    const {
      autoContainerWidth,
      containerStyle
    } = this.props;

    const { isScrolling } = this.state;

    const totalColumnsWidth = this._columnSizeAndPositionManager.getTotalSize();
    const totalRowsHeight = this._rowSizeAndPositionManager.getTotalSize();
    const childrenToDisplay = this._childrenToDisplay;

    if (childrenToDisplay.length > 0) {
      return (
        <div className={styles.tableBodyContainer}>
          <div
            style={{
              width: autoContainerWidth ? 'auto' : totalColumnsWidth,
              height: totalRowsHeight,
              maxWidth: totalColumnsWidth,
              maxHeight: totalRowsHeight,
              overflow: 'hidden',
              pointerEvents: isScrolling ? 'none' : '',
              ...containerStyle
            }}
          >
            {childrenToDisplay}
          </div>
        </div>
      );
    }

    return (
      <div />
    );
  }
}

export default VirtualTableBody;
