import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import styles from './EnhancedSelectInputOption.css';

class EnhancedSelectInputOption extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      id,
      onSelect
    } = this.props;

    onSelect(id);
  }

  //
  // Render

  render() {
    const {
      className,
      isSelected,
      isDisabled,
      isHidden,
      isMobile,
      children
    } = this.props;

    return (
      <Link
        className={classNames(
          className,
          isSelected && styles.isSelected,
          isDisabled && styles.isDisabled,
          isHidden && styles.isHidden,
          isMobile && styles.isMobile
        )}
        component="div"
        isDisabled={isDisabled}
        onPress={this.onPress}
      >
        {children}

        {
          isMobile &&
            <div className={styles.iconContainer}>
              <Icon
                name={isSelected ? icons.CHECK_CIRCLE : icons.CIRCLE_OUTLINE}
              />
            </div>
        }
      </Link>
    );
  }
}

EnhancedSelectInputOption.propTypes = {
  className: PropTypes.string.isRequired,
  id: PropTypes.string.isRequired,
  isSelected: PropTypes.bool.isRequired,
  isDisabled: PropTypes.bool.isRequired,
  isHidden: PropTypes.bool.isRequired,
  isMobile: PropTypes.bool.isRequired,
  children: PropTypes.node.isRequired,
  onSelect: PropTypes.func.isRequired
};

EnhancedSelectInputOption.defaultProps = {
  className: styles.option,
  isDisabled: false,
  isHidden: false
};

export default EnhancedSelectInputOption;
