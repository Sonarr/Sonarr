import React, { useCallback, useState } from 'react';
import Icon from 'Components/Icon';
import Menu from 'Components/Menu/Menu';
import MenuButton from 'Components/Menu/MenuButton';
import MenuContent from 'Components/Menu/MenuContent';
import MenuItem from 'Components/Menu/MenuItem';
import { align, icons } from 'Helpers/Props';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import ParseModal from 'Parse/ParseModal';
import translate from 'Utilities/String/translate';
import styles from './PageHeaderToolsMenu.module.css';

function PageHeaderToolsMenu() {
  const [isParseModalOpen, setIsParseModalOpen] = useState(false);
  const [isInteractiveImportModalOpen, setIsInteractiveImportModalOpen] =
    useState(false);

  const handleParsePress = useCallback(() => {
    setIsParseModalOpen(true);
  }, []);

  const handleParseModalClose = useCallback(() => {
    setIsParseModalOpen(false);
  }, []);

  const handleInteractiveImportPress = useCallback(() => {
    setIsInteractiveImportModalOpen(true);
  }, []);

  const handleInteractiveImportModalClose = useCallback(() => {
    setIsInteractiveImportModalOpen(false);
  }, []);

  return (
    <div>
      <Menu alignMenu={align.RIGHT}>
        <MenuButton
          className={styles.menuButton}
          aria-label={translate('Tools')}
        >
          <Icon
            name={icons.TOOLS}
            title={translate('Tools')}
            titleWrapperClassName={styles.menuButtonIcon}
          />
        </MenuButton>

        <MenuContent>
          <MenuItem onPress={handleParsePress}>
            <Icon className={styles.itemIcon} name={icons.PARSE} />
            {translate('TestParsing')}
          </MenuItem>

          <MenuItem onPress={handleInteractiveImportPress}>
            <Icon className={styles.itemIcon} name={icons.INTERACTIVE_IMPORT} />
            {translate('ManualImport')}
          </MenuItem>
        </MenuContent>
      </Menu>

      <ParseModal
        isOpen={isParseModalOpen}
        onModalClose={handleParseModalClose}
      />

      <InteractiveImportModal
        isOpen={isInteractiveImportModalOpen}
        onModalClose={handleInteractiveImportModalClose}
      />
    </div>
  );
}

export default PageHeaderToolsMenu;
