import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import styles from './LanguageProfileItem.css';

class LanguageProfileItem extends Component {

  //
  // Listeners

  onAllowedChange = ({ value }) => {
    const {
      languageId,
      onLanguageProfileItemAllowedChange
    } = this.props;

    onLanguageProfileItemAllowedChange(languageId, value);
  };

  //
  // Render

  render() {
    const {
      name,
      allowed,
      isDragging,
      connectDragSource
    } = this.props;

    return (
      <div
        className={classNames(
          styles.languageProfileItem,
          isDragging && styles.isDragging
        )}
      >
        <label
          className={styles.languageName}
        >
          <CheckInput
            containerClassName={styles.checkContainer}
            name={name}
            value={allowed}
            onChange={this.onAllowedChange}
          />
          {name}
        </label>

        {
          connectDragSource(
            <div className={styles.dragHandle}>
              <Icon
                className={styles.dragIcon}
                name={icons.REORDER}
              />
            </div>
          )
        }
      </div>
    );
  }
}

LanguageProfileItem.propTypes = {
  languageId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  allowed: PropTypes.bool.isRequired,
  sortIndex: PropTypes.number.isRequired,
  isDragging: PropTypes.bool.isRequired,
  connectDragSource: PropTypes.func,
  onLanguageProfileItemAllowedChange: PropTypes.func
};

LanguageProfileItem.defaultProps = {
  // The drag preview will not connect the drag handle.
  connectDragSource: (node) => node
};

export default LanguageProfileItem;
