import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import translate from 'Utilities/String/translate';

function SeriesMonitoringOptionsPopoverContent() {
  return (
    <DescriptionList>
      <DescriptionListItem
        title={translate('MonitorAllEpisodes')}
        data={translate('MonitorAllEpisodesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorFutureEpisodes')}
        data={translate('MonitorFutureEpisodesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorMissingEpisodes')}
        data={translate('MonitorMissingEpisodesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorExistingEpisodes')}
        data={translate('MonitorExistingEpisodesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorRecentEpisodes')}
        data={translate('MonitorRecentEpisodesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorPilotEpisode')}
        data={translate('MonitorPilotEpisodeDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorFirstSeason')}
        data={translate('MonitorFirstSeasonDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorLastSeason')}
        data={translate('MonitorLastSeasonDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorSpecials')}
        data={translate('MonitorSpecialsDescription')}
      />

      <DescriptionListItem
        title={translate('UnmonitorSpecials')}
        data={translate('UnmonitorSpecialsDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorNone')}
        data={translate('MonitorNoneDescription')}
      />
    </DescriptionList>
  );
}

export default SeriesMonitoringOptionsPopoverContent;
