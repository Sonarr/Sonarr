import React from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import Notifications from './Notifications/Notifications';

function NotificationSettings() {
  return (
    <SettingsPage title={translate('ConnectSettings')} showSave={false}>
      <PageContentBody>
        <Notifications />
      </PageContentBody>
    </SettingsPage>
  );
}

export default NotificationSettings;
