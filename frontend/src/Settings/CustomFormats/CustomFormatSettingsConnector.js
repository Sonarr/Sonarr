import React, { Component } from 'react';
import { DndProvider } from 'react-dnd';
import { HTML5Backend } from 'react-dnd-html5-backend';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import CustomFormatsConnector from './CustomFormats/CustomFormatsConnector';

class CustomFormatSettingsConnector extends Component {

  //
  // Render

  render() {
    return (
      <PageContent title="Custom Format Settings">
        <SettingsToolbarConnector
          showSave={false}
        />

        <PageContentBody>
          <DndProvider backend={HTML5Backend}>
            <CustomFormatsConnector />
          </DndProvider>
        </PageContentBody>
      </PageContent>
    );
  }
}

export default CustomFormatSettingsConnector;

