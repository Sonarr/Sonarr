import React from 'react';
import Menu from 'Components/Menu/Menu';
import ToolbarMenuButton, {
  ToolbarMenuButtonProps,
} from 'Components/Menu/ToolbarMenuButton';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

interface ViewMenuProps extends Omit<ToolbarMenuButtonProps, 'iconName'> {
  children: React.ReactNode;
  isDisabled?: boolean;
  alignMenu?: 'left' | 'right';
}

function ViewMenu({
  children,
  isDisabled = false,
  ...otherProps
}: ViewMenuProps) {
  return (
    <Menu {...otherProps}>
      <ToolbarMenuButton
        iconName={icons.VIEW}
        text={translate('View')}
        isDisabled={isDisabled}
      />
      {children}
    </Menu>
  );
}

export default ViewMenu;
