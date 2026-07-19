import React from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import SectionHeading from 'Components/SectionHeading';
import settingsStyles from 'Settings/Settings.css';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import DelayProfiles from './Delay/DelayProfiles';
import QualityProfiles from './Quality/QualityProfiles';
import ReleaseProfiles from './Release/ReleaseProfiles';

function Profiles() {
  return (
    <SettingsPage title={translate('Profiles')} showSave={false}>
      <PageContentBody>
        <div className={settingsStyles.section}>
          <PageHeading
            scope={translate('Settings')}
            title={translate('Profiles')}
          />

          <div className={settingsStyles.pageSection}>
            <SectionHeading
              title={translate('QualityProfiles')}
              description={translate('QualityProfilesSectionDescription')}
            />

            <QualityProfiles />
          </div>

          <div className={settingsStyles.pageSection}>
            <SectionHeading
              title={translate('DelayProfiles')}
              description={translate('DelayProfilesSectionDescription')}
            />

            <DelayProfiles />
          </div>

          <div className={settingsStyles.pageSection}>
            <SectionHeading
              title={translate('ReleaseProfiles')}
              description={translate('ReleaseProfilesSectionDescription')}
            />

            <ReleaseProfiles />
          </div>
        </div>
      </PageContentBody>
    </SettingsPage>
  );
}

export default Profiles;
