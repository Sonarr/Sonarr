import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { findDOMNode } from 'react-dom';
import { DragSource, DropTarget } from 'react-dnd';
import classNames from 'classnames';
import { QUALITY_PROFILE_ITEM } from 'Helpers/dragTypes';
import QualityProfileItem from './QualityProfileItem';
import styles from './QualityProfileItemDragSource.css';

const qualityProfileItemDragSource = {
  beginDrag({ qualityId, name, allowed, sortIndex }) {
    return {
      qualityId,
      name,
      allowed,
      sortIndex
    };
  },

  endDrag(props, monitor, component) {
    props.onQualityProfileItemDragEnd(monitor.getItem(), monitor.didDrop());
  }
};

const qualityProfileItemDropTarget = {
  hover(props, monitor, component) {
    const dragIndex = monitor.getItem().sortIndex;
    const hoverIndex = props.sortIndex;

    const hoverBoundingRect = findDOMNode(component).getBoundingClientRect();
    const hoverMiddleY = (hoverBoundingRect.bottom - hoverBoundingRect.top) / 2;
    const clientOffset = monitor.getClientOffset();
    const hoverClientY = clientOffset.y - hoverBoundingRect.top;

    // Moving up, only trigger if drag position is above 50%
    if (dragIndex < hoverIndex && hoverClientY > hoverMiddleY) {
      return;
    }

    // Moving down, only trigger if drag position is below 50%
    if (dragIndex > hoverIndex && hoverClientY < hoverMiddleY) {
      return;
    }

    props.onQualityProfileItemDragMove(dragIndex, hoverIndex);
  }
};

function collectDragSource(connect, monitor) {
  return {
    connectDragSource: connect.dragSource(),
    isDragging: monitor.isDragging()
  };
}

function collectDropTarget(connect, monitor) {
  return {
    connectDropTarget: connect.dropTarget(),
    isOver: monitor.isOver()
  };
}

class QualityProfileItemDragSource extends Component {

  //
  // Render

  render() {
    const {
      qualityId,
      name,
      allowed,
      sortIndex,
      isDragging,
      isDraggingUp,
      isDraggingDown,
      isOver,
      connectDragSource,
      connectDropTarget,
      onQualityProfileItemAllowedChange
    } = this.props;

    const isBefore = !isDragging && isDraggingUp && isOver;
    const isAfter = !isDragging && isDraggingDown && isOver;

    // if (isDragging && !isOver) {
    //   return null;
    // }

    return connectDropTarget(
      <div
        className={classNames(
          styles.qualityProfileItemDragSource,
          isBefore && styles.isDraggingUp,
          isAfter && styles.isDraggingDown
        )}
      >
        {
          isBefore &&
            <div
              className={classNames(
                styles.qualityProfileItemPlaceholder,
                styles.qualityProfileItemPlaceholderBefore
              )}
            />
        }

        <QualityProfileItem
          qualityId={qualityId}
          name={name}
          allowed={allowed}
          sortIndex={sortIndex}
          isDragging={isDragging}
          isOver={isOver}
          connectDragSource={connectDragSource}
          onQualityProfileItemAllowedChange={onQualityProfileItemAllowedChange}
        />

        {
          isAfter &&
            <div
              className={classNames(
                styles.qualityProfileItemPlaceholder,
                styles.qualityProfileItemPlaceholderAfter
              )}
            />
        }
      </div>
    );
  }
}

QualityProfileItemDragSource.propTypes = {
  qualityId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  allowed: PropTypes.bool.isRequired,
  sortIndex: PropTypes.number.isRequired,
  isDragging: PropTypes.bool,
  isDraggingUp: PropTypes.bool,
  isDraggingDown: PropTypes.bool,
  isOver: PropTypes.bool,
  connectDragSource: PropTypes.func,
  connectDropTarget: PropTypes.func,
  onQualityProfileItemAllowedChange: PropTypes.func.isRequired,
  onQualityProfileItemDragMove: PropTypes.func.isRequired,
  onQualityProfileItemDragEnd: PropTypes.func.isRequired
};

export default DropTarget(
  QUALITY_PROFILE_ITEM,
  qualityProfileItemDropTarget,
  collectDropTarget
)(DragSource(
  QUALITY_PROFILE_ITEM,
  qualityProfileItemDragSource,
  collectDragSource
)(QualityProfileItemDragSource));
