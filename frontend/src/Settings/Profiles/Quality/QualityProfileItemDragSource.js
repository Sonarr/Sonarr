import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { findDOMNode } from 'react-dom';
import { DragSource, DropTarget } from 'react-dnd';
import classNames from 'classnames';
import { QUALITY_PROFILE_ITEM } from 'Helpers/dragTypes';
import QualityProfileItem from './QualityProfileItem';
import QualityProfileItemGroup from './QualityProfileItemGroup';
import styles from './QualityProfileItemDragSource.css';

const qualityProfileItemDragSource = {
  beginDrag(props) {
    const {
      editGroups,
      qualityIndex,
      groupId,
      qualityId,
      name,
      allowed
    } = props;

    return {
      editGroups,
      qualityIndex,
      groupId,
      qualityId,
      isGroup: !qualityId,
      name,
      allowed
    };
  },

  endDrag(props, monitor, component) {
    props.onQualityProfileItemDragEnd(monitor.didDrop());
  }
};

const qualityProfileItemDropTarget = {
  hover(props, monitor, component) {
    const {
      qualityIndex: dragQualityIndex,
      isGroup: isDragGroup
    } = monitor.getItem();

    const dropQualityIndex = props.qualityIndex;
    const isDropGroupItem = !!(props.qualityId && props.groupId);

    // Use childNodeIndex to select the correct node to get the middle of so
    // we don't bounce between above and below causing rapid setState calls.
    const childNodeIndex = component.props.isOverCurrent && component.props.isDraggingUp ? 1 :0;
    const componentDOMNode = findDOMNode(component).children[childNodeIndex];
    const hoverBoundingRect = componentDOMNode.getBoundingClientRect();
    const hoverMiddleY = (hoverBoundingRect.bottom - hoverBoundingRect.top) / 2;
    const clientOffset = monitor.getClientOffset();
    const hoverClientY = clientOffset.y - hoverBoundingRect.top;

    // If we're hovering over a child don't trigger on the parent
    if (!monitor.isOver({ shallow: true })) {
      return;
    }

    // Don't show targets for dropping on self
    if (dragQualityIndex === dropQualityIndex) {
      return;
    }

    // Don't allow a group to be dropped inside a group
    if (isDragGroup && isDropGroupItem) {
      return;
    }

    let dropPosition = null;

    // Determine drop position based on position over target
    if (hoverClientY > hoverMiddleY) {
      dropPosition = 'below';
    } else if (hoverClientY < hoverMiddleY) {
      dropPosition = 'above';
    } else {
      return;
    }

    props.onQualityProfileItemDragMove({
      dragQualityIndex,
      dropQualityIndex,
      dropPosition
    });
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
    isOver: monitor.isOver(),
    isOverCurrent: monitor.isOver({ shallow: true })
  };
}

class QualityProfileItemDragSource extends Component {

  //
  // Render

  render() {
    const {
      editGroups,
      groupId,
      qualityId,
      name,
      allowed,
      items,
      qualityIndex,
      isDragging,
      isDraggingUp,
      isDraggingDown,
      isOverCurrent,
      connectDragSource,
      connectDropTarget,
      onCreateGroupPress,
      onDeleteGroupPress,
      onQualityProfileItemAllowedChange,
      onItemGroupAllowedChange,
      onItemGroupNameChange,
      onQualityProfileItemDragMove,
      onQualityProfileItemDragEnd
    } = this.props;

    const isBefore = !isDragging && isDraggingUp && isOverCurrent;
    const isAfter = !isDragging && isDraggingDown && isOverCurrent;

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

        {
          !!groupId && qualityId == null &&
            <QualityProfileItemGroup
              editGroups={editGroups}
              groupId={groupId}
              name={name}
              allowed={allowed}
              items={items}
              qualityIndex={qualityIndex}
              isDragging={isDragging}
              isDraggingUp={isDraggingUp}
              isDraggingDown={isDraggingDown}
              connectDragSource={connectDragSource}
              onDeleteGroupPress={onDeleteGroupPress}
              onQualityProfileItemAllowedChange={onQualityProfileItemAllowedChange}
              onItemGroupAllowedChange={onItemGroupAllowedChange}
              onItemGroupNameChange={onItemGroupNameChange}
              onQualityProfileItemDragMove={onQualityProfileItemDragMove}
              onQualityProfileItemDragEnd={onQualityProfileItemDragEnd}
            />
        }

        {
          qualityId != null &&
            <QualityProfileItem
              editGroups={editGroups}
              groupId={groupId}
              qualityId={qualityId}
              name={name}
              allowed={allowed}
              qualityIndex={qualityIndex}
              isDragging={isDragging}
              isOverCurrent={isOverCurrent}
              connectDragSource={connectDragSource}
              onCreateGroupPress={onCreateGroupPress}
              onQualityProfileItemAllowedChange={onQualityProfileItemAllowedChange}
            />
        }

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
  editGroups: PropTypes.bool.isRequired,
  groupId: PropTypes.number,
  qualityId: PropTypes.number,
  name: PropTypes.string.isRequired,
  allowed: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object),
  qualityIndex: PropTypes.string.isRequired,
  isDragging: PropTypes.bool,
  isDraggingUp: PropTypes.bool,
  isDraggingDown: PropTypes.bool,
  isOverCurrent: PropTypes.bool,
  isInGroup: PropTypes.bool,
  connectDragSource: PropTypes.func,
  connectDropTarget: PropTypes.func,
  onCreateGroupPress: PropTypes.func,
  onDeleteGroupPress: PropTypes.func,
  onQualityProfileItemAllowedChange: PropTypes.func.isRequired,
  onItemGroupAllowedChange: PropTypes.func,
  onItemGroupNameChange: PropTypes.func,
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
