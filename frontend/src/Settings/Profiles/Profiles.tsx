import { HTML5toTouch } from 'rdndmb-html5-to-touch';
import React from 'react';
import { DndProvider } from 'react-dnd-multi-backend';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import DelayProfiles from './Delay/DelayProfiles';
import QualityProfiles from './Quality/QualityProfiles';
import ReleaseProfiles from './Release/ReleaseProfiles';

// Only a single DragDrop Context can exist so it's done here to allow editing
// quality profiles and reordering delay profiles to work.

function Profiles() {
  return (
    <SettingsPage title={translate('Profiles')} showSave={false}>
      <PageContentBody>
        <DndProvider options={HTML5toTouch}>
          <QualityProfiles />
          <DelayProfiles />
          <ReleaseProfiles />
        </DndProvider>
      </PageContentBody>
    </SettingsPage>
  );
}

export default Profiles;
