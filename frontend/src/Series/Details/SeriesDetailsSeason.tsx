import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import EpisodesAppState from 'App/State/EpisodesAppState';
import * as commandNames from 'Commands/commandNames';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import Menu from 'Components/Menu/Menu';
import MenuButton from 'Components/Menu/MenuButton';
import MenuContent from 'Components/Menu/MenuContent';
import MenuItem from 'Components/Menu/MenuItem';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import SpinnerIcon from 'Components/SpinnerIcon';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import Popover from 'Components/Tooltip/Popover';
import Episode from 'Episode/Episode';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { align, icons, sortDirections, tooltipPositions } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import OrganizePreviewModal from 'Organize/OrganizePreviewModal';
import SeriesHistoryModal from 'Series/History/SeriesHistoryModal';
import SeasonInteractiveSearchModal from 'Series/Search/SeasonInteractiveSearchModal';
import { Statistics } from 'Series/Series';
import useSeries from 'Series/useSeries';
import {
  setEpisodesSort,
  setEpisodesTableOption,
  toggleEpisodesMonitored,
} from 'Store/Actions/episodeActions';
import { toggleSeasonMonitored } from 'Store/Actions/seriesActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { TableOptionsChangePayload } from 'typings/Table';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import isAfter from 'Utilities/Date/isAfter';
import isBefore from 'Utilities/Date/isBefore';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import getToggledRange from 'Utilities/Table/getToggledRange';
import EpisodeRow from './EpisodeRow';
import SeasonInfo from './SeasonInfo';
import SeasonProgressLabel from './SeasonProgressLabel';
import styles from './SeriesDetailsSeason.css';

function getSeasonStatistics(episodes: Episode[]) {
  let episodeCount = 0;
  let episodeFileCount = 0;
  let totalEpisodeCount = 0;
  let monitoredEpisodeCount = 0;
  let hasMonitoredEpisodes = false;
  const sizeOnDisk = 0;

  episodes.forEach((episode) => {
    if (
      episode.episodeFileId ||
      (episode.monitored && isBefore(episode.airDateUtc))
    ) {
      episodeCount++;
    }

    if (episode.episodeFileId) {
      episodeFileCount++;
    }

    if (episode.monitored) {
      monitoredEpisodeCount++;
      hasMonitoredEpisodes = true;
    }

    totalEpisodeCount++;
  });

  return {
    episodeCount,
    episodeFileCount,
    totalEpisodeCount,
    monitoredEpisodeCount,
    hasMonitoredEpisodes,
    sizeOnDisk,
  };
}

function createEpisodesSelector(seasonNumber: number) {
  return createSelector(
    createClientSideCollectionSelector('episodes'),
    (episodes: EpisodesAppState) => {
      const { items, columns, sortKey, sortDirection } = episodes;

      const episodesInSeason = items.filter(
        (episode) => episode.seasonNumber === seasonNumber
      );

      return { items: episodesInSeason, columns, sortKey, sortDirection };
    }
  );
}

function createIsSearchingSelector(seriesId: number, seasonNumber: number) {
  return createSelector(createCommandsSelector(), (commands) => {
    return isCommandExecuting(
      findCommand(commands, {
        name: commandNames.SEASON_SEARCH,
        seriesId,
        seasonNumber,
      })
    );
  });
}

interface SeriesDetailsSeasonProps {
  seriesId: number;
  monitored: boolean;
  seasonNumber: number;
  statistics?: Statistics;
  isSaving?: boolean;
  isExpanded?: boolean;
  onExpandPress: (seasonNumber: number, isExpanded: boolean) => void;
}

