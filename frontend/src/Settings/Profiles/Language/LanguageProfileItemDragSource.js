import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { findDOMNode } from 'react-dom';
import { DragSource, DropTarget } from 'react-dnd';
import classNames from 'classnames';
import { QUALITY_PROFILE_ITEM } from 'Helpers/dragTypes';
import LanguageProfileItem from './LanguageProfileItem';
import styles from './LanguageProfileItemDragSource.css';

const languageProfileItemDragSource = {
  beginDrag({ languageId, name, allowed, sortIndex }) {
    return {
      languageId,
      name,
      allowed,
      sortIndex
    };
  },

  endDrag(props, monitor, component) {
    props.onLanguageProfileItemDragEnd(monitor.getItem(), monitor.didDrop());
  }
};

const languageProfileItemDropTarget = {
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

    props.onLanguageProfileItemDragMove(dragIndex, hoverIndex);
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

class LanguageProfileItemDragSource extends Component {

  //
  // Render

  render() {
    const {
      languageId,
      name,
      allowed,
      sortIndex,
      isDragging,
      isDraggingUp,
      isDraggingDown,
      isOver,
      connectDragSource,
      connectDropTarget,
      onLanguageProfileItemAllowedChange
    } = this.props;

    const isBefore = !isDragging && isDraggingUp && isOver;
    const isAfter = !isDragging && isDraggingDown && isOver;

    // if (isDragging && !isOver) {
    //   return null;
    // }

    return connectDropTarget(
      <div
        className={classNames(
          styles.languageProfileItemDragSource,
          isBefore && styles.isDraggingUp,
          isAfter && styles.isDraggingDown
        )}
      >
        {
          isBefore &&
            <div
              className={classNames(
                styles.languageProfileItemPlaceholder,
                styles.languageProfileItemPlaceholderBefore
              )}
            />
        }

        <LanguageProfileItem
          languageId={languageId}
          name={name}
          allowed={allowed}
          sortIndex={sortIndex}
          isDragging={isDragging}
          isOver={isOver}
          connectDragSource={connectDragSource}
          onLanguageProfileItemAllowedChange={onLanguageProfileItemAllowedChange}
        />

        {
          isAfter &&
            <div
              className={classNames(
                styles.languageProfileItemPlaceholder,
                styles.languageProfileItemPlaceholderAfter
              )}
            />
        }
      </div>
    );
  }
}

LanguageProfileItemDragSource.propTypes = {
  languageId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  allowed: PropTypes.bool.isRequired,
  sortIndex: PropTypes.number.isRequired,
  isDragging: PropTypes.bool,
  isDraggingUp: PropTypes.bool,
  isDraggingDown: PropTypes.bool,
  isOver: PropTypes.bool,
  connectDragSource: PropTypes.func,
  connectDropTarget: PropTypes.func,
  onLanguageProfileItemAllowedChange: PropTypes.func.isRequired,
  onLanguageProfileItemDragMove: PropTypes.func.isRequired,
  onLanguageProfileItemDragEnd: PropTypes.func.isRequired
};

export default DropTarget(
  QUALITY_PROFILE_ITEM,
  languageProfileItemDropTarget,
  collectDropTarget
)(DragSource(
  QUALITY_PROFILE_ITEM,
  languageProfileItemDragSource,
  collectDragSource
)(LanguageProfileItemDragSource));
