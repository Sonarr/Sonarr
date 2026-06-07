import classNames from 'classnames';
import React, { useCallback, useMemo, useState } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useExecuteCommand } from 'Commands/useCommands';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import useMeasure from 'Helpers/Hooks/useMeasure';
import { icons } from 'Helpers/Props';
import DeleteSeriesModal from 'Series/Delete/DeleteSeriesModal';
import EditSeriesModal from 'Series/Edit/EditSeriesModal';
import SeriesIndexProgressBar from 'Series/Index/ProgressBar/SeriesIndexProgressBar';
import SeriesIndexPosterSelect from 'Series/Index/Select/SeriesIndexPosterSelect';
import { SeriesStatus, Statistics } from 'Series/Series';
import { useSeriesOverviewOptions } from 'Series/seriesOptionsStore';
import SeriesPoster from 'Series/SeriesPoster';
import translate from 'Utilities/String/translate';
import useSeriesIndexItem from '../useSeriesIndexItem';
import { ROW_BORDER, ROW_VERTICAL_PADDING } from './overviewLayout';
import SeriesIndexOverviewInfo from './SeriesIndexOverviewInfo';
import styles from './SeriesIndexOverview.css';

// Mirrors .synopsis font-size/line-height in SeriesIndexOverview.css.
const SYNOPSIS_FONT_SIZE = 13.5;
const SYNOPSIS_LINE_HEIGHT = 1.6;

const TITLE_HEIGHT = 38;
const CHIP_STRIP_HEIGHT = 28;

interface SeriesIndexOverviewProps {
  seriesId: number;
  sortKey: string;
  posterWidth: number;
  posterHeight: number;
  rowHeight: number;
  isSelectMode: boolean;
}

function getStatusBadge(status: SeriesStatus) {
  switch (status) {
    case 'deleted':
      return { label: translate('Deleted'), statusClass: styles.deleted };
    case 'continuing':
      return { label: translate('Continuing'), statusClass: styles.continuing };
    case 'upcoming':
      return { label: translate('Upcoming'), statusClass: styles.upcoming };
    case 'ended':
      return { label: translate('Ended'), statusClass: styles.ended };
    default:
      return null;
  }
}

