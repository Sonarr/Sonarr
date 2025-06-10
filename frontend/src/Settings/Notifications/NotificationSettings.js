import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import translate from 'Utilities/String/translate';
import NotificationsConnector from './Notifications/NotificationsConnector';
import NotificationTemplates from './NotificationTemplates/NotificationTemplates';

function NotificationSettings() {
  return (
    <PageContent title={translate('ConnectSettings')}>
      <SettingsToolbarConnector
        showSave={false}
      />

      <PageContentBody>
        <NotificationsConnector />
        <NotificationTemplates />
      </PageContentBody>
    </PageContent>
  );
}

export default NotificationSettings;
