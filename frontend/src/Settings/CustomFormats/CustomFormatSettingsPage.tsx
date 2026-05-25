import React, { useCallback, useMemo, useState } from 'react';
import { DndProvider } from 'react-dnd';
import { HTML5Backend } from 'react-dnd-html5-backend';
import PageContentBody from 'Components/Page/PageContentBody';
import { OverflowDivider } from 'Components/Page/Toolbar/Overflow';
import { type MoreMenuItem } from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import { icons } from 'Helpers/Props';
import useParseModal from 'Parse/useParseModal';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import CustomFormats from './CustomFormats/CustomFormats';
import ManageCustomFormatsToolbarButton from './CustomFormats/Manage/ManageCustomFormatsToolbarButton';

function CustomFormatSettingsPage() {
  const { open: onParseModalPress, modal: parseModal } = useParseModal();

  const [isManageCustomFormatsOpen, setIsManageCustomFormatsOpen] =
    useState(false);

  const handleManageCustomFormatsPress = useCallback(() => {
    setIsManageCustomFormatsOpen(true);
  }, []);

  const handleManageCustomFormatsClose = useCallback(() => {
    setIsManageCustomFormatsOpen(false);
  }, []);

  const moreMenuItems = useMemo<MoreMenuItem[]>(
    () => [
      {
        id: 'test-parsing',
        label: translate('TestParsing'),
        iconName: icons.PARSE,
        onPress: onParseModalPress,
      },
      {
        id: 'manage-custom-formats',
        label: translate('ManageFormats'),
        iconName: icons.MANAGE,
        onPress: handleManageCustomFormatsPress,
      },
    ],
    [onParseModalPress, handleManageCustomFormatsPress]
  );

  return (
    <SettingsPage
      title={translate('CustomFormatsSettings')}
      showSave={false}
      moreMenuItems={moreMenuItems}
      toolbarChildren={
        <>
          <OverflowDivider groupId="extras">
            <PageToolbarSeparator />
          </OverflowDivider>

          <ToolbarItem id="test-parsing" priority={1} groupId="extras">
            <PageToolbarButton
              label={translate('TestParsing')}
              iconName={icons.PARSE}
              onPress={onParseModalPress}
            />
          </ToolbarItem>

          <ToolbarItem id="manage-custom-formats" priority={1} groupId="extras">
            <ManageCustomFormatsToolbarButton
              isOpen={isManageCustomFormatsOpen}
              onPress={handleManageCustomFormatsPress}
              onModalClose={handleManageCustomFormatsClose}
            />
          </ToolbarItem>
        </>
      }
    >
      <PageContentBody>
        <DndProvider backend={HTML5Backend}>
          <CustomFormats />
        </DndProvider>
      </PageContentBody>

      {parseModal}
    </SettingsPage>
  );
}

export default CustomFormatSettingsPage;
