import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import QualityProfileItemSize from './QualityProfileItemSize';
import styles from './QualityProfileItem.css';

class QualityProfileItem extends Component {

  //
  // Listeners

  onAllowedChange = ({ value }) => {
    const {
      qualityId,
      onQualityProfileItemAllowedChange
    } = this.props;

    onQualityProfileItemAllowedChange(qualityId, value);
  };

  onCreateGroupPress = () => {
    const {
      qualityId,
      onCreateGroupPress
    } = this.props;

    onCreateGroupPress(qualityId);
  };

  //
  // Render

  render() {
    const {
      mode,
      isPreview,
      qualityId,
      groupId,
      name,
      allowed,
      minSize,
      maxSize,
      preferredSize,
      isDragging,
      isOverCurrent,
      connectDragSource,
      onSizeChange
    } = this.props;

    return (
      <div
        className={classNames(
          styles.qualityProfileItem,
          mode === 'editSizes' && styles.editSizes,
          isDragging && styles.isDragging,
          isPreview && styles.isPreview,
          isOverCurrent && styles.isOverCurrent,
          groupId && styles.isInGroup
        )}
      >
        <label
          className={classNames(
            styles.qualityNameContainer,
            mode === 'editSizes' && styles.editSizes
          )}
        >
          {
            mode === 'editGroups' && !groupId && !isPreview &&
              <IconButton
                className={styles.createGroupButton}
                name={icons.GROUP}
                title={translate('Group')}
                onPress={this.onCreateGroupPress}
              />
          }

          {
            mode === 'default' &&
              <CheckInput
                className={styles.checkInput}
                containerClassName={styles.checkInputContainer}
                name={name}
                value={allowed}
                isDisabled={!!groupId}
                onChange={this.onAllowedChange}
              />
          }

          <div className={classNames(
            styles.qualityName,
            groupId && mode !== 'editSizes' && styles.isInGroup,
            !allowed && styles.notAllowed
          )}
          >
            {name}
          </div>
        </label>

        {
          mode === 'editSizes' && qualityId != null ?
            <div>
              <QualityProfileItemSize
                id={qualityId}
                minSize={minSize}
                maxSize={maxSize}
                preferredSize={preferredSize}
                onSizeChange={onSizeChange}
              />
            </div> :
            null
        }

        {
          mode === 'editSizes' ? null :
            connectDragSource(
              <div className={styles.dragHandle}>
                <Icon
                  className={styles.dragIcon}
                  title={translate('CreateGroup')}
                  name={icons.REORDER}
                />
              </div>
            )
        }
      </div>
    );
  }
}

QualityProfileItem.propTypes = {
  mode: PropTypes.string.isRequired,
  isPreview: PropTypes.bool,
  groupId: PropTypes.number,
  qualityId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  allowed: PropTypes.bool.isRequired,
  minSize: PropTypes.number,
  maxSize: PropTypes.number,
  preferredSize: PropTypes.number,
  isDragging: PropTypes.bool.isRequired,
  isOverCurrent: PropTypes.bool.isRequired,
  isInGroup: PropTypes.bool,
  connectDragSource: PropTypes.func,
  onCreateGroupPress: PropTypes.func,
  onQualityProfileItemAllowedChange: PropTypes.func,
  onSizeChange: PropTypes.func
};

QualityProfileItem.defaultProps = {
  mode: 'default',
  isPreview: false,
  isOverCurrent: false,
  // The drag preview will not connect the drag handle.
  connectDragSource: (node) => node
};

export default QualityProfileItem;
