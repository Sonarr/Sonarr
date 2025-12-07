import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import Menu from 'Components/Menu/Menu';
import MenuButton from 'Components/Menu/MenuButton';
import MenuContent from 'Components/Menu/MenuContent';
import MenuItem from 'Components/Menu/MenuItem';
import MenuItemSeparator from 'Components/Menu/MenuItemSeparator';
import { align, icons, kinds } from 'Helpers/Props';
import { useSystemStatusData } from 'System/Status/useSystemStatus';
import { useRestart, useShutdown } from 'System/useSystem';
import translate from 'Utilities/String/translate';
import styles from './PageHeaderActionsMenu.css';

interface PageHeaderActionsMenuProps {
  onKeyboardShortcutsPress(): void;
}

function PageHeaderActionsMenu(props: PageHeaderActionsMenuProps) {
  const { onKeyboardShortcutsPress } = props;

  const { authentication, isContainerized } = useSystemStatusData();
  const { mutate: restart } = useRestart();
  const { mutate: shutdown } = useShutdown();

  const formsAuth = authentication === 'forms';

  const handleRestartPress = useCallback(() => {
    restart();
  }, [restart]);

  const handleShutdownPress = useCallback(() => {
    shutdown();
  }, [shutdown]);

  return (
    <div>
      <Menu alignMenu={align.RIGHT}>
        <MenuButton className={styles.menuButton} aria-label="Menu Button">
          <Icon name={icons.INTERACTIVE} title={translate('Menu')} />
        </MenuButton>

        <MenuContent>
          <MenuItem onPress={onKeyboardShortcutsPress}>
            <Icon className={styles.itemIcon} name={icons.KEYBOARD} />
            {translate('KeyboardShortcuts')}
          </MenuItem>

          <MenuItemSeparator />

          <MenuItem onPress={handleRestartPress}>
            <Icon className={styles.itemIcon} name={icons.RESTART} />
            {translate('Restart')}
          </MenuItem>

          {isContainerized ? null : (
            <MenuItem onPress={handleShutdownPress}>
              <Icon
                className={styles.itemIcon}
                name={icons.SHUTDOWN}
                kind={kinds.DANGER}
              />
              {translate('Shutdown')}
            </MenuItem>
          )}

          {formsAuth ? (
            <>
              <MenuItemSeparator />

              <MenuItem to={`${window.Sonarr.urlBase}/logout`} noRouter={true}>
                <Icon className={styles.itemIcon} name={icons.LOGOUT} />
                {translate('Logout')}
              </MenuItem>
            </>
          ) : null}
        </MenuContent>
      </Menu>
    </div>
  );
}

export default PageHeaderActionsMenu;
