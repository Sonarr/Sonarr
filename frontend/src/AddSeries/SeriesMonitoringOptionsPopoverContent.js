import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';

function SeriesMonitoringOptionsPopoverContent() {
  return (
    <DescriptionList>
      <DescriptionListItem
        title="All Episodes"
        data="Monitor all episodes except specials"
      />

      <DescriptionListItem
        title="Future Episodes"
        data="Monitor episodes that have not aired yet"
      />

      <DescriptionListItem
        title="Missing Episodes"
        data="Monitor episodes that do not have files or have not aired yet"
      />

      <DescriptionListItem
        title="Existing Episodes"
        data="Monitor episodes that have files or have not aired yet"
      />

      <DescriptionListItem
        title="First Season"
        data="Monitor all episodes of the first season. All other seasons will be ignored"
      />

      <DescriptionListItem
        title="Latest Season"
        data="Monitor all episodes of the latest season and future seasons"
      />

      <DescriptionListItem
        title="None"
        data="No episodes will be monitored."
      />
    </DescriptionList>
  );
}

export default SeriesMonitoringOptionsPopoverContent;
