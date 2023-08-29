import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useSelect } from 'App/SelectContext';
import { REFRESH_SERIES, SERIES_SEARCH } from 'Commands/commandNames';
import CheckInput from 'Components/Form/CheckInput';
import HeartRating from 'Components/HeartRating';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import Column from 'Components/Table/Column';
import TagListConnector from 'Components/TagListConnector';
import { icons } from 'Helpers/Props';
import DeleteSeriesModal from 'Series/Delete/DeleteSeriesModal';
import EditSeriesModalConnector from 'Series/Edit/EditSeriesModalConnector';
import createSeriesIndexItemSelector from 'Series/Index/createSeriesIndexItemSelector';
import { Statistics } from 'Series/Series';
import SeriesBanner from 'Series/SeriesBanner';
import SeriesTitleLink from 'Series/SeriesTitleLink';
import { executeCommand } from 'Store/Actions/commandActions';
import { SelectStateInputProps } from 'typings/props';
import formatBytes from 'Utilities/Number/formatBytes';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';
import SeriesIndexProgressBar from '../ProgressBar/SeriesIndexProgressBar';
import hasGrowableColumns from './hasGrowableColumns';
import SeasonsCell from './SeasonsCell';
import selectTableOptions from './selectTableOptions';
import SeriesStatusCell from './SeriesStatusCell';
import styles from './SeriesIndexRow.css';

interface SeriesIndexRowProps {
  seriesId: number;
  sortKey: string;
  columns: Column[];
  isSelectMode: boolean;
}