function SeriesIndexOverview(props: SeriesIndexOverviewProps) {
  const {
    seriesId,
    sortKey,
    posterWidth,
    posterHeight,
    rowHeight,
    isSelectMode,
  } = props;

  const { series, qualityProfile, isRefreshingSeries, isSearchingSeries } =
    useSeriesIndexItem(seriesId);

  const overviewOptions = useSeriesOverviewOptions();

  const executeCommand = useExecuteCommand();
  const [isEditSeriesModalOpen, setIsEditSeriesModalOpen] = useState(false);
  const [isDeleteSeriesModalOpen, setIsDeleteSeriesModalOpen] = useState(false);

  const onRefreshPress = useCallback(() => {
    executeCommand({
      name: CommandNames.RefreshSeries,
      seriesIds: [seriesId],
    });
  }, [seriesId, executeCommand]);

  const onSearchPress = useCallback(() => {
    executeCommand({
      name: CommandNames.SeriesSearch,
      seriesId,
    });
  }, [seriesId, executeCommand]);

  const onEditSeriesPress = useCallback(() => {
    setIsEditSeriesModalOpen(true);
  }, [setIsEditSeriesModalOpen]);

  const onEditSeriesModalClose = useCallback(() => {
    setIsEditSeriesModalOpen(false);
  }, [setIsEditSeriesModalOpen]);

  const onDeleteSeriesPress = useCallback(() => {
    setIsEditSeriesModalOpen(false);
    setIsDeleteSeriesModalOpen(true);
  }, [setIsDeleteSeriesModalOpen]);

  const onDeleteSeriesModalClose = useCallback(() => {
    setIsDeleteSeriesModalOpen(false);
  }, [setIsDeleteSeriesModalOpen]);

  const contentHeight = useMemo(() => {
    return rowHeight - (ROW_VERTICAL_PADDING * 2 + ROW_BORDER);
  }, [rowHeight]);

  const [chipStripRef, chipStripBounds] = useMeasure();
  const chipStripHeight = chipStripBounds.height || CHIP_STRIP_HEIGHT;

  const synopsisLines = Math.max(
    1,
    Math.floor(
      (contentHeight - TITLE_HEIGHT - chipStripHeight) /
        (SYNOPSIS_FONT_SIZE * SYNOPSIS_LINE_HEIGHT)
    )
  );

  if (!series) {
    return null;
  }

  const {
    title,
    monitored,
    status,
    path,
    titleSlug,
    nextAiring,
    previousAiring,
    added,
    overview,
    statistics = {} as Statistics,
    images,
    tags,
    network,
  } = series;

  const {
    seasonCount = 0,
    episodeCount = 0,
    episodeFileCount = 0,
    totalEpisodeCount = 0,
    sizeOnDisk = 0,
  } = statistics;

  const link = `/series/${titleSlug}`;

  const elementStyle = {
    width: `${posterWidth}px`,
    height: `${posterHeight}px`,
  };

  const statusBadge = getStatusBadge(status);

  return (
    <div>
      <div className={styles.content}>
        <div className={styles.poster}>
          <div className={styles.posterContainer}>
            {isSelectMode ? (
              <SeriesIndexPosterSelect
                seriesId={seriesId}
                titleSlug={titleSlug}
              />
            ) : null}

            {statusBadge ? (
              <div
                className={classNames(
                  styles.statusPill,
                  statusBadge.statusClass
                )}
              >
                <span className={styles.statusLabel}>{statusBadge.label}</span>
              </div>
            ) : null}

            <Link className={styles.link} style={elementStyle} to={link}>
              <SeriesPoster
                className={styles.posterImage}
                style={elementStyle}
                images={images}
                size={250}
                lazy={false}
                overflow={true}
                title={title}
              />
            </Link>
          </div>

          <SeriesIndexProgressBar
            seriesId={seriesId}
            monitored={monitored}
            status={status}
            episodeCount={episodeCount}
            episodeFileCount={episodeFileCount}
            totalEpisodeCount={totalEpisodeCount}
            width={posterWidth}
            detailedProgressBar={overviewOptions.detailedProgressBar}
            isStandalone={false}
          />
        </div>

        <div className={styles.info} style={{ maxHeight: contentHeight }}>
          <div className={styles.titleRow}>
            <Link className={styles.title} to={link}>
              {title}
            </Link>

            <div className={styles.actions}>
              <SpinnerIconButton
                className={styles.action}
                name={icons.REFRESH}
                title={translate('RefreshSeries')}
                isSpinning={isRefreshingSeries}
                onPress={onRefreshPress}
              />

              {overviewOptions.showSearchAction ? (
                <SpinnerIconButton
                  className={styles.action}
                  name={icons.SEARCH}
                  title={translate('SearchForMonitoredEpisodes')}
                  isSpinning={isSearchingSeries}
                  onPress={onSearchPress}
                />
              ) : null}

              <IconButton
                className={styles.action}
                name={icons.EDIT}
                title={translate('EditSeries')}
                aria-label={translate('EditSeries')}
                onPress={onEditSeriesPress}
              />
            </div>
          </div>

          <Link
            className={styles.synopsis}
            to={link}
            style={{ WebkitLineClamp: synopsisLines }}
          >
            {overview}
          </Link>

          <SeriesIndexOverviewInfo
            ref={chipStripRef}
            monitored={monitored}
            network={network}
            nextAiring={nextAiring}
            previousAiring={previousAiring}
            added={added}
            seasonCount={seasonCount}
            qualityProfile={qualityProfile}
            sizeOnDisk={sizeOnDisk}
            path={path}
            sortKey={sortKey}
            showNetwork={overviewOptions.showNetwork}
            showMonitored={overviewOptions.showMonitored}
            showQualityProfile={overviewOptions.showQualityProfile}
            showPreviousAiring={overviewOptions.showPreviousAiring}
            showAdded={overviewOptions.showAdded}
            showSeasonCount={overviewOptions.showSeasonCount}
            showPath={overviewOptions.showPath}
            showSizeOnDisk={overviewOptions.showSizeOnDisk}
            showTags={overviewOptions.showTags}
            tags={tags}
          />
        </div>
      </div>

      <EditSeriesModal
        isOpen={isEditSeriesModalOpen}
        seriesId={seriesId}
        onModalClose={onEditSeriesModalClose}
        onDeleteSeriesPress={onDeleteSeriesPress}
      />

      <DeleteSeriesModal
        isOpen={isDeleteSeriesModalOpen}
        seriesId={seriesId}
        onModalClose={onDeleteSeriesModalClose}
      />
    </div>
  );
}

export default SeriesIndexOverview;
