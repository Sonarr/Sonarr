import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import MenuButton from 'Components/Menu/MenuButton';
import styles from './PageMenuButton.css';

function PageMenuButton(props) {
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
      <Icon
        name={iconName}
        size={18}
      />

      <div className={styles.label}>
        {text}
      </div>
    </MenuButton>
  );
}

PageMenuButton.propTypes = {
  iconName: PropTypes.object.isRequired,
  text: PropTypes.string
};

export default PageMenuButton;
