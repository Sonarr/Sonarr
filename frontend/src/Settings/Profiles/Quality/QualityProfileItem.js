import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
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
      editGroups,
      isPreview,
      groupId,
      name,
      allowed,
      isDragging,
      isOverCurrent,
      connectDragSource
    } = this.props;

    return (
      <div
        className={classNames(
          styles.qualityProfileItem,
          isDragging && styles.isDragging,
          isPreview && styles.isPreview,
          isOverCurrent && styles.isOverCurrent,
          groupId && styles.isInGroup
        )}
      >
        <label
          className={styles.qualityNameContainer}
        >
          {
            editGroups && !groupId && !isPreview &&
              <IconButton
                className={styles.createGroupButton}
                name={icons.GROUP}
                title={translate('Group')}
                onPress={this.onCreateGroupPress}
              />
          }

          {
            !editGroups &&
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
            groupId && styles.isInGroup,
            !allowed && styles.notAllowed
          )}
          >
            {name}
          </div>
        </label>

        {
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
  editGroups: PropTypes.bool,
  isPreview: PropTypes.bool,
  groupId: PropTypes.number,
  qualityId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  allowed: PropTypes.bool.isRequired,
  isDragging: PropTypes.bool.isRequired,
  isOverCurrent: PropTypes.bool.isRequired,
  isInGroup: PropTypes.bool,
  connectDragSource: PropTypes.func,
  onCreateGroupPress: PropTypes.func,
  onQualityProfileItemAllowedChange: PropTypes.func
};

QualityProfileItem.defaultProps = {
  isPreview: false,
  isOverCurrent: false,
  // The drag preview will not connect the drag handle.
  connectDragSource: (node) => node
};

export default QualityProfileItem;
