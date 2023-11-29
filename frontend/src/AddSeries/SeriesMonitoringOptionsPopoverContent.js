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
        title={translate('MonitorSpecialEpisodes')}
        data={translate('MonitorSpecialEpisodesDescription')}
      />

      <DescriptionListItem
        title={translate('UnmonitorSpecialEpisodes')}
        data={translate('UnmonitorSpecialsEpisodesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorNoEpisodes')}
        data={translate('MonitorNoEpisodesDescription')}
      />
    </DescriptionList>
  );
}

export default SeriesMonitoringOptionsPopoverContent;
