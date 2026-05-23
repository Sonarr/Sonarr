import React from 'react';
import Icon, { IconName } from 'Components/Icon';
import MenuButton, { MenuButtonProps } from 'Components/Menu/MenuButton';
import { icons } from 'Helpers/Props';
import styles from './ToolbarMenuButton.css';

export interface ToolbarMenuButtonProps
  extends Omit<MenuButtonProps, 'children'> {
  className?: string;
  iconName: IconName;
  showIndicator?: boolean;
  text?: string;
}

function ToolbarMenuButton({
  iconName,
  showIndicator = false,
  text,
  ...otherProps
}: ToolbarMenuButtonProps) {
  return (
    <MenuButton className={styles.menuButton} {...otherProps}>
      <div className={styles.inner}>
        <div className={styles.iconWrapper}>
          <Icon name={iconName} size={16} />

          {showIndicator ? (
            <span className={styles.indicatorContainer}>
              <Icon name={icons.CIRCLE} size={7} />
            </span>
          ) : null}
        </div>

        <span className={styles.label}>{text}</span>
      </div>
    </MenuButton>
  );
}

export default ToolbarMenuButton;
