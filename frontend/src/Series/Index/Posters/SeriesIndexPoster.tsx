import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { REFRESH_SERIES, SERIES_SEARCH } from 'Commands/commandNames';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import { icons } from 'Helpers/Props';
import DeleteSeriesModal from 'Series/Delete/DeleteSeriesModal';
import EditSeriesModalConnector from 'Series/Edit/EditSeriesModalConnector';
import SeriesIndexProgressBar from 'Series/Index/ProgressBar/SeriesIndexProgressBar';
import SeriesPoster from 'Series/SeriesPoster';
import { executeCommand } from 'Store/Actions/commandActions';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import createSeriesIndexItemSelector from '../createSeriesIndexItemSelector';
import selectPosterOptions from './selectPosterOptions';
import SeriesIndexPosterInfo from './SeriesIndexPosterInfo';
import styles from './SeriesIndexPoster.css';

interface SeriesIndexPosterProps {
  seriesId: number;
  sortKey: string;
  posterWidth: number;
  posterHeight: number;
}

function SeriesIndexPoster(props: SeriesIndexPosterProps) {
  const { seriesId, sortKey, posterWidth, posterHeight } = props;

  const { series, qualityProfile, isRefreshingSeries, isSearchingSeries } =
    useSelector(createSeriesIndexItemSelector(props.seriesId));

  const {
    detailedProgressBar,
    showTitle,
    showMonitored,
    showQualityProfile,
    showSearchAction,
  } = useSelector(selectPosterOptions);

  const { showRelativeDates, shortDateFormat, timeFormat } = useSelector(
    createUISettingsSelector()
  );

  const {
    title,
    monitored,
    status,
    path,
    titleSlug,
    nextAiring,
    statistics,
    images,
  } = series;

  const {
    seasonCount,
    episodeCount,
    episodeFileCount,
    totalEpisodeCount,
    sizeOnDisk,
  } = statistics;

  const dispatch = useDispatch();
  const [hasPosterError, setHasPosterError] = useState(false);
  const [isEditSeriesModalOpen, setIsEditSeriesModalOpen] = useState(false);
  const [isDeleteSeriesModalOpen, setIsDeleteSeriesModalOpen] = useState(false);

  const onRefreshPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: REFRESH_SERIES,
        seriesId,
      })
    );
  }, [seriesId, dispatch]);

  const onSearchPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: SERIES_SEARCH,
        seriesId,
      })
    );
  }, [seriesId, dispatch]);

  const onPosterLoadError = useCallback(() => {
    setHasPosterError(true);
  }, [setHasPosterError]);

  const onPosterLoad = useCallback(() => {
    setHasPosterError(false);
  }, [setHasPosterError]);

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

  const link = `/series/${titleSlug}`;

  const elementStyle = {
    width: `${posterWidth}px`,
    height: `${posterHeight}px`,
  };

  return (
    <div className={styles.content}>
      <div className={styles.posterContainer}>
        <Label className={styles.controls}>
          <SpinnerIconButton
            className={styles.action}
            name={icons.REFRESH}
            title="Refresh series"
            isSpinning={isRefreshingSeries}
            onPress={onRefreshPress}
          />

          {showSearchAction ? (
            <SpinnerIconButton
              className={styles.action}
              name={icons.SEARCH}
              title="Search for monitored episodes"
              isSpinning={isSearchingSeries}
              onPress={onSearchPress}
            />
          ) : null}

          <IconButton
            className={styles.action}
            name={icons.EDIT}
            title="Edit Series"
            onPress={onEditSeriesPress}
          />
        </Label>

        {status === 'ended' ? (
          <div className={styles.ended} title="Ended" />
        ) : null}

        <Link className={styles.link} style={elementStyle} to={link}>
          <SeriesPoster
            style={elementStyle}
            images={images}
            size={250}
            lazy={false}
            overflow={true}
            onError={onPosterLoadError}
            onLoad={onPosterLoad}
          />

          {hasPosterError ? (
            <div className={styles.overlayTitle}>{title}</div>
          ) : null}
        </Link>
      </div>

      <SeriesIndexProgressBar
        monitored={monitored}
        status={status}
        episodeCount={episodeCount}
        episodeFileCount={episodeFileCount}
        totalEpisodeCount={totalEpisodeCount}
        posterWidth={posterWidth}
        detailedProgressBar={detailedProgressBar}
      />

      {showTitle ? <div className={styles.title}>{title}</div> : null}

      {showMonitored ? (
        <div className={styles.title}>
          {monitored ? 'Monitored' : 'Unmonitored'}
        </div>
      ) : null}

      {showQualityProfile ? (
        <div className={styles.title}>{qualityProfile.name}</div>
      ) : null}

      {nextAiring ? (
        <div className={styles.nextAiring}>
          {getRelativeDate(nextAiring, shortDateFormat, showRelativeDates, {
            timeFormat,
            timeForToday: true,
          })}
        </div>
      ) : null}

      <SeriesIndexPosterInfo
        seasonCount={seasonCount}
        sizeOnDisk={sizeOnDisk}
        path={path}
        qualityProfile={qualityProfile}
        showQualityProfile={showQualityProfile}
        showRelativeDates={showRelativeDates}
        sortKey={sortKey}
        shortDateFormat={shortDateFormat}
        timeFormat={timeFormat}
      />

      <EditSeriesModalConnector
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

export default SeriesIndexPoster;
