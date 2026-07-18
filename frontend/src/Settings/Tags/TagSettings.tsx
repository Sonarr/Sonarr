import React from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import SectionHeading from 'Components/SectionHeading';
import settingsStyles from 'Settings/Settings.css';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import AutoTaggings from './AutoTagging/AutoTaggings';
import Tags from './Tags';

function TagSettings() {
  return (
    <SettingsPage title={translate('Tags')} showSave={false}>
      <PageContentBody>
        <div className={settingsStyles.section}>
          <PageHeading
            scope={translate('Settings')}
            title={translate('Tags')}
          />

          <div className={settingsStyles.pageSection}>
            <SectionHeading
              title={translate('Tags')}
              description={translate('TagsSectionDescription')}
            />

            <Tags />
          </div>

          <div className={settingsStyles.pageSection}>
            <SectionHeading
              title={translate('AutoTagging')}
              description={translate('AutoTaggingSectionDescription')}
            />

            <AutoTaggings />
          </div>
        </div>
      </PageContentBody>
    </SettingsPage>
  );
}

export default TagSettings;
