import React from 'react';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

interface SeriesIndexActionsProps {
  title: string;
  className?: string;
  tabIndex?: number;
  showSearchAction: boolean;
  isRefreshingSeries: boolean;
  isSearchingSeries: boolean;
  onRefreshPress: () => void;
  onSearchPress: () => void;
  onEditSeriesPress: () => void;
}

function SeriesIndexActions({
  title,
  className,
  tabIndex,
  showSearchAction,
  isRefreshingSeries,
  isSearchingSeries,
  onRefreshPress,
  onSearchPress,
  onEditSeriesPress,
}: SeriesIndexActionsProps) {
  const refreshLabel = translate('RefreshSeriesWithTitle', { title });
  const searchLabel = translate('SearchForMonitoredEpisodesWithTitle', {
    title,
  });
  const editLabel = translate('EditSeriesWithTitle', { title });

  return (
    <>
      <SpinnerIconButton
        className={className}
        name={icons.REFRESH}
        title={refreshLabel}
        aria-label={refreshLabel}
        isSpinning={isRefreshingSeries}
        tabIndex={tabIndex}
        onPress={onRefreshPress}
      />

      {showSearchAction ? (
        <SpinnerIconButton
          className={className}
          name={icons.SEARCH}
          title={searchLabel}
          aria-label={searchLabel}
          isSpinning={isSearchingSeries}
          tabIndex={tabIndex}
          onPress={onSearchPress}
        />
      ) : null}

      <IconButton
        className={className}
        name={icons.EDIT}
        title={editLabel}
        aria-label={editLabel}
        tabIndex={tabIndex}
        onPress={onEditSeriesPress}
      />
    </>
  );
}

export default SeriesIndexActions;
