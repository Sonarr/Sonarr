import React from 'react';
import MenuContent from 'Components/Menu/MenuContent';
import SortMenu from 'Components/Menu/SortMenu';
import SortMenuItem from 'Components/Menu/SortMenuItem';
import { align } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import translate from 'Utilities/String/translate';

interface SeriesIndexSortMenuProps {
  sortKey?: string;
  sortDirection?: SortDirection;
  isDisabled: boolean;
  onSortSelect(sortKey: string): void;
}

function SeriesIndexSortMenu(props: SeriesIndexSortMenuProps) {
  const { sortKey, sortDirection, isDisabled, onSortSelect } = props;

  return (
    <SortMenu isDisabled={isDisabled} alignMenu={align.RIGHT}>
      <MenuContent>
        <SortMenuItem
          name="status"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('MonitoredStatus')}
        </SortMenuItem>

        <SortMenuItem
          name="sortTitle"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('Title')}
        </SortMenuItem>

        <SortMenuItem
          name="network"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('Network')}
        </SortMenuItem>

        <SortMenuItem
          name="originalLanguage"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('OriginalLanguage')}
        </SortMenuItem>

        <SortMenuItem
          name="qualityProfileId"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('QualityProfile')}
        </SortMenuItem>

        <SortMenuItem
          name="nextAiring"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('NextAiring')}
        </SortMenuItem>

        <SortMenuItem
          name="previousAiring"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('PreviousAiring')}
        </SortMenuItem>

        <SortMenuItem
          name="added"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('Added')}
        </SortMenuItem>

        <SortMenuItem
          name="seasonCount"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('Seasons')}
        </SortMenuItem>

        <SortMenuItem
          name="episodeProgress"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('Episodes')}
        </SortMenuItem>

        <SortMenuItem
          name="episodeCount"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('EpisodeCount')}
        </SortMenuItem>

        <SortMenuItem
          name="latestSeason"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('LatestSeason')}
        </SortMenuItem>

        <SortMenuItem
          name="path"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('Path')}
        </SortMenuItem>

        <SortMenuItem
          name="sizeOnDisk"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('SizeOnDisk')}
        </SortMenuItem>

        <SortMenuItem
          name="tags"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('Tags')}
        </SortMenuItem>

        <SortMenuItem
          name="ratings"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('Rating')}
        </SortMenuItem>
      </MenuContent>
    </SortMenu>
  );
}

export default SeriesIndexSortMenu;
