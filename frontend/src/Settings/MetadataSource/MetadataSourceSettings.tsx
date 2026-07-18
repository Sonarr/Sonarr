import React from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import settingsStyles from 'Settings/Settings.css';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import TheTvdb from './TheTvdb';

function MetadataSourceSettings() {
  return (
    <SettingsPage title={translate('MetadataSourceSettings')} showSave={false}>
      <PageContentBody>
        <div className={settingsStyles.section}>
          <PageHeading
            scope={translate('Settings')}
            title={translate('MetadataSource')}
          />
          <TheTvdb />
        </div>
      </PageContentBody>
    </SettingsPage>
  );
}

export default MetadataSourceSettings;
