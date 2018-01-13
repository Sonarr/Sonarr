import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import MenuButton from 'Components/Menu/MenuButton';
import styles from './ToolbarMenuButton.css';

function ToolbarMenuButton(props) {
  const {
    iconName,
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

        <div className={styles.label}>
          {text}
        </div>
      </div>
    </MenuButton>
  );
}

ToolbarMenuButton.propTypes = {
  iconName: PropTypes.object.isRequired,
  text: PropTypes.string
};

export default ToolbarMenuButton;
