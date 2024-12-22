import React from 'react';
import Menu from 'Components/Menu/Menu';
import ToolbarMenuButton, {
  ToolbarMenuButtonProps,
} from 'Components/Menu/ToolbarMenuButton';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

interface SortMenuProps extends Omit<ToolbarMenuButtonProps, 'iconName'> {
  className?: string;
  children: React.ReactNode;
  isDisabled?: boolean;
  alignMenu?: 'left' | 'right';
}

function SortMenu({
  className,
  children,
  isDisabled = false,
  ...otherProps
}: SortMenuProps) {
  return (
    <Menu className={className} {...otherProps}>
      <ToolbarMenuButton
        iconName={icons.SORT}
        text={translate('Sort')}
        isDisabled={isDisabled}
      />
      {children}
    </Menu>
  );
}

export default SortMenu;
