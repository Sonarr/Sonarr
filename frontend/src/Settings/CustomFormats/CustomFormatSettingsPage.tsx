import React, { useCallback, useState } from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import { OverflowDivider } from 'Components/Page/Toolbar/Overflow';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import SectionHeading from 'Components/SectionHeading';
import { icons } from 'Helpers/Props';
import settingsStyles from 'Settings/Settings.module.css';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import CustomFormats from './CustomFormats/CustomFormats';
import ManageCustomFormatsModal from './CustomFormats/Manage/ManageCustomFormatsModal';

function CustomFormatSettingsPage() {
  const [isManageCustomFormatsOpen, setIsManageCustomFormatsOpen] =
    useState(false);

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
        <div className={settingsStyles.section}>
          <PageHeading
            scope={translate('Settings')}
            title={translate('CustomFormats')}
          />

          <div className={settingsStyles.pageSection}>
            <SectionHeading
              title={translate('CustomFormats')}
              description={translate('CustomFormatsSectionDescription')}
            />

            <CustomFormats />
          </div>
        </div>
      </PageContentBody>

      <ManageCustomFormatsModal
        isOpen={isManageCustomFormatsOpen}
        onModalClose={handleManageCustomFormatsClose}
      />
    </SettingsPage>
  );
}

export default CustomFormatSettingsPage;
