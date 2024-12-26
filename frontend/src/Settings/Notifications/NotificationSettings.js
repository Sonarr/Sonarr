import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbar from 'Settings/SettingsToolbar';
import translate from 'Utilities/String/translate';
import NotificationsConnector from './Notifications/NotificationsConnector';

function NotificationSettings() {
  return (
    <PageContent title={translate('ConnectSettings')}>
      <SettingsToolbar
        showSave={false}
      />

      <PageContentBody>
        <NotificationsConnector />
      </PageContentBody>
    </PageContent>
  );
}

export default NotificationSettings;
