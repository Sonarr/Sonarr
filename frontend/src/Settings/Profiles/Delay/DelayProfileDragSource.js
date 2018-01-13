import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { findDOMNode } from 'react-dom';
import { DragSource, DropTarget } from 'react-dnd';
import classNames from 'classnames';
import { DELAY_PROFILE } from 'Helpers/dragTypes';
import DelayProfile from './DelayProfile';
import styles from './DelayProfileDragSource.css';

const delayProfileDragSource = {
  beginDrag(item) {
    return item;
  },

  endDrag(props, monitor, component) {
    props.onDelayProfileDragEnd(monitor.getItem(), monitor.didDrop());
  }
};

const delayProfileDropTarget = {
  hover(props, monitor, component) {
    const dragIndex = monitor.getItem().order;
    const hoverIndex = props.order;

    const hoverBoundingRect = findDOMNode(component).getBoundingClientRect();
    const hoverMiddleY = (hoverBoundingRect.bottom - hoverBoundingRect.top) / 2;
    const clientOffset = monitor.getClientOffset();
    const hoverClientY = clientOffset.y - hoverBoundingRect.top;

    if (dragIndex === hoverIndex) {
      return;
    }

    // When moving up, only trigger if drag position is above 50% and
    // when moving down, only trigger if drag position is below 50%.
    // If we're moving down the hoverIndex needs to be increased
    // by one so it's ordered properly. Otherwise the hoverIndex will work.

    if (dragIndex < hoverIndex && hoverClientY > hoverMiddleY) {
      props.onDelayProfileDragMove(dragIndex, hoverIndex + 1);
    } else if (dragIndex > hoverIndex && hoverClientY < hoverMiddleY) {
      props.onDelayProfileDragMove(dragIndex, hoverIndex);
    }
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

class DelayProfileDragSource extends Component {

  //
  // Render

  render() {
    const {
      id,
      order,
      isDragging,
      isDraggingUp,
      isDraggingDown,
      isOver,
      connectDragSource,
      connectDropTarget,
      ...otherProps
    } = this.props;

    const isBefore = !isDragging && isDraggingUp && isOver;
    const isAfter = !isDragging && isDraggingDown && isOver;

    // if (isDragging && !isOver) {
    //   return null;
    // }

    return connectDropTarget(
      <div
        className={classNames(
          styles.delayProfileDragSource,
          isBefore && styles.isDraggingUp,
          isAfter && styles.isDraggingDown
        )}
      >
        {
          isBefore &&
            <div
              className={classNames(
                styles.delayProfilePlaceholder,
                styles.delayProfilePlaceholderBefore
              )}
            />
        }

        <DelayProfile
          id={id}
          order={order}
          isDragging={isDragging}
          isOver={isOver}
          {...otherProps}
          connectDragSource={connectDragSource}
        />

        {
          isAfter &&
            <div
              className={classNames(
                styles.delayProfilePlaceholder,
                styles.delayProfilePlaceholderAfter
              )}
            />
        }
      </div>
    );
  }
}

DelayProfileDragSource.propTypes = {
  id: PropTypes.number.isRequired,
  order: PropTypes.number.isRequired,
  isDragging: PropTypes.bool,
  isDraggingUp: PropTypes.bool,
  isDraggingDown: PropTypes.bool,
  isOver: PropTypes.bool,
  connectDragSource: PropTypes.func,
  connectDropTarget: PropTypes.func,
  onDelayProfileDragMove: PropTypes.func.isRequired,
  onDelayProfileDragEnd: PropTypes.func.isRequired
};

export default DropTarget(
  DELAY_PROFILE,
  delayProfileDropTarget,
  collectDropTarget
)(DragSource(
  DELAY_PROFILE,
  delayProfileDragSource,
  collectDragSource
)(DelayProfileDragSource));
