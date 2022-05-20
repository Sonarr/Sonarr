import React, { Component } from 'react';
import { DndProvider } from 'react-dnd-multi-backend';
import HTML5toTouch from 'react-dnd-multi-backend/dist/esm/HTML5toTouch';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import DelayProfilesConnector from './Delay/DelayProfilesConnector';
import LanguageProfilesConnector from './Language/LanguageProfilesConnector';
import QualityProfilesConnector from './Quality/QualityProfilesConnector';
import ReleaseProfilesConnector from './Release/ReleaseProfilesConnector';

// Only a single DragDrop Context can exist so it's done here to allow editing
// quality profiles and reordering delay profiles to work.

class Profiles extends Component {

  //
  // Render

  render() {
    return (
      <PageContent title="Profiles">
        <SettingsToolbarConnector showSave={false} />

        <PageContentBody>
          <DndProvider options={HTML5toTouch}>
            <QualityProfilesConnector />
            <LanguageProfilesConnector />
            <DelayProfilesConnector />
            <ReleaseProfilesConnector />
          </DndProvider>
        </PageContentBody>
      </PageContent>
    );
  }
}

export default Profiles;
