import moment from 'moment';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useCommands, useExecuteCommand } from 'Commands/useCommands';
import Alert from 'Components/Alert';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import MetadataAttribution from 'Components/MetadataAttribution';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import useEpisodes from 'Episode/useEpisodes';
import useEpisodeFiles from 'EpisodeFile/useEpisodeFiles';
import usePrevious from 'Helpers/Hooks/usePrevious';
import {
  align,
  icons,
  kinds,
  sizes,
  sortDirections,
  tooltipPositions,
} from 'Helpers/Props';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import OrganizePreviewModal from 'Organize/OrganizePreviewModal';
import DeleteSeriesModal from 'Series/Delete/DeleteSeriesModal';
import EditSeriesModal from 'Series/Edit/EditSeriesModal';
import SeriesHistoryModal from 'Series/History/SeriesHistoryModal';
import MonitoringOptionsModal from 'Series/MonitoringOptions/MonitoringOptionsModal';
import { Image, Statistics } from 'Series/Series';
import SeriesGenres from 'Series/SeriesGenres';
import SeriesPoster from 'Series/SeriesPoster';
import { getSeriesStatusDetails } from 'Series/SeriesStatus';
import useSeries, {
  useSingleSeries,
  useToggleSeriesMonitored,
} from 'Series/useSeries';
import QualityProfileName from 'Settings/Profiles/Quality/QualityProfileName';
import sortByProp from 'Utilities/Array/sortByProp';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import formatBytes from 'Utilities/Number/formatBytes';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import filterAlternateTitles from 'Utilities/Series/filterAlternateTitles';
import translate from 'Utilities/String/translate';
import toggleSelected from 'Utilities/Table/toggleSelected';
import SeriesAlternateTitles from './SeriesAlternateTitles';
import SeriesDetailsLinks from './SeriesDetailsLinks';
import SeriesDetailsProvider from './SeriesDetailsProvider';
import SeriesDetailsSeason from './SeriesDetailsSeason';
import SeriesProgressLabel from './SeriesProgressLabel';
import SeriesTags from './SeriesTags';
import styles from './SeriesDetails.css';

function getFanartUrl(images: Image[]) {
  return images.find((image) => image.coverType === 'fanart')?.url;
}

function getDateYear(date: string | undefined) {
  const dateDate = moment.utc(date);

  return dateDate.format('YYYY');
}

interface ExpandedState {
  allExpanded: boolean;
  allCollapsed: boolean;
  seasons: Record<number, boolean>;
}

interface SeriesDetailsProps {
  seriesId: number;
}

