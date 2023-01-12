import React, { useCallback, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import TextTruncate from 'react-text-truncate';
import { REFRESH_SERIES, SERIES_SEARCH } from 'Commands/commandNames';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import { icons } from 'Helpers/Props';
import DeleteSeriesModal from 'Series/Delete/DeleteSeriesModal';
import EditSeriesModalConnector from 'Series/Edit/EditSeriesModalConnector';
import SeriesIndexProgressBar from 'Series/Index/ProgressBar/SeriesIndexProgressBar';
import SeriesIndexPosterSelect from 'Series/Index/Select/SeriesIndexPosterSelect';
import SeriesPoster from 'Series/SeriesPoster';
import { executeCommand } from 'Store/Actions/commandActions';
import dimensions from 'Styles/Variables/dimensions';
import fonts from 'Styles/Variables/fonts';
import createSeriesIndexItemSelector from '../createSeriesIndexItemSelector';
import selectOverviewOptions from './selectOverviewOptions';
import SeriesIndexOverviewInfo from './SeriesIndexOverviewInfo';
import styles from './SeriesIndexOverview.css';

const columnPadding = parseInt(dimensions.seriesIndexColumnPadding);
const columnPaddingSmallScreen = parseInt(
  dimensions.seriesIndexColumnPaddingSmallScreen
);
const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

// Hardcoded height beased on line-height of 32 + bottom margin of 10.
// Less side-effecty than using react-measure.
const TITLE_HEIGHT = 42;

interface SeriesIndexOverviewProps {
  seriesId: number;
  sortKey: string;
  posterWidth: number;
  posterHeight: number;
  rowHeight: number;
  isSelectMode: boolean;
  isSmallScreen: boolean;
}

function SeriesIndexOverview(props: SeriesIndexOverviewProps) {
  const {
    seriesId,
    sortKey,
    posterWidth,
    posterHeight,
    rowHeight,
    isSelectMode,
    isSmallScreen,
  } = props;

  const { series, qualityProfile, isRefreshingSeries, isSearchingSeries } =
    useSelector(createSeriesIndexItemSelector(props.seriesId));

  const overviewOptions = useSelector(selectOverviewOptions);

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
    statistics = {},
    images,
    network,
  } = series;

  const {
    seasonCount = 0,
    episodeCount = 0,
    episodeFileCount = 0,
    totalEpisodeCount = 0,
    sizeOnDisk = 0,
  } = statistics;

  const dispatch = useDispatch();
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

  const contentHeight = useMemo(() => {
    const padding = isSmallScreen ? columnPaddingSmallScreen : columnPadding;

    return rowHeight - padding;
  }, [rowHeight, isSmallScreen]);

  const overviewHeight = contentHeight - TITLE_HEIGHT;

  return (
    <div>
      <div className={styles.content}>
        <div className={styles.poster}>
          <div className={styles.posterContainer}>
            {isSelectMode ? (
              <SeriesIndexPosterSelect seriesId={seriesId} />
            ) : null}

            {status === 'ended' && (
              <div className={styles.ended} title="Ended" />
            )}

            <Link className={styles.link} style={elementStyle} to={link}>
              <SeriesPoster
                className={styles.poster}
                style={elementStyle}
                images={images}
                size={250}
                lazy={false}
                overflow={true}
              />
            </Link>
          </div>

          <SeriesIndexProgressBar
            monitored={monitored}
            status={status}
            episodeCount={episodeCount}
            episodeFileCount={episodeFileCount}
            totalEpisodeCount={totalEpisodeCount}
            posterWidth={posterWidth}
            detailedProgressBar={overviewOptions.detailedProgressBar}
          />
        </div>

        <div className={styles.info} style={{ maxHeight: contentHeight }}>
          <div className={styles.titleRow}>
            <Link className={styles.title} to={link}>
              {title}
            </Link>

            <div className={styles.actions}>
              <SpinnerIconButton
                name={icons.REFRESH}
                title="Refresh series"
                isSpinning={isRefreshingSeries}
                onPress={onRefreshPress}
              />

              {overviewOptions.showSearchAction ? (
                <SpinnerIconButton
                  name={icons.SEARCH}
                  title="Search for monitored episodes"
                  isSpinning={isSearchingSeries}
                  onPress={onSearchPress}
                />
              ) : null}

              <IconButton
                name={icons.EDIT}
                title="Edit Series"
                onPress={onEditSeriesPress}
              />
            </div>
          </div>

          <div className={styles.details}>
            <Link className={styles.overview} to={link}>
              <TextTruncate
                line={Math.floor(
                  overviewHeight / (defaultFontSize * lineHeight)
                )}
                text={overview}
              />
            </Link>

            <SeriesIndexOverviewInfo
              height={overviewHeight}
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
              {...overviewOptions}
            />
          </div>
        </div>
      </div>

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

export default SeriesIndexOverview;
