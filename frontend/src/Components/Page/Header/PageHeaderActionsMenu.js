import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import Menu from 'Components/Menu/Menu';
import MenuButton from 'Components/Menu/MenuButton';
import MenuContent from 'Components/Menu/MenuContent';
import MenuItem from 'Components/Menu/MenuItem';
import MenuItemSeparator from 'Components/Menu/MenuItemSeparator';
import { align, icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './PageHeaderActionsMenu.css';

function PageHeaderActionsMenu(props) {
  const {
    formsAuth,
    onKeyboardShortcutsPress,
    onRestartPress,
    onShutdownPress
  } = props;

  return (
    <div>
      <Menu alignMenu={align.RIGHT}>
        <MenuButton className={styles.menuButton} aria-label="Menu Button">
          <Icon
            name={icons.INTERACTIVE}
            title={translate('Menu')}
          />
        </MenuButton>

        <MenuContent>
          <MenuItem onPress={onKeyboardShortcutsPress}>
            <Icon
              className={styles.itemIcon}
              name={icons.KEYBOARD}
            />
            {translate('KeyboardShortcuts')}
          </MenuItem>

          <MenuItemSeparator />

          <MenuItem onPress={onRestartPress}>
            <Icon
              className={styles.itemIcon}
              name={icons.RESTART}
            />
            {translate('Restart')}
          </MenuItem>

          <MenuItem onPress={onShutdownPress}>
            <Icon
              className={styles.itemIcon}
              name={icons.SHUTDOWN}
              kind={kinds.DANGER}
            />
            {translate('Shutdown')}
          </MenuItem>

          {
            formsAuth &&
              <div className={styles.separator} />
          }

          {
            formsAuth &&
              <MenuItem
                to={`${window.Sonarr.urlBase}/logout`}
                noRouter={true}
              >
                <Icon
                  className={styles.itemIcon}
                  name={icons.LOGOUT}
                />
                {translate('Logout')}
              </MenuItem>
          }
        </MenuContent>
      </Menu>
    </div>
  );
}

PageHeaderActionsMenu.propTypes = {
  formsAuth: PropTypes.bool.isRequired,
  onKeyboardShortcutsPress: PropTypes.func.isRequired,
  onRestartPress: PropTypes.func.isRequired,
  onShutdownPress: PropTypes.func.isRequired
};

export default PageHeaderActionsMenu;
