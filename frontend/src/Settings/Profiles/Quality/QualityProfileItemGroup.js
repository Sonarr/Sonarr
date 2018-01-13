import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import CheckInput from 'Components/Form/CheckInput';
import TextInput from 'Components/Form/TextInput';
import QualityProfileItemDragSource from './QualityProfileItemDragSource';
import styles from './QualityProfileItemGroup.css';

class QualityProfileItemGroup extends Component {

  //
  // Listeners

  onAllowedChange = ({ value }) => {
    const {
      groupId,
      onItemGroupAllowedChange
    } = this.props;

    onItemGroupAllowedChange(groupId, value);
  }

  onNameChange = ({ value }) => {
    const {
      groupId,
      onItemGroupNameChange
    } = this.props;

    onItemGroupNameChange(groupId, value);
  }

  onDeleteGroupPress = ({ value }) => {
    const {
      groupId,
      onDeleteGroupPress
    } = this.props;

    onDeleteGroupPress(groupId, value);
  }

  //
  // Render

  render() {
    const {
      editGroups,
      groupId,
      name,
      allowed,
      items,
      qualityIndex,
      isDragging,
      isDraggingUp,
      isDraggingDown,
      connectDragSource,
      onQualityProfileItemAllowedChange,
      onQualityProfileItemDragMove,
      onQualityProfileItemDragEnd
    } = this.props;

    return (
      <div
        className={classNames(
          styles.qualityProfileItemGroup,
          editGroups && styles.editGroups,
          isDragging && styles.isDragging,
        )}
      >
        <div className={styles.qualityProfileItemGroupInfo}>
          {
            editGroups &&
              <div className={styles.qualityNameContainer}>
                <IconButton
                  className={styles.deleteGroupButton}
                  name={icons.UNGROUP}
                  title="Ungroup"
                  onPress={this.onDeleteGroupPress}
                />

                <TextInput
                  className={styles.nameInput}
                  name="name"
                  value={name}
                  onChange={this.onNameChange}
                />
              </div>
          }

          {
            !editGroups &&
              <label
                className={styles.qualityNameLabel}
              >
                <CheckInput
                  className={styles.checkInput}
                  containerClassName={styles.checkInputContainer}
                  name="allowed"
                  value={allowed}
                  onChange={this.onAllowedChange}
                />

                <div className={styles.nameContainer}>
                  <div className={classNames(
                    styles.name,
                    !allowed && styles.notAllowed
                  )}
                  >
                    {name}
                  </div>

                  <div className={styles.groupQualities}>
                    {
                      items.map(({ quality }) => {
                        return (
                          <Label key={quality.id}>
                            {quality.name}
                          </Label>
                        );
                      }).reverse()
                    }
                  </div>
                </div>
              </label>
          }

          {
            connectDragSource(
              <div className={styles.dragHandle}>
                <Icon
                  className={styles.dragIcon}
                  name={icons.REORDER}
                  title="Reorder"
                />
              </div>
            )
          }
        </div>

        {
          editGroups &&
            <div className={styles.items}>
              {
                items.map(({ quality }, index) => {
                  return (
                    <QualityProfileItemDragSource
                      key={quality.id}
                      editGroups={editGroups}
                      groupId={groupId}
                      qualityId={quality.id}
                      name={quality.name}
                      allowed={allowed}
                      items={items}
                      qualityIndex={`${qualityIndex}.${index + 1}`}
                      isDragging={isDragging}
                      isDraggingUp={isDraggingUp}
                      isDraggingDown={isDraggingDown}
                      isInGroup={true}
                      onQualityProfileItemAllowedChange={onQualityProfileItemAllowedChange}
                      onQualityProfileItemDragMove={onQualityProfileItemDragMove}
                      onQualityProfileItemDragEnd={onQualityProfileItemDragEnd}
                    />
                  );
                }).reverse()
              }
            </div>
        }
      </div>
    );
  }
}

QualityProfileItemGroup.propTypes = {
  editGroups: PropTypes.bool,
  groupId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  allowed: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  qualityIndex: PropTypes.string.isRequired,
  isDragging: PropTypes.bool.isRequired,
  isDraggingUp: PropTypes.bool.isRequired,
  isDraggingDown: PropTypes.bool.isRequired,
  connectDragSource: PropTypes.func,
  onItemGroupAllowedChange: PropTypes.func.isRequired,
  onQualityProfileItemAllowedChange: PropTypes.func.isRequired,
  onItemGroupNameChange: PropTypes.func.isRequired,
  onDeleteGroupPress: PropTypes.func.isRequired,
  onQualityProfileItemDragMove: PropTypes.func.isRequired,
  onQualityProfileItemDragEnd: PropTypes.func.isRequired
};

QualityProfileItemGroup.defaultProps = {
  // The drag preview will not connect the drag handle.
  connectDragSource: (node) => node
};

export default QualityProfileItemGroup;
