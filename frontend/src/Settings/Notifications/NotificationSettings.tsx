import React from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import SectionHeading from 'Components/SectionHeading';
import settingsStyles from 'Settings/Settings.css';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import Notifications from './Notifications/Notifications';

function NotificationSettings() {
  return (
    <SettingsPage title={translate('ConnectSettings')} showSave={false}>
      <PageContentBody>
        <div className={settingsStyles.section}>
          <PageHeading
            scope={translate('Settings')}
            title={translate('Connect')}
          />

          <div className={settingsStyles.pageSection}>
            <SectionHeading
              title={translate('Connections')}
              description={translate('NotificationsSectionDescription')}
            />

            <Notifications />
          </div>
        </div>
      </PageContentBody>
    </SettingsPage>
  );
}

export default NotificationSettings;
