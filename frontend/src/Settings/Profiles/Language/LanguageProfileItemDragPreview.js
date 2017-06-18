import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { DragLayer } from 'react-dnd';
import dimensions from 'Styles/Variables/dimensions.js';
import { QUALITY_PROFILE_ITEM } from 'Helpers/dragTypes';
import DragPreviewLayer from 'Components/DragPreviewLayer';
import LanguageProfileItem from './LanguageProfileItem';
import styles from './LanguageProfileItemDragPreview.css';

const formGroupSmallWidth = parseInt(dimensions.formGroupSmallWidth);
const formLabelLargeWidth = parseInt(dimensions.formLabelLargeWidth);
const formLabelRightMarginWidth = parseInt(dimensions.formLabelRightMarginWidth);
const dragHandleWidth = parseInt(dimensions.dragHandleWidth);

function collectDragLayer(monitor) {
  return {
    item: monitor.getItem(),
    itemType: monitor.getItemType(),
    currentOffset: monitor.getSourceClientOffset()
  };
}

class LanguageProfileItemDragPreview extends Component {

  //
  // Render

  render() {
    const {
      item,
      itemType,
      currentOffset
    } = this.props;

    if (!currentOffset || itemType !== QUALITY_PROFILE_ITEM) {
      return null;
    }

    // The offset is shifted because the drag handle is on the right edge of the
    // list item and the preview is wider than the drag handle.

    const { x, y } = currentOffset;
    const handleOffset = formGroupSmallWidth - formLabelLargeWidth - formLabelRightMarginWidth - dragHandleWidth;
    const transform = `translate3d(${x - handleOffset}px, ${y}px, 0)`;

    const style = {
      position: 'absolute',
      WebkitTransform: transform,
      msTransform: transform,
      transform
    };

    const {
      languageId,
      name,
      allowed,
      sortIndex
    } = item;

    return (
      <DragPreviewLayer>
        <div
          className={styles.dragPreview}
          style={style}
        >
          <LanguageProfileItem
            languageId={languageId}
            name={name}
            allowed={allowed}
            sortIndex={sortIndex}
            isDragging={false}
          />
        </div>
      </DragPreviewLayer>
    );
  }
}

LanguageProfileItemDragPreview.propTypes = {
  item: PropTypes.object,
  itemType: PropTypes.string,
  currentOffset: PropTypes.shape({
    x: PropTypes.number.isRequired,
    y: PropTypes.number.isRequired
  })
};

export default DragLayer(collectDragLayer)(LanguageProfileItemDragPreview);
