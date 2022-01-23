import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import MenuButton from 'Components/Menu/MenuButton';
import { icons } from 'Helpers/Props';
import styles from './PageMenuButton.css';

function PageMenuButton(props) {
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
      <Icon
        name={iconName}
        size={18}
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

      <div className={styles.label}>
        {text}
      </div>
    </MenuButton>
  );
}

PageMenuButton.propTypes = {
  iconName: PropTypes.object.isRequired,
  showIndicator: PropTypes.bool.isRequired,
  text: PropTypes.string
};

PageMenuButton.defaultProps = {
  showIndicator: false
};

export default PageMenuButton;