function SeriesDetailsSeason({
  seriesId,
  monitored,
  seasonNumber,
  statistics = {} as Statistics,
  isSaving,
  isExpanded,
  onExpandPress,
}: SeriesDetailsSeasonProps) {
  const dispatch = useDispatch();
  const { monitored: seriesMonitored, path } = useSeries(seriesId)!;

  const { items, columns, sortKey, sortDirection } = useSelector(
    createEpisodesSelector(seasonNumber)
  );

  const { isSmallScreen } = useSelector(createDimensionsSelector());
  const isSearching = useSelector(
    createIsSearchingSelector(seriesId, seasonNumber)
  );

  const { sizeOnDisk = 0 } = statistics;

  const {
    episodeCount,
    episodeFileCount,
    totalEpisodeCount,
    monitoredEpisodeCount,
    hasMonitoredEpisodes,
  } = getSeasonStatistics(items);

  const previousEpisodeFileCount = usePrevious(episodeFileCount);

  const [isOrganizeModalOpen, setIsOrganizeModalOpen] = useState(false);
  const [isManageEpisodesOpen, setIsManageEpisodesOpen] = useState(false);
  const [isHistoryModalOpen, setIsHistoryModalOpen] = useState(false);
  const [isInteractiveSearchModalOpen, setIsInteractiveSearchModalOpen] =
    useState(false);

  const lastToggledEpisode = useRef<number | null>(null);
  const itemsRef = useRef(items);

  itemsRef.current = items;

  const seasonNumberTitle =
    seasonNumber === 0
      ? translate('Specials')
      : translate('SeasonNumberToken', { seasonNumber });

  const handleMonitorSeasonPress = useCallback(
    (value: boolean) => {
      dispatch(
        toggleSeasonMonitored({
          seriesId,
          seasonNumber,
          monitored: value,
        })
      );
    },
    [seriesId, seasonNumber, dispatch]
  );

  const handleExpandPress = useCallback(() => {
    onExpandPress(seasonNumber, !isExpanded);
  }, [seasonNumber, isExpanded, onExpandPress]);

  const handleMonitorEpisodePress = useCallback(
    (
      episodeId: number,
      value: boolean,
      { shiftKey }: { shiftKey: boolean }
    ) => {
      const lastToggled = lastToggledEpisode.current;
      const episodeIds = [episodeId];

      if (shiftKey && lastToggled) {
        const { lower, upper } = getToggledRange(items, episodeId, lastToggled);
        for (let i = lower; i < upper; i++) {
          episodeIds.push(items[i].id);
        }
      }

      lastToggledEpisode.current = episodeId;

      dispatch(
        toggleEpisodesMonitored({
          episodeIds,
          value,
        })
      );
    },
    [items, dispatch]
  );

  const handleSearchPress = useCallback(() => {
    dispatch({
      name: commandNames.SEASON_SEARCH,
      seriesId,
      seasonNumber,
    });
  }, [seriesId, seasonNumber, dispatch]);

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

  const handleHistoryPress = useCallback(() => {
    setIsHistoryModalOpen(true);
  }, []);

  const handleHistoryModalClose = useCallback(() => {
    setIsHistoryModalOpen(false);
  }, []);

  const handleInteractiveSearchPress = useCallback(() => {
    setIsInteractiveSearchModalOpen(true);
  }, []);

  const handleInteractiveSearchModalClose = useCallback(() => {
    setIsInteractiveSearchModalOpen(false);
  }, []);

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      dispatch(
        setEpisodesSort({
          sortKey,
          sortDirection,
        })
      );
    },
    [dispatch]
  );

  const handleTableOptionChange = useCallback(
    (payload: TableOptionsChangePayload) => {
      dispatch(setEpisodesTableOption(payload));
    },
    [dispatch]
  );

  useEffect(() => {
    const expand =
      itemsRef.current.some(
        (item) =>
          isAfter(item.airDateUtc) || isAfter(item.airDateUtc, { days: -30 })
      ) || itemsRef.current.every((item) => !item.airDateUtc);

    onExpandPress(seasonNumber, expand && seasonNumber > 0);
  }, [seriesId, seasonNumber, onExpandPress]);

  useEffect(() => {
    if ((previousEpisodeFileCount ?? 0) > 0 && episodeFileCount === 0) {
      setIsOrganizeModalOpen(false);
      setIsManageEpisodesOpen(false);
    }
  }, [episodeFileCount, previousEpisodeFileCount]);

  return (
    <div className={styles.season}>
      <div className={styles.header}>
        <div className={styles.left}>
          <MonitorToggleButton
            monitored={monitored}
            isDisabled={!seriesMonitored}
            isSaving={isSaving}
            size={24}
            onPress={handleMonitorSeasonPress}
          />

          <div className={styles.seasonInfo}>
            <div className={styles.seasonNumber}>{seasonNumberTitle}</div>
          </div>

          <div className={styles.seasonStats}>
            <Popover
              className={styles.episodeCountTooltip}
              canFlip={true}
              anchor={
                <SeasonProgressLabel
                  className={styles.seasonStatsLabel}
                  seriesId={seriesId}
                  seasonNumber={seasonNumber}
                  monitored={monitored}
                  episodeCount={episodeCount}
                  episodeFileCount={episodeFileCount}
                />
              }
              title={translate('SeasonInformation')}
              body={
                <div>
                  <SeasonInfo
                    totalEpisodeCount={totalEpisodeCount}
                    monitoredEpisodeCount={monitoredEpisodeCount}
                    episodeFileCount={episodeFileCount}
                    sizeOnDisk={sizeOnDisk}
                  />
                </div>
              }
              position={tooltipPositions.BOTTOM}
            />

            {sizeOnDisk ? (
              <Label
                className={styles.seasonStatsLabel}
                kind="default"
                size="large"
              >
                {formatBytes(sizeOnDisk)}
              </Label>
            ) : null}
          </div>
        </div>

        <Link className={styles.expandButton} onPress={handleExpandPress}>
          <Icon
            className={styles.expandButtonIcon}
            name={isExpanded ? icons.COLLAPSE : icons.EXPAND}
            title={
              isExpanded ? translate('HideEpisodes') : translate('ShowEpisodes')
            }
            size={24}
          />
          {isSmallScreen ? null : <span>&nbsp;</span>}
        </Link>

        {isSmallScreen ? (
          <Menu
            className={styles.actionsMenu}
            alignMenu={align.RIGHT}
            enforceMaxHeight={false}
          >
            <MenuButton>
              <Icon name={icons.ACTIONS} size={22} />
            </MenuButton>

            <MenuContent className={styles.actionsMenuContent}>
              <MenuItem
                isDisabled={
                  isSearching || !hasMonitoredEpisodes || !seriesMonitored
                }
                onPress={handleSearchPress}
              >
                <SpinnerIcon
                  className={styles.actionMenuIcon}
                  name={icons.SEARCH}
                  isSpinning={isSearching}
                />

                {translate('Search')}
              </MenuItem>

              <MenuItem
                isDisabled={!totalEpisodeCount}
                onPress={handleInteractiveSearchPress}
              >
                <Icon
                  className={styles.actionMenuIcon}
                  name={icons.INTERACTIVE}
                />

                {translate('InteractiveSearch')}
              </MenuItem>

              <MenuItem
                isDisabled={!episodeFileCount}
                onPress={handleOrganizePress}
              >
                <Icon className={styles.actionMenuIcon} name={icons.ORGANIZE} />

                {translate('PreviewRename')}
              </MenuItem>

              <MenuItem
                isDisabled={!episodeFileCount}
                onPress={handleManageEpisodesPress}
              >
                <Icon
                  className={styles.actionMenuIcon}
                  name={icons.EPISODE_FILE}
                />

                {translate('ManageEpisodes')}
              </MenuItem>

              <MenuItem
                isDisabled={!totalEpisodeCount}
                onPress={handleHistoryPress}
              >
                <Icon className={styles.actionMenuIcon} name={icons.HISTORY} />

                {translate('History')}
              </MenuItem>
            </MenuContent>
          </Menu>
        ) : (
          <div className={styles.actions}>
            <SpinnerIconButton
              className={styles.actionButton}
              name={icons.SEARCH}
              title={
                hasMonitoredEpisodes && seriesMonitored
                  ? translate('SearchForMonitoredEpisodesSeason')
                  : translate('NoMonitoredEpisodesSeason')
              }
              size={24}
              isSpinning={isSearching}
              isDisabled={
                isSearching || !hasMonitoredEpisodes || !seriesMonitored
              }
              onPress={handleSearchPress}
            />

            <IconButton
              className={styles.actionButton}
              name={icons.INTERACTIVE}
              title={translate('InteractiveSearchSeason')}
              size={24}
              isDisabled={!totalEpisodeCount}
              onPress={handleInteractiveSearchPress}
            />

            <IconButton
              className={styles.actionButton}
              name={icons.ORGANIZE}
              title={translate('PreviewRenameSeason')}
              size={24}
              isDisabled={!episodeFileCount}
              onPress={handleOrganizePress}
            />

            <IconButton
              className={styles.actionButton}
              name={icons.EPISODE_FILE}
              title={translate('ManageEpisodesSeason')}
              size={24}
              isDisabled={!episodeFileCount}
              onPress={handleManageEpisodesPress}
            />

            <IconButton
              className={styles.actionButton}
              name={icons.HISTORY}
              title={translate('HistorySeason')}
              size={24}
              isDisabled={!totalEpisodeCount}
              onPress={handleHistoryPress}
            />
          </div>
        )}
      </div>

      <div>
        {isExpanded ? (
          <div className={styles.episodes}>
            {items.length ? (
              <Table
                columns={columns}
                sortKey={sortKey}
                sortDirection={sortDirection}
                onSortPress={handleSortPress}
                onTableOptionChange={handleTableOptionChange}
              >
                <TableBody>
                  {items.map((item) => {
                    return (
                      <EpisodeRow
                        key={item.id}
                        columns={columns}
                        {...item}
                        onMonitorEpisodePress={handleMonitorEpisodePress}
                      />
                    );
                  })}
                </TableBody>
              </Table>
            ) : (
              <div className={styles.noEpisodes}>
                {translate('NoEpisodesInThisSeason')}
              </div>
            )}

            <div className={styles.collapseButtonContainer}>
              <IconButton
                iconClassName={styles.collapseButtonIcon}
                name={icons.COLLAPSE}
                size={20}
                title={translate('HideEpisodes')}
                onPress={handleExpandPress}
              />
            </div>
          </div>
        ) : null}
      </div>

      <OrganizePreviewModal
        isOpen={isOrganizeModalOpen}
        seriesId={seriesId}
        seasonNumber={seasonNumber}
        onModalClose={handleOrganizeModalClose}
      />

      <InteractiveImportModal
        isOpen={isManageEpisodesOpen}
        seriesId={seriesId}
        seasonNumber={seasonNumber}
        title={seasonNumberTitle}
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
        isOpen={isHistoryModalOpen}
        seriesId={seriesId}
        seasonNumber={seasonNumber}
        onModalClose={handleHistoryModalClose}
      />

      <SeasonInteractiveSearchModal
        isOpen={isInteractiveSearchModalOpen}
        episodeCount={totalEpisodeCount}
        seriesId={seriesId}
        seasonNumber={seasonNumber}
        onModalClose={handleInteractiveSearchModalClose}
      />
    </div>
  );
}

export default SeriesDetailsSeason;
