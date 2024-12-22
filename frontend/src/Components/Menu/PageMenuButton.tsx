import { IconName } from '@fortawesome/free-regular-svg-icons';
import classNames from 'classnames';
import React from 'react';
import Icon from 'Components/Icon';
import MenuButton from 'Components/Menu/MenuButton';
import { icons } from 'Helpers/Props';
import styles from './PageMenuButton.css';

interface PageMenuButtonProps {
  iconName: IconName;
  showIndicator: boolean;
  text?: string;
}

function PageMenuButton({
  iconName,
  showIndicator = false,
  text,
  ...otherProps
}: PageMenuButtonProps) {
  return (
    <MenuButton className={styles.menuButton} {...otherProps}>
      <Icon name={iconName} size={18} />

      {showIndicator ? (
        <span
          className={classNames(styles.indicatorContainer, 'fa-layers fa-fw')}
        >
          <Icon name={icons.CIRCLE} size={9} />
        </span>
      ) : null}

      <div className={styles.label}>{text}</div>
    </MenuButton>
  );
}

export default PageMenuButton;
