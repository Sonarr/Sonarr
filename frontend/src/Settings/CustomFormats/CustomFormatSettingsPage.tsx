import React from 'react';
import { DndProvider } from 'react-dnd';
import { HTML5Backend } from 'react-dnd-html5-backend';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import ParseToolbarButton from 'Parse/ParseToolbarButton';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import translate from 'Utilities/String/translate';
import CustomFormatsConnector from './CustomFormats/CustomFormatsConnector';
import ManageCustomFormatsToolbarButton from './CustomFormats/Manage/ManageCustomFormatsToolbarButton';

function CustomFormatSettingsPage() {
  return (
    <PageContent title={translate('CustomFormatsSettings')}>
      <SettingsToolbarConnector
        // eslint-disable-next-line @typescript-eslint/ban-ts-comment
        // @ts-ignore
        showSave={false}
        additionalButtons={
          <>
            <PageToolbarSeparator />

            <ParseToolbarButton />

            <ManageCustomFormatsToolbarButton />
          </>
        }
      />

      <PageContentBody>
        {/* TODO: Upgrade react-dnd to get typings, we're 2 major versions behind */}
        {/* eslint-disable-next-line @typescript-eslint/ban-ts-comment */}
        {/* @ts-ignore */}
        <DndProvider backend={HTML5Backend}>
          {/* eslint-disable-next-line @typescript-eslint/ban-ts-comment */}
          {/* @ts-ignore */}
          <CustomFormatsConnector />
        </DndProvider>
      </PageContentBody>
    </PageContent>
  );
}

export default CustomFormatSettingsPage;
