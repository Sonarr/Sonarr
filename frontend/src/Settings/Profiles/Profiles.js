import React, { Component } from 'react';
import { DndProvider } from 'react-dnd';
import HTML5Backend from 'react-dnd-html5-backend';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import QualityProfilesConnector from './Quality/QualityProfilesConnector';
import LanguageProfilesConnector from './Language/LanguageProfilesConnector';
import DelayProfilesConnector from './Delay/DelayProfilesConnector';
import ReleaseProfilesConnector from './Release/ReleaseProfilesConnector';

// Only a single DragDrop Context can exist so it's done here to allow editing
// quality profiles and reordering delay profiles to work.

class Profiles extends Component {

  //
  // Render

  render() {
    return (
      <PageContent title="Profiles">
        <SettingsToolbarConnector
          showSave={false}
        />

        <PageContentBodyConnector>
          <DndProvider backend={HTML5Backend}>
            <QualityProfilesConnector />
            <LanguageProfilesConnector />
            <DelayProfilesConnector />
            <ReleaseProfilesConnector />
          </DndProvider>
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

export default Profiles;
