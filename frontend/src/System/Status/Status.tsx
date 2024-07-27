import React, { Component } from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import translate from 'Utilities/String/translate';
import About from './About/About';
import DiskSpace from './DiskSpace/DiskSpace';
import Health from './Health/Health';
import MoreInfo from './MoreInfo/MoreInfo';

class Status extends Component {
  //
  // Render

  render() {
    return (
      <PageContent title={translate('Status')}>
        <PageContentBody>
          <Health />
          <DiskSpace />
          <About />
          <MoreInfo />
        </PageContentBody>
      </PageContent>
    );
  }
}

export default Status;
