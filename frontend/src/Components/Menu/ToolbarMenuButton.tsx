import classNames from 'classnames';
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
      <div>
        <Icon name={iconName} size={21} />

        {showIndicator ? (
          <span
            className={classNames(styles.indicatorContainer, 'fa-layers fa-fw')}
          >
            <Icon name={icons.CIRCLE} size={9} />
          </span>
        ) : null}

        <div className={styles.labelContainer}>
          <div className={styles.label}>{text}</div>
        </div>
      </div>
    </MenuButton>
  );
}

export default ToolbarMenuButton;
