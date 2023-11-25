import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import translate from 'Utilities/String/translate';

function SeriesMonitorNewItemsOptionsPopoverContent() {
  return (
    <DescriptionList>
      <DescriptionListItem
        title={translate('MonitorAllSeasons')}
        data={translate('MonitorAllSeasonsDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorNoNewSeasons')}
        data={translate('MonitorNoNewSeasonsDescription')}
      />
    </DescriptionList>
  );
}

export default SeriesMonitorNewItemsOptionsPopoverContent;
