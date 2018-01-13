import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { findDOMNode } from 'react-dom';
import { DragSource, DropTarget } from 'react-dnd';
import classNames from 'classnames';
import { TABLE_COLUMN } from 'Helpers/dragTypes';
import TableOptionsColumn from './TableOptionsColumn';
import styles from './TableOptionsColumnDragSource.css';

const columnDragSource = {
  beginDrag(column) {
    return column;
  },

  endDrag(props, monitor, component) {
    props.onColumnDragEnd(monitor.getItem(), monitor.didDrop());
  }
};

const columnDropTarget = {
  hover(props, monitor, component) {
    const dragIndex = monitor.getItem().index;
    const hoverIndex = props.index;

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

    // Dragging downwards
    if (dragIndex < hoverIndex && hoverClientY < hoverMiddleY) {
      return;
    }

    // Dragging upwards
    if (dragIndex > hoverIndex && hoverClientY > hoverMiddleY) {
      return;
    }

    props.onColumnDragMove(dragIndex, hoverIndex);
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

class TableOptionsColumnDragSource extends Component {

  //
  // Render

  render() {
    const {
      name,
      label,
      isVisible,
      isModifiable,
      index,
      isDragging,
      isDraggingUp,
      isDraggingDown,
      isOver,
      connectDragSource,
      connectDropTarget,
      onVisibleChange
    } = this.props;

    const isBefore = !isDragging && isDraggingUp && isOver;
    const isAfter = !isDragging && isDraggingDown && isOver;

    // if (isDragging && !isOver) {
    //   return null;
    // }

    return connectDropTarget(
      <div
        className={classNames(
          styles.columnDragSource,
          isBefore && styles.isDraggingUp,
          isAfter && styles.isDraggingDown
        )}
      >
        {
          isBefore &&
            <div
              className={classNames(
                styles.columnPlaceholder,
                styles.columnPlaceholderBefore
              )}
            />
        }

        <TableOptionsColumn
          name={name}
          label={label}
          isVisible={isVisible}
          isModifiable={isModifiable}
          index={index}
          isDragging={isDragging}
          isOver={isOver}
          connectDragSource={connectDragSource}
          onVisibleChange={onVisibleChange}
        />

        {
          isAfter &&
            <div
              className={classNames(
                styles.columnPlaceholder,
                styles.columnPlaceholderAfter
              )}
            />
        }
      </div>
    );
  }
}

TableOptionsColumnDragSource.propTypes = {
  name: PropTypes.string.isRequired,
  label: PropTypes.string.isRequired,
  isVisible: PropTypes.bool.isRequired,
  isModifiable: PropTypes.bool.isRequired,
  index: PropTypes.number.isRequired,
  isDragging: PropTypes.bool,
  isDraggingUp: PropTypes.bool,
  isDraggingDown: PropTypes.bool,
  isOver: PropTypes.bool,
  connectDragSource: PropTypes.func,
  connectDropTarget: PropTypes.func,
  onVisibleChange: PropTypes.func.isRequired,
  onColumnDragMove: PropTypes.func.isRequired,
  onColumnDragEnd: PropTypes.func.isRequired
};

export default DropTarget(
  TABLE_COLUMN,
  columnDropTarget,
  collectDropTarget
)(DragSource(
  TABLE_COLUMN,
  columnDragSource,
  collectDragSource
)(TableOptionsColumnDragSource));
