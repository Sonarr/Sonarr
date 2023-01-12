import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { SelectActionType, useSelect } from 'App/SelectContext';
import { REFRESH_SERIES, SERIES_SEARCH } from 'Commands/commandNames';
import CheckInput from 'Components/Form/CheckInput';
import HeartRating from 'Components/HeartRating';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import ProgressBar from 'Components/ProgressBar';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import Column from 'Components/Table/Column';
import TagListConnector from 'Components/TagListConnector';
import { icons } from 'Helpers/Props';
import DeleteSeriesModal from 'Series/Delete/DeleteSeriesModal';
import EditSeriesModalConnector from 'Series/Edit/EditSeriesModalConnector';
import createSeriesIndexItemSelector from 'Series/Index/createSeriesIndexItemSelector';
import SeriesBanner from 'Series/SeriesBanner';
import SeriesTitleLink from 'Series/SeriesTitleLink';
import { executeCommand } from 'Store/Actions/commandActions';
import formatBytes from 'Utilities/Number/formatBytes';
import getProgressBarKind from 'Utilities/Series/getProgressBarKind';
import titleCase from 'Utilities/String/titleCase';
import hasGrowableColumns from './hasGrowableColumns';
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
    statistics = {},
    images,
    seriesType,
    network,
    originalLanguage,
    certification,
    year,
    useSceneNumbering,
    genres = [],
    ratings,
    tags = [],
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

  const onUseSceneNumberingChange = useCallback(() => {
    // Mock handler to satisfy `onChange` being required for `CheckInput`.
  }, []);

  const onSelectedChange = useCallback(
    ({ id, value, shiftKey }) => {
      selectDispatch({
        type: SelectActionType.ToggleSelected,
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
              monitored={monitored}
              status={status}
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
              {qualityProfile.name}
            </VirtualTableRowCell>
          );
        }

        if (name === 'nextAiring') {
          return (
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
            <VirtualTableRowCell key={name} className={styles[name]}>
              {seasonCount}
            </VirtualTableRowCell>
          );
        }

        if (name === 'episodeProgress') {
          const progress = episodeCount
            ? (episodeFileCount / episodeCount) * 100
            : 100;

          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <ProgressBar
                progress={progress}
                kind={getProgressBarKind(status, monitored, progress)}
                showText={true}
                text={`${episodeFileCount} / ${episodeCount}`}
                title={`${episodeFileCount} / ${episodeCount} (Total: ${totalEpisodeCount})`}
                width={125}
              />
            </VirtualTableRowCell>
          );
        }

        if (name === 'latestSeason') {
          if (!latestSeason) {
            return <VirtualTableRowCell key={name} className={styles[name]} />;
          }

          const seasonStatistics = latestSeason.statistics || {};
          const progress = seasonStatistics.episodeCount
            ? (seasonStatistics.episodeFileCount /
                seasonStatistics.episodeCount) *
              100
            : 100;

          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <ProgressBar
                progress={progress}
                kind={getProgressBarKind(status, monitored, progress)}
                showText={true}
                text={`${seasonStatistics.episodeFileCount} / ${seasonStatistics.episodeCount}`}
                title={`${seasonStatistics.episodeFileCount} / ${seasonStatistics.episodeCount} (Total: ${seasonStatistics.totalEpisodeCount})`}
                width={125}
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
                onChange={onUseSceneNumberingChange}
              />
            </VirtualTableRowCell>
          );
        }

        if (name === 'actions') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <SpinnerIconButton
                name={icons.REFRESH}
                title="Refresh series"
                isSpinning={isRefreshingSeries}
                onPress={onRefreshPress}
              />

              {showSearchAction ? (
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
