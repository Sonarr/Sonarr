import React from 'react';
import Icon, { IconName } from 'Components/Icon';
import MenuButton from 'Components/Menu/MenuButton';
import { icons } from 'Helpers/Props';
import styles from './PageMenuButton.css';

interface PageMenuButtonProps {
  iconName: IconName;
  showIndicator?: boolean;
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
      <div className={styles.inner}>
        <div className={styles.iconWrapper}>
          <Icon name={iconName} size={18} />

          {showIndicator ? (
            <span className={styles.indicatorContainer}>
              <Icon name={icons.CIRCLE} size={9} />
            </span>
          ) : null}
        </div>

        <div className={styles.label}>{text}</div>
      </div>
    </MenuButton>
  );
}

export default PageMenuButton;
