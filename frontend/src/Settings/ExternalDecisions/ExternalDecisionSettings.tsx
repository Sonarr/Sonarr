import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbar from 'Settings/SettingsToolbar';
import translate from 'Utilities/String/translate';
import ExternalDecisions from './ExternalDecisions/ExternalDecisions';

function ExternalDecisionSettings() {
  return (
    <PageContent title={translate('ExternalDecisionSettings')}>
      <SettingsToolbar showSave={false} />

      <PageContentBody>
        <ExternalDecisions />
      </PageContentBody>
    </PageContent>
  );
}

export default ExternalDecisionSettings;