function SeriesIndexRow(props: SeriesIndexRowProps) {
  const { seriesId, columns, isSelectMode } = props;

  const {
    series,
    qualityProfile,
    latestSeason,
    isRefreshingSeries,
    isSearchingSeries,
  } = useSelector(createSeriesIndexItemSelector(props.seriesId));

  const { showBanners, showSearchAction } = useSelector(selectTableOptions);

  const {
    title,
    monitored,
    status,
    path,
    titleSlug,
    nextAiring,
    previousAiring,
    added,
    statistics = {} as Statistics,
    seasonFolder,
    images,
    seriesType,
    network,
    originalLanguage,
    certification,
    year,
    useSceneNumbering,
    genres = [],
    ratings,
    seasons = [],
    tags = [],
    isSaving = false,
  } = series;

  const {
    seasonCount = 0,
    episodeCount = 0,
    episodeFileCount = 0,
    totalEpisodeCount = 0,
    sizeOnDisk = 0,
    releaseGroups = [],
  } = statistics;

  const dispatch = useDispatch();
  const [hasBannerError, setHasBannerError] = useState(false);
  const [isEditSeriesModalOpen, setIsEditSeriesModalOpen] = useState(false);
  const [isDeleteSeriesModalOpen, setIsDeleteSeriesModalOpen] = useState(false);
  const [selectState, selectDispatch] = useSelect();

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

  const onBannerLoadError = useCallback(() => {
    setHasBannerError(true);
  }, [setHasBannerError]);

  const onBannerLoad = useCallback(() => {
    setHasBannerError(false);
  }, [setHasBannerError]);

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

  const checkInputCallback = useCallback(() => {
    // Mock handler to satisfy `onChange` being required for `CheckInput`.
  }, []);

  const onSelectedChange = useCallback(
    ({ id, value, shiftKey }: SelectStateInputProps) => {
      selectDispatch({
        type: 'toggleSelected',
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [selectDispatch]
  );

  return (
    <>
      {isSelectMode ? (
        <VirtualTableSelectCell
          id={seriesId}
          isSelected={selectState.selectedState[seriesId]}
          isDisabled={false}
          onSelectedChange={onSelectedChange}
        />
      ) : null}

      {columns.map((column) => {
        const { name, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'status') {
          return (
            <SeriesStatusCell
              key={name}
              className={styles[name]}
              seriesId={seriesId}
              monitored={monitored}
              status={status}
              isSelectMode={isSelectMode}
              isSaving={isSaving}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'sortTitle') {
          return (
            <VirtualTableRowCell
              key={name}
              className={classNames(
                styles[name],
                showBanners && styles.banner,
                showBanners && !hasGrowableColumns(columns) && styles.bannerGrow
              )}
            >
              {showBanners ? (
                <Link className={styles.link} to={`/series/${titleSlug}`}>
                  <SeriesBanner
                    className={styles.bannerImage}
                    images={images}
                    lazy={false}
                    overflow={true}
                    onError={onBannerLoadError}
                    onLoad={onBannerLoad}
                  />

                  {hasBannerError && (
                    <div className={styles.overlayTitle}>{title}</div>
                  )}
                </Link>
              ) : (
                <SeriesTitleLink titleSlug={titleSlug} title={title} />
              )}
            </VirtualTableRowCell>
          );
        }

        if (name === 'seriesType') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {titleCase(seriesType)}
            </VirtualTableRowCell>
          );
        }

        if (name === 'network') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {network}
            </VirtualTableRowCell>
          );
        }

        if (name === 'originalLanguage') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {originalLanguage.name}
            </VirtualTableRowCell>
          );
        }

        if (name === 'qualityProfileId') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {qualityProfile?.name ?? ''}
            </VirtualTableRowCell>
          );
        }

        if (name === 'nextAiring') {
          return (
            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore ts(2739)
            <RelativeDateCellConnector
              key={name}
              className={styles[name]}
              date={nextAiring}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'previousAiring') {
          return (
            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore ts(2739)
            <RelativeDateCellConnector
              key={name}
              className={styles[name]}
              date={previousAiring}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'added') {
          return (
            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore ts(2739)
            <RelativeDateCellConnector
              key={name}
              className={styles[name]}
              date={added}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'seasonCount') {
          return (
            <SeasonsCell
              key={name}
              className={styles[name]}
              seriesId={seriesId}
              seasonCount={seasonCount}
              seasons={seasons}
              isSelectMode={isSelectMode}
            />
          );
        }

        if (name === 'seasonFolder') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <CheckInput
                name="seasonFolder"
                value={seasonFolder}
                isDisabled={true}
                onChange={checkInputCallback}
              />
            </VirtualTableRowCell>
          );
        }

        if (name === 'episodeProgress') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <SeriesIndexProgressBar
                seriesId={seriesId}
                monitored={monitored}
                status={status}
                episodeCount={episodeCount}
                episodeFileCount={episodeFileCount}
                totalEpisodeCount={totalEpisodeCount}
                width={125}
                detailedProgressBar={true}
                isStandalone={true}
              />
            </VirtualTableRowCell>
          );
        }

        if (name === 'latestSeason') {
          if (!latestSeason) {
            return <VirtualTableRowCell key={name} className={styles[name]} />;
          }

          const seasonStatistics = latestSeason.statistics || {};

          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <SeriesIndexProgressBar
                seriesId={seriesId}
                seasonNumber={latestSeason.seasonNumber}
                monitored={monitored}
                status={status}
                episodeCount={seasonStatistics.episodeCount}
                episodeFileCount={seasonStatistics.episodeFileCount}
                totalEpisodeCount={seasonStatistics.totalEpisodeCount}
                width={125}
                detailedProgressBar={true}
                isStandalone={true}
              />
            </VirtualTableRowCell>
          );
        }

        if (name === 'episodeCount') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {totalEpisodeCount}
            </VirtualTableRowCell>
          );
        }

        if (name === 'year') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {year}
            </VirtualTableRowCell>
          );
        }

        if (name === 'path') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {path}
            </VirtualTableRowCell>
          );
        }

        if (name === 'sizeOnDisk') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {formatBytes(sizeOnDisk)}
            </VirtualTableRowCell>
          );
        }

        if (name === 'genres') {
          const joinedGenres = genres.join(', ');

          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <span title={joinedGenres}>{joinedGenres}</span>
            </VirtualTableRowCell>
          );
        }

        if (name === 'ratings') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <HeartRating rating={ratings.value} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'certification') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {certification}
            </VirtualTableRowCell>
          );
        }

        if (name === 'releaseGroups') {
          const joinedReleaseGroups = releaseGroups.join(', ');
          const truncatedReleaseGroups =
            releaseGroups.length > 3
              ? `${releaseGroups.slice(0, 3).join(', ')}...`
              : joinedReleaseGroups;

          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <span title={joinedReleaseGroups}>{truncatedReleaseGroups}</span>
            </VirtualTableRowCell>
          );
        }

        if (name === 'tags') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <TagListConnector tags={tags} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'useSceneNumbering') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <CheckInput
                className={styles.checkInput}
                name="useSceneNumbering"
                value={useSceneNumbering}
                isDisabled={true}
                onChange={checkInputCallback}
              />
            </VirtualTableRowCell>
          );
        }

        if (name === 'actions') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <SpinnerIconButton
                name={icons.REFRESH}
                title={translate('RefreshSeries')}
                isSpinning={isRefreshingSeries}
                onPress={onRefreshPress}
              />

              {showSearchAction ? (
                <SpinnerIconButton
                  name={icons.SEARCH}
                  title={translate('SearchForMonitoredEpisodes')}
                  isSpinning={isSearchingSeries}
                  onPress={onSearchPress}
                />
              ) : null}

              <IconButton
                name={icons.EDIT}
                title={translate('EditSeries')}
                onPress={onEditSeriesPress}
              />
            </VirtualTableRowCell>
          );
        }

        return null;
      })}

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
    </>
  );
}

export default SeriesIndexRow;
