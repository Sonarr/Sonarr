import React, { useCallback, useState } from 'react';
import { DndProvider } from 'react-dnd';
import { HTML5Backend } from 'react-dnd-html5-backend';
import PageContentBody from 'Components/Page/PageContentBody';
import { OverflowDivider } from 'Components/Page/Toolbar/Overflow';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import { icons } from 'Helpers/Props';
import ParseModal from 'Parse/ParseModal';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import CustomFormats from './CustomFormats/CustomFormats';
import ManageCustomFormatsModal from './CustomFormats/Manage/ManageCustomFormatsModal';

function CustomFormatSettingsPage() {
  const [isParseModalOpen, setIsParseModalOpen] = useState(false);
  const [isManageCustomFormatsOpen, setIsManageCustomFormatsOpen] =
    useState(false);

  const handleParseModalPress = useCallback(() => {
    setIsParseModalOpen(true);
  }, []);

  const handleParseModalClose = useCallback(() => {
    setIsParseModalOpen(false);
  }, []);

  const handleManageCustomFormatsPress = useCallback(() => {
    setIsManageCustomFormatsOpen(true);
  }, []);

  const handleManageCustomFormatsClose = useCallback(() => {
    setIsManageCustomFormatsOpen(false);
  }, []);

  return (
    <SettingsPage
      title={translate('CustomFormatsSettings')}
      showSave={false}
      toolbarChildren={
        <>
          <OverflowDivider groupId="extras">
            <PageToolbarSeparator />
          </OverflowDivider>

          <ToolbarItem
            id="test-parsing"
            priority={1}
            groupId="extras"
            label={translate('TestParsing')}
            iconName={icons.PARSE}
            onPress={handleParseModalPress}
          />

          <ToolbarItem
            id="manage-custom-formats"
            priority={1}
            groupId="extras"
            label={translate('ManageFormats')}
            iconName={icons.MANAGE}
            onPress={handleManageCustomFormatsPress}
          />
        </>
      }
    >
      <PageContentBody>
        <DndProvider backend={HTML5Backend}>
          <CustomFormats />
        </DndProvider>
      </PageContentBody>

      <ParseModal
        isOpen={isParseModalOpen}
        onModalClose={handleParseModalClose}
      />

      <ManageCustomFormatsModal
        isOpen={isManageCustomFormatsOpen}
        onModalClose={handleManageCustomFormatsClose}
      />
    </SettingsPage>
  );
}

export default CustomFormatSettingsPage;
