import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { DragLayer } from 'react-dnd';
import dimensions from 'Styles/Variables/dimensions.js';
import { DELAY_PROFILE } from 'Helpers/dragTypes';
import DragPreviewLayer from 'Components/DragPreviewLayer';
import DelayProfile from './DelayProfile';
import styles from './DelayProfileDragPreview.css';

const dragHandleWidth = parseInt(dimensions.dragHandleWidth);

function collectDragLayer(monitor) {
  return {
    item: monitor.getItem(),
    itemType: monitor.getItemType(),
    currentOffset: monitor.getSourceClientOffset()
  };
}

class DelayProfileDragPreview extends Component {

  //
  // Render

  render() {
    const {
      width,
      item,
      itemType,
      currentOffset
    } = this.props;

    if (!currentOffset || itemType !== DELAY_PROFILE) {
      return null;
    }

    // The offset is shifted because the drag handle is on the right edge of the
    // list item and the preview is wider than the drag handle.

    const { x, y } = currentOffset;
    const handleOffset = width - dragHandleWidth;
    const transform = `translate3d(${x - handleOffset}px, ${y}px, 0)`;

    const style = {
      width,
      position: 'absolute',
      WebkitTransform: transform,
      msTransform: transform,
      transform
    };

    return (
      <DragPreviewLayer>
        <div
          className={styles.dragPreview}
          style={style}
        >
          <DelayProfile
            isDragging={false}
            {...item}
          />
        </div>
      </DragPreviewLayer>
    );
  }
}

DelayProfileDragPreview.propTypes = {
  width: PropTypes.number.isRequired,
  item: PropTypes.object,
  itemType: PropTypes.string,
  currentOffset: PropTypes.shape({
    x: PropTypes.number.isRequired,
    y: PropTypes.number.isRequired
  })
};

export default DragLayer(collectDragLayer)(DelayProfileDragPreview);
