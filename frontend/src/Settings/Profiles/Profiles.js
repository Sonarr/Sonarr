import React, { Component } from 'react';
import { DragDropContext } from 'react-dnd';
import HTML5Backend from 'react-dnd-html5-backend';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import QualityProfilesConnector from './Quality/QualityProfilesConnector';
import LanguageProfilesConnector from './Language/LanguageProfilesConnector';
import DelayProfilesConnector from './Delay/DelayProfilesConnector';

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
          <QualityProfilesConnector />
          <LanguageProfilesConnector />
          <DelayProfilesConnector />
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

// Only a single DragDropContext can exist so it's done here to allow editing
// quality profiles and reordering delay profiles to work.

export default DragDropContext(HTML5Backend)(Profiles);
