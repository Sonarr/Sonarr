import React from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
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
            scope={`${translate('Configuration')} · ${translate('Connect')}`}
            title={translate('ConnectSettings')}
          />

          <Notifications />
        </div>
      </PageContentBody>
    </SettingsPage>
  );
}

export default NotificationSettings;
