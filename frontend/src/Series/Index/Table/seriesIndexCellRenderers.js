import React from 'react';
import getProgressBarKind from 'Utilities/Series/getProgressBarKind';
import ProgressBar from 'Components/ProgressBar';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import SeriesTitleLink from 'Series/SeriesTitleLink';
import SeriesIndexItemConnector from 'Series/Index/SeriesIndexItemConnector';
import SeriesIndexActionsCell from './SeriesIndexActionsCell';
import SeriesStatusCell from './SeriesStatusCell';

export default function seriesIndexCellRenderers(cellProps) {
  const {
    cellKey,
    dataKey,
    rowData,
    ...otherProps
  } = cellProps;

  const {
    id,
    monitored,
    status,
    title,
    titleSlug,
    network,
    qualityProfileId,
    nextAiring,
    previousAiring,
    seasonCount,
    episodeCount,
    episodeFileCount
  } = rowData;

  const progress = episodeCount ? episodeFileCount / episodeCount * 100 : 100;

  if (dataKey === 'status') {
    return (
      <SeriesStatusCell
        key={cellKey}
        monitored={monitored}
        status={status}
        component={VirtualTableRowCell}
        {...otherProps}
      />
    );
  }

  if (dataKey === 'sortTitle') {
    return (
      <VirtualTableRowCell
        key={cellKey}
        {...otherProps}
      >
        <SeriesTitleLink
          titleSlug={titleSlug}
          title={title}
        />
      </VirtualTableRowCell>

    );
  }

  if (dataKey === 'network') {
    return (
      <VirtualTableRowCell
        key={cellKey}
        {...otherProps}
      >
        {network}
      </VirtualTableRowCell>

    );
  }

  if (dataKey === 'qualityProfileId') {
    return (
      <VirtualTableRowCell
        key={cellKey}
        {...otherProps}
      >
        <QualityProfileNameConnector
          qualityProfileId={qualityProfileId}
        />
      </VirtualTableRowCell>
    );
  }

  if (dataKey === 'nextAiring') {
    return (
      <RelativeDateCellConnector
        key={cellKey}
        date={nextAiring}
        component={VirtualTableRowCell}
        {...otherProps}
      />
    );
  }

  if (dataKey === 'previousAiring') {
    return (
      <RelativeDateCellConnector
        key={cellKey}
        date={previousAiring}
        component={VirtualTableRowCell}
        {...otherProps}
      />
    );
  }

  if (dataKey === 'seasonCount') {
    return (
      <VirtualTableRowCell
        key={cellKey}
        {...otherProps}
      >
        {seasonCount}
      </VirtualTableRowCell>
    );
  }

  if (dataKey === 'episodeProgress') {
    return (
      <VirtualTableRowCell
        key={cellKey}
        {...otherProps}
      >
        <ProgressBar
          progress={progress}
          kind={getProgressBarKind(status, monitored, progress)}
          showText={true}
          text={`${episodeFileCount} / ${episodeCount}`}
          width={125}
        />
      </VirtualTableRowCell>
    );
  }

  if (dataKey === 'actions') {
    return (
      <SeriesIndexItemConnector
        key={cellKey}
        component={SeriesIndexActionsCell}
        id={id}
        {...otherProps}
      />
    );
  }
}
