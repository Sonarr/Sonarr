import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import MenuButton from 'Components/Menu/MenuButton';
import { icons } from 'Helpers/Props';
import styles from './ToolbarMenuButton.css';

function ToolbarMenuButton(props) {
  const {
    iconName,
    showIndicator,
    text,
    ...otherProps
  } = props;

  return (
    <MenuButton
      className={styles.menuButton}
      {...otherProps}
    >
      <div>
        <Icon
          name={iconName}
          size={21}
        />

        {
          showIndicator ?
            <span
              className={classNames(
                styles.indicatorContainer,
                'fa-layers fa-fw'
              )}
            >
              <Icon
                name={icons.CIRCLE}
                size={9}
              />
            </span> :
            null
        }

        <div className={styles.labelContainer}>
          <div className={styles.label}>
            {text}
          </div>
        </div>
      </div>
    </MenuButton>
  );
}

ToolbarMenuButton.propTypes = {
  iconName: PropTypes.object.isRequired,
  showIndicator: PropTypes.bool.isRequired,
  text: PropTypes.string
};

ToolbarMenuButton.defaultProps = {
  showIndicator: false
};

export default ToolbarMenuButton;
