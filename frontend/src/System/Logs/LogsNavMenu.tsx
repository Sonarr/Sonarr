import React, { useCallback, useState } from 'react';
import Menu from 'Components/Menu/Menu';
import MenuButton from 'Components/Menu/MenuButton';
import MenuContent from 'Components/Menu/MenuContent';
import MenuItem from 'Components/Menu/MenuItem';
import translate from 'Utilities/String/translate';

interface LogsNavMenuProps {
  current: string;
}

function LogsNavMenu({ current }: LogsNavMenuProps) {
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const handleMenuButtonPress = useCallback(() => {
    setIsMenuOpen((prevIsMenuOpen) => !prevIsMenuOpen);
  }, []);

  return (
    <Menu>
      <MenuButton onPress={handleMenuButtonPress}>{current}</MenuButton>
      <MenuContent isOpen={isMenuOpen}>
        <MenuItem to="/system/logs/files">{translate('LogFiles')}</MenuItem>

        <MenuItem to="/system/logs/files/update">
          {translate('UpdaterLogFiles')}
        </MenuItem>
      </MenuContent>
    </Menu>
  );
}

export default LogsNavMenu;