function SeriesDetails({ seriesId }: SeriesDetailsProps) {
  const executeCommand = useExecuteCommand();

  const series = useSingleSeries(seriesId);
  const { toggleSeriesMonitored, isTogglingSeriesMonitored } =
    useToggleSeriesMonitored(seriesId);
  const { data: allSeries } = useSeries();

  const {
    isFetching: isEpisodesFetching,
    isFetched: isEpisodesFetched,
    error: episodesError,
    data,
    refetch: refetchEpisodes,
  } = useEpisodes({ seriesId });

  const { hasEpisodes, hasMonitoredEpisodes } = useMemo(() => {
    return {
      hasEpisodes: data.length > 0,
      hasMonitoredEpisodes: data.some((e) => e.monitored),
    };
  }, [data]);

  const {
    isFetching: isEpisodeFilesFetching,
    isFetched: isEpisodeFilesFetched,
    error: episodeFilesError,
    hasEpisodeFiles,
    refetch: refetchEpisodeFiles,
  } = useEpisodeFiles({ seriesId });

  const { data: commands } = useCommands();

  const { isRefreshing, isRenaming, isSearching } = useMemo(() => {
    const seriesRefreshingCommand = findCommand(commands, {
      name: CommandNames.RefreshSeries,
    });

    const isSeriesRefreshingCommandExecuting = isCommandExecuting(
      seriesRefreshingCommand
    );

    const allSeriesRefreshing =
      isSeriesRefreshingCommandExecuting &&
      seriesRefreshingCommand &&
      (!('seriesIds' in seriesRefreshingCommand.body) ||
        seriesRefreshingCommand.body.seriesIds.length === 0);

    const isSeriesRefreshing =
      isSeriesRefreshingCommandExecuting &&
      seriesRefreshingCommand &&
      'seriesIds' in seriesRefreshingCommand.body &&
      seriesRefreshingCommand.body.seriesIds.includes(seriesId);

    const isSearchingExecuting = isCommandExecuting(
      findCommand(commands, {
        name: CommandNames.SeriesSearch,
        seriesId,
      })
    );

    const isRenamingFiles = isCommandExecuting(
      findCommand(commands, {
        name: CommandNames.RenameFiles,
        seriesId,
      })
    );

    const isRenamingSeriesCommand = findCommand(commands, {
      name: CommandNames.RenameSeries,
    });

    const isRenamingSeries =
      isCommandExecuting(isRenamingSeriesCommand) &&
      isRenamingSeriesCommand &&
      'seriesIds' in isRenamingSeriesCommand.body &&
      isRenamingSeriesCommand.body.seriesIds.includes(seriesId);

    return {
      isRefreshing: isSeriesRefreshing || allSeriesRefreshing,
      isRenaming: isRenamingFiles || isRenamingSeries,
      isSearching: isSearchingExecuting,
    };
  }, [seriesId, commands]);

  const { nextSeries, previousSeries } = useMemo(() => {
    const sortedSeries = [...allSeries].sort(sortByProp('sortTitle'));
    const seriesIndex = sortedSeries.findIndex(
      (series) => series.id === seriesId
    );

    if (seriesIndex === -1) {
      return {
        nextSeries: undefined,
        previousSeries: undefined,
      };
    }

    const nextSeries = sortedSeries[seriesIndex + 1] ?? sortedSeries[0];
    const previousSeries =
      sortedSeries[seriesIndex - 1] ?? sortedSeries[sortedSeries.length - 1];

    return {
      nextSeries: {
        title: nextSeries.title,
        titleSlug: nextSeries.titleSlug,
      },
      previousSeries: {
        title: previousSeries.title,
        titleSlug: previousSeries.titleSlug,
      },
    };
  }, [seriesId, allSeries]);

  const [isOrganizeModalOpen, setIsOrganizeModalOpen] = useState(false);
  const [isManageEpisodesOpen, setIsManageEpisodesOpen] = useState(false);
  const [isEditSeriesModalOpen, setIsEditSeriesModalOpen] = useState(false);
  const [isDeleteSeriesModalOpen, setIsDeleteSeriesModalOpen] = useState(false);
  const [isSeriesHistoryModalOpen, setIsSeriesHistoryModalOpen] =
    useState(false);
  const [isMonitorOptionsModalOpen, setIsMonitorOptionsModalOpen] =
    useState(false);
  const [expandedState, setExpandedState] = useState<ExpandedState>({
    allExpanded: false,
    allCollapsed: false,
    seasons: {},
  });
  const wasRefreshing = usePrevious(isRefreshing);
  const wasRenaming = usePrevious(isRenaming);

  const alternateTitles = useMemo(() => {
    if (!series) {
      return [];
    }

    return filterAlternateTitles(
      series.alternateTitles,
      series.title,
      series.useSceneNumbering
    );
  }, [series]);

  const handleOrganizePress = useCallback(() => {
    setIsOrganizeModalOpen(true);
  }, []);

  const handleOrganizeModalClose = useCallback(() => {
    setIsOrganizeModalOpen(false);
  }, []);

  const handleManageEpisodesPress = useCallback(() => {
    setIsManageEpisodesOpen(true);
  }, []);

  const handleManageEpisodesModalClose = useCallback(() => {
    setIsManageEpisodesOpen(false);
  }, []);

  const handleEditSeriesPress = useCallback(() => {
    setIsEditSeriesModalOpen(true);
  }, []);

  const handleEditSeriesModalClose = useCallback(() => {
    setIsEditSeriesModalOpen(false);
  }, []);

  const handleDeleteSeriesPress = useCallback(() => {
    setIsEditSeriesModalOpen(false);
    setIsDeleteSeriesModalOpen(true);
  }, []);

  const handleDeleteSeriesModalClose = useCallback(() => {
    setIsDeleteSeriesModalOpen(false);
  }, []);

  const handleSeriesHistoryPress = useCallback(() => {
    setIsSeriesHistoryModalOpen(true);
  }, []);

  const handleSeriesHistoryModalClose = useCallback(() => {
    setIsSeriesHistoryModalOpen(false);
  }, []);

  const handleMonitorOptionsPress = useCallback(() => {
    setIsMonitorOptionsModalOpen(true);
  }, []);

  const handleMonitorOptionsClose = useCallback(() => {
    setIsMonitorOptionsModalOpen(false);
  }, []);

  const handleExpandAllPress = useCallback(() => {
    const expandAll = !expandedState.allExpanded;

    const newSeasons = Object.keys(expandedState.seasons).reduce<
      Record<number | string, boolean>
    >((acc, item) => {
      acc[item] = expandAll;
      return acc;
    }, {});

    setExpandedState({
      allExpanded: expandAll,
      allCollapsed: !expandAll,
      seasons: newSeasons,
    });
  }, [expandedState]);

  const handleExpandPress = useCallback(
    (seasonNumber: number, isExpanded: boolean) => {
      setExpandedState((state) => {
        const { allExpanded, allCollapsed } = state;

        const convertedState = {
          allSelected: allExpanded,
          allUnselected: allCollapsed,
          selectedState: state.seasons,
          lastToggled: null,
        };

        const newState = toggleSelected(
          convertedState,
          [],
          seasonNumber,
          isExpanded,
          false
        );

        return {
          allExpanded: newState.allSelected,
          allCollapsed: newState.allUnselected,
          seasons: newState.selectedState,
        };
      });
    },
    []
  );

  const handleMonitorTogglePress = useCallback(
    (value: boolean) => {
      toggleSeriesMonitored({
        monitored: value,
      });
    },
    [toggleSeriesMonitored]
  );

  const handleRefreshPress = useCallback(() => {
    executeCommand({
      name: CommandNames.RefreshSeries,
      seriesId,
    });
  }, [seriesId, executeCommand]);

  const handleSearchPress = useCallback(() => {
    executeCommand({
      name: CommandNames.SeriesSearch,
      seriesId,
    });
  }, [seriesId, executeCommand]);

  const populate = useCallback(() => {
    refetchEpisodes();
    refetchEpisodeFiles();
  }, [refetchEpisodes, refetchEpisodeFiles]);

  useEffect(() => {
    populate();
  }, [populate]);

  useEffect(() => {
    registerPagePopulator(populate, ['seriesUpdated']);

    return () => {
      unregisterPagePopulator(populate);
    };
  }, [populate]);

  useEffect(() => {
    if ((!isRefreshing && wasRefreshing) || (!isRenaming && wasRenaming)) {
      populate();
    }
  }, [isRefreshing, wasRefreshing, isRenaming, wasRenaming, populate]);

  if (!series) {
    return null;
  }

  const {
    tvdbId,
    tvMazeId,
    imdbId,
    tmdbId,
    title,
    runtime,
    ratings,
    path,
    statistics = {} as Statistics,
    qualityProfileId,
    monitored,
    status,
    network,
    originalLanguage,
    overview,
    images,
    seasons,
    genres,
    tags,
    year,
  } = series;

  const {
    episodeCount = 0,
    episodeFileCount = 0,
    sizeOnDisk = 0,
    lastAired,
  } = statistics;

  const statusDetails = getSeriesStatusDetails(status);
  const runningYears =
    status === 'ended' ? `${year}-${getDateYear(lastAired)}` : `${year}-`;

  let episodeFilesCountMessage = translate('SeriesDetailsNoEpisodeFiles');

  if (episodeFileCount === 1) {
    episodeFilesCountMessage = translate('SeriesDetailsOneEpisodeFile');
  } else if (episodeFileCount > 1) {
    episodeFilesCountMessage = translate('SeriesDetailsCountEpisodeFiles', {
      episodeFileCount,
    });
  }

  let expandIcon = icons.EXPAND_INDETERMINATE;

  if (expandedState.allExpanded) {
    expandIcon = icons.COLLAPSE;
  } else if (expandedState.allCollapsed) {
    expandIcon = icons.EXPAND;
  }

  const fanartUrl = getFanartUrl(images);
  const isFetching = isEpisodesFetching || isEpisodeFilesFetching;
  const isPopulated = isEpisodesFetched && isEpisodeFilesFetched;

  return (
    <SeriesDetailsProvider seriesId={seriesId}>
      <PageContent title={title}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label={translate('RefreshAndScan')}
              iconName={icons.REFRESH}
              spinningName={icons.REFRESH}
              title={translate('RefreshAndScanTooltip')}
              isSpinning={isRefreshing}
              onPress={handleRefreshPress}
            />

            <PageToolbarButton
              label={translate('SearchMonitored')}
              iconName={icons.SEARCH}
              isDisabled={!monitored || !hasMonitoredEpisodes || !hasEpisodes}
              isSpinning={isSearching}
              title={
                hasMonitoredEpisodes
                  ? undefined
                  : translate('NoMonitoredEpisodes')
              }
              onPress={handleSearchPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('PreviewRename')}
              iconName={icons.ORGANIZE}
              isDisabled={!hasEpisodeFiles}
              onPress={handleOrganizePress}
            />

            <PageToolbarButton
              label={translate('ManageEpisodes')}
              iconName={icons.EPISODE_FILE}
              onPress={handleManageEpisodesPress}
            />

            <PageToolbarButton
              label={translate('History')}
              iconName={icons.HISTORY}
              isDisabled={!hasEpisodes}
              onPress={handleSeriesHistoryPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('EpisodeMonitoring')}
              iconName={icons.MONITORED}
              onPress={handleMonitorOptionsPress}
            />

            <PageToolbarButton
              label={translate('Edit')}
              iconName={icons.EDIT}
              onPress={handleEditSeriesPress}
            />

            <PageToolbarButton
              label={translate('Delete')}
              iconName={icons.DELETE}
              onPress={handleDeleteSeriesPress}
            />
          </PageToolbarSection>

          <PageToolbarSection alignContent={align.RIGHT}>
            <PageToolbarButton
              label={
                expandedState.allExpanded
                  ? translate('CollapseAll')
                  : translate('ExpandAll')
              }
              iconName={expandIcon}
              onPress={handleExpandAllPress}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody innerClassName={styles.innerContentBody}>
          <div className={styles.header}>
            <div
              className={styles.backdrop}
              style={
                fanartUrl ? { backgroundImage: `url(${fanartUrl})` } : undefined
              }
            >
              <div className={styles.backdropOverlay} />
            </div>

            <div className={styles.headerContent}>
              <SeriesPoster
                className={styles.poster}
                images={images}
                size={500}
                lazy={false}
                title={title}
              />

              <div className={styles.info}>
                <div className={styles.titleRow}>
                  <div className={styles.titleContainer}>
                    <div className={styles.toggleMonitoredContainer}>
                      <MonitorToggleButton
                        className={styles.monitorToggleButton}
                        monitored={monitored}
                        isSaving={isTogglingSeriesMonitored}
                        size={40}
                        onPress={handleMonitorTogglePress}
                      />
                    </div>

                    <div className={styles.title}>{title}</div>

                    {alternateTitles.length ? (
                      <div className={styles.alternateTitlesIconContainer}>
                        <Popover
                          anchor={
                            <Icon name={icons.ALTERNATE_TITLES} size={20} />
                          }
                          title={translate('AlternateTitles')}
                          body={
                            <SeriesAlternateTitles
                              alternateTitles={alternateTitles}
                            />
                          }
                          position={tooltipPositions.BOTTOM}
                        />
                      </div>
                    ) : null}
                  </div>

                  <div className={styles.seriesNavigationButtons}>
                    {previousSeries ? (
                      <IconButton
                        className={styles.seriesNavigationButton}
                        name={icons.ARROW_LEFT}
                        size={30}
                        title={translate('SeriesDetailsGoTo', {
                          title: previousSeries.title,
                        })}
                        to={`/series/${previousSeries.titleSlug}`}
                      />
                    ) : null}

                    {nextSeries ? (
                      <IconButton
                        className={styles.seriesNavigationButton}
                        name={icons.ARROW_RIGHT}
                        size={30}
                        title={translate('SeriesDetailsGoTo', {
                          title: nextSeries.title,
                        })}
                        to={`/series/${nextSeries.titleSlug}`}
                      />
                    ) : null}
                  </div>
                </div>

                <div className={styles.details}>
                  <div>
                    {runtime ? (
                      <span className={styles.runtime}>
                        {translate('SeriesDetailsRuntime', { runtime })}
                      </span>
                    ) : null}

                    {ratings.value ? (
                      <HeartRating
                        rating={ratings.value}
                        votes={ratings.votes}
                        iconSize={20}
                      />
                    ) : null}

                    <SeriesGenres className={styles.genres} genres={genres} />

                    <span>{runningYears}</span>
                  </div>
                </div>

                <div>
                  <Label className={styles.detailsLabel} size={sizes.LARGE}>
                    <div>
                      <Icon name={icons.FOLDER} size={17} />
                      <span className={styles.path}>{path}</span>
                    </div>
                  </Label>

                  <Tooltip
                    anchor={
                      <Label className={styles.detailsLabel} size={sizes.LARGE}>
                        <div>
                          <Icon name={icons.DRIVE} size={17} />

                          <span className={styles.sizeOnDisk}>
                            {formatBytes(sizeOnDisk)}
                          </span>
                        </div>
                      </Label>
                    }
                    tooltip={<span>{episodeFilesCountMessage}</span>}
                    kind={kinds.INVERSE}
                    position={tooltipPositions.BOTTOM}
                  />

                  <Label
                    className={styles.detailsLabel}
                    title={translate('QualityProfile')}
                    size={sizes.LARGE}
                  >
                    <div>
                      <Icon name={icons.PROFILE} size={17} />
                      <span className={styles.qualityProfileName}>
                        <QualityProfileName
                          qualityProfileId={qualityProfileId}
                        />
                      </span>
                    </div>
                  </Label>

                  <Label className={styles.detailsLabel} size={sizes.LARGE}>
                    <div>
                      <Icon
                        name={monitored ? icons.MONITORED : icons.UNMONITORED}
                        size={17}
                      />
                      <span className={styles.qualityProfileName}>
                        {monitored
                          ? translate('Monitored')
                          : translate('Unmonitored')}
                      </span>
                    </div>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    title={statusDetails.message}
                    size={sizes.LARGE}
                    kind={status === 'deleted' ? kinds.INVERSE : undefined}
                  >
                    <div>
                      <Icon name={statusDetails.icon} size={17} />
                      <span className={styles.statusName}>
                        {statusDetails.title}
                      </span>
                    </div>
                  </Label>

                  {originalLanguage?.name ? (
                    <Label
                      className={styles.detailsLabel}
                      title={translate('OriginalLanguage')}
                      size={sizes.LARGE}
                    >
                      <div>
                        <Icon name={icons.LANGUAGE} size={17} />
                        <span className={styles.originalLanguageName}>
                          {originalLanguage.name}
                        </span>
                      </div>
                    </Label>
                  ) : null}

                  {network ? (
                    <Label
                      className={styles.detailsLabel}
                      title={translate('Network')}
                      size={sizes.LARGE}
                    >
                      <div>
                        <Icon name={icons.NETWORK} size={17} />
                        <span className={styles.network}>{network}</span>
                      </div>
                    </Label>
                  ) : null}

                  <Tooltip
                    anchor={
                      <Label className={styles.detailsLabel} size={sizes.LARGE}>
                        <div>
                          <Icon name={icons.EXTERNAL_LINK} size={17} />
                          <span className={styles.links}>
                            {translate('Links')}
                          </span>
                        </div>
                      </Label>
                    }
                    tooltip={
                      <SeriesDetailsLinks
                        tvdbId={tvdbId}
                        tvMazeId={tvMazeId}
                        imdbId={imdbId}
                        tmdbId={tmdbId}
                      />
                    }
                    kind={kinds.INVERSE}
                    position={tooltipPositions.BOTTOM}
                  />

                  {tags.length ? (
                    <Tooltip
                      anchor={
                        <Label
                          className={styles.detailsLabel}
                          size={sizes.LARGE}
                        >
                          <Icon name={icons.TAGS} size={17} />

                          <span className={styles.tags}>
                            {translate('Tags')}
                          </span>
                        </Label>
                      }
                      tooltip={<SeriesTags seriesId={seriesId} />}
                      kind={kinds.INVERSE}
                      position={tooltipPositions.BOTTOM}
                    />
                  ) : null}

                  <SeriesProgressLabel
                    className={styles.seriesProgressLabel}
                    seriesId={seriesId}
                    monitored={monitored}
                    episodeCount={episodeCount}
                    episodeFileCount={episodeFileCount}
                  />
                </div>

                <div className={styles.overview}>{overview}</div>

                <MetadataAttribution />
              </div>
            </div>
          </div>

          <div className={styles.contentContainer}>
            {!isPopulated && !episodesError && !episodeFilesError ? (
              <LoadingIndicator />
            ) : null}

            {!isFetching && episodesError ? (
              <Alert kind={kinds.DANGER}>
                {translate('EpisodesLoadError')}
              </Alert>
            ) : null}

            {!isFetching && episodeFilesError ? (
              <Alert kind={kinds.DANGER}>
                {translate('EpisodeFilesLoadError')}
              </Alert>
            ) : null}

            {isPopulated && !!seasons.length ? (
              <div>
                {seasons
                  .slice(0)
                  .reverse()
                  .map((season) => {
                    return (
                      <SeriesDetailsSeason
                        key={season.seasonNumber}
                        seriesId={seriesId}
                        {...season}
                        isExpanded={expandedState.seasons[season.seasonNumber]}
                        onExpandPress={handleExpandPress}
                      />
                    );
                  })}
              </div>
            ) : null}

            {isPopulated && !seasons.length ? (
              <Alert kind={kinds.WARNING}>
                {translate('NoEpisodeInformation')}
              </Alert>
            ) : null}
          </div>

          <OrganizePreviewModal
            isOpen={isOrganizeModalOpen}
            seriesId={seriesId}
            onModalClose={handleOrganizeModalClose}
          />

          <InteractiveImportModal
            isOpen={isManageEpisodesOpen}
            seriesId={seriesId}
            title={title}
            folder={path}
            initialSortKey="relativePath"
            initialSortDirection={sortDirections.DESCENDING}
            showSeries={false}
            allowSeriesChange={false}
            showDelete={true}
            showImportMode={false}
            modalTitle={translate('ManageEpisodes')}
            onModalClose={handleManageEpisodesModalClose}
          />

          <SeriesHistoryModal
            isOpen={isSeriesHistoryModalOpen}
            seriesId={seriesId}
            onModalClose={handleSeriesHistoryModalClose}
          />

          <EditSeriesModal
            isOpen={isEditSeriesModalOpen}
            seriesId={seriesId}
            onModalClose={handleEditSeriesModalClose}
            onDeleteSeriesPress={handleDeleteSeriesPress}
          />

          <DeleteSeriesModal
            isOpen={isDeleteSeriesModalOpen}
            seriesId={seriesId}
            onModalClose={handleDeleteSeriesModalClose}
          />

          <MonitoringOptionsModal
            isOpen={isMonitorOptionsModalOpen}
            seriesId={seriesId}
            onModalClose={handleMonitorOptionsClose}
          />
        </PageContentBody>
      </PageContent>
    </SeriesDetailsProvider>
  );
}

export default SeriesDetails;
