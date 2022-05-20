import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
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
import { align, icons, kinds, sizes, sortDirections, tooltipPositions } from 'Helpers/Props';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import OrganizePreviewModalConnector from 'Organize/OrganizePreviewModalConnector';
import SeriesHistoryModal from 'Series/History/SeriesHistoryModal';
import SeasonInteractiveSearchModalConnector from 'Series/Search/SeasonInteractiveSearchModalConnector';
import isAfter from 'Utilities/Date/isAfter';
import isBefore from 'Utilities/Date/isBefore';
import formatBytes from 'Utilities/Number/formatBytes';
import getToggledRange from 'Utilities/Table/getToggledRange';
import EpisodeRowConnector from './EpisodeRowConnector';
import SeasonInfo from './SeasonInfo';
import styles from './SeriesDetailsSeason.css';

function getSeasonStatistics(episodes) {
  let episodeCount = 0;
  let episodeFileCount = 0;
  let totalEpisodeCount = 0;
  let monitoredEpisodeCount = 0;
  let hasMonitoredEpisodes = false;
  const sizeOnDisk = 0;

  episodes.forEach((episode) => {
    if (episode.episodeFileId || (episode.monitored && isBefore(episode.airDateUtc))) {
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
    sizeOnDisk
  };
}

function getEpisodeCountKind(monitored, episodeFileCount, episodeCount) {
  if (episodeFileCount === episodeCount && episodeCount > 0) {
    return kinds.SUCCESS;
  }

  if (!monitored) {
    return kinds.WARNING;
  }

  return kinds.DANGER;
}

class SeriesDetailsSeason extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isOrganizeModalOpen: false,
      isManageEpisodesOpen: false,
      isHistoryModalOpen: false,
      isInteractiveSearchModalOpen: false,
      lastToggledEpisode: null
    };
  }

  componentDidMount() {
    this._expandByDefault();
  }

  componentDidUpdate(prevProps) {
    const {
      seriesId,
      items
    } = this.props;

    if (prevProps.seriesId !== seriesId) {
      this._expandByDefault();
      return;
    }

    if (
      getSeasonStatistics(prevProps.items).episodeFileCount > 0 &&
      getSeasonStatistics(items).episodeFileCount === 0
    ) {
      this.setState({
        isOrganizeModalOpen: false,
        isManageEpisodesOpen: false
      });
    }
  }

  //
  // Control

  _expandByDefault() {
    const {
      seasonNumber,
      onExpandPress,
      items
    } = this.props;

    const expand = _.some(items, (item) => {
      return isAfter(item.airDateUtc) ||
             isAfter(item.airDateUtc, { days: -30 });
    });

    onExpandPress(seasonNumber, expand && seasonNumber > 0);
  }

  //
  // Listeners

  onOrganizePress = () => {
    this.setState({ isOrganizeModalOpen: true });
  };

  onOrganizeModalClose = () => {
    this.setState({ isOrganizeModalOpen: false });
  };

  onManageEpisodesPress = () => {
    this.setState({ isManageEpisodesOpen: true });
  };

  onManageEpisodesModalClose = () => {
    this.setState({ isManageEpisodesOpen: false });
  };

  onHistoryPress = () => {
    this.setState({ isHistoryModalOpen: true });
  };

  onHistoryModalClose = () => {
    this.setState({ isHistoryModalOpen: false });
  };

  onInteractiveSearchPress = () => {
    this.setState({ isInteractiveSearchModalOpen: true });
  };

  onInteractiveSearchModalClose = () => {
    this.setState({ isInteractiveSearchModalOpen: false });
  };

  onExpandPress = () => {
    const {
      seasonNumber,
      isExpanded
    } = this.props;

    this.props.onExpandPress(seasonNumber, !isExpanded);
  };

  onMonitorEpisodePress = (episodeId, monitored, { shiftKey }) => {
    const lastToggled = this.state.lastToggledEpisode;
    const episodeIds = [episodeId];

    if (shiftKey && lastToggled) {
      const { lower, upper } = getToggledRange(this.props.items, episodeId, lastToggled);
      const items = this.props.items;

      for (let i = lower; i < upper; i++) {
        episodeIds.push(items[i].id);
      }
    }

    this.setState({ lastToggledEpisode: episodeId });

    this.props.onMonitorEpisodePress(_.uniq(episodeIds), monitored);
  };

  //
  // Render

  render() {
    const {
      seriesId,
      path,
      monitored,
      seasonNumber,
      items,
      columns,
      statistics,
      isSaving,
      isExpanded,
      isSearching,
      seriesMonitored,
      isSmallScreen,
      onTableOptionChange,
      onMonitorSeasonPress,
      onSearchPress
    } = this.props;

    const {
      episodeCount,
      episodeFileCount,
      totalEpisodeCount,
      monitoredEpisodeCount,
      hasMonitoredEpisodes
    } = getSeasonStatistics(items);

    const {
      isOrganizeModalOpen,
      isManageEpisodesOpen,
      isHistoryModalOpen,
      isInteractiveSearchModalOpen
    } = this.state;

    const title = seasonNumber === 0 ? 'Specials' : `Season ${seasonNumber}`;

    return (
      <div
        className={styles.season}
      >
        <div className={styles.header}>
          <div className={styles.left}>
            <MonitorToggleButton
              monitored={monitored}
              isDisabled={!seriesMonitored}
              isSaving={isSaving}
              size={24}
              onPress={onMonitorSeasonPress}
            />

            <span className={styles.seasonNumber}>
              {title}
            </span>

            <Popover
              className={styles.episodeCountTooltip}
              anchor={
                <Label
                  kind={getEpisodeCountKind(monitored, episodeFileCount, episodeCount)}
                  size={sizes.LARGE}
                >
                  <span>{episodeFileCount} / {episodeCount}</span>
                </Label>
              }
              title="Season Information"
              body={
                <div>
                  <SeasonInfo
                    totalEpisodeCount={totalEpisodeCount}
                    monitoredEpisodeCount={monitoredEpisodeCount}
                    episodeFileCount={episodeFileCount}
                    sizeOnDisk={statistics.sizeOnDisk}
                  />
                </div>
              }
              position={tooltipPositions.BOTTOM}
            />

            {
              statistics.sizeOnDisk ?
                <div className={styles.sizeOnDisk}>
                  {formatBytes(statistics.sizeOnDisk)}
                </div> :
                null
            }
          </div>

          <Link
            className={styles.expandButton}
            onPress={this.onExpandPress}
          >
            <Icon
              className={styles.expandButtonIcon}
              name={isExpanded ? icons.COLLAPSE : icons.EXPAND}
              title={isExpanded ? 'Hide episodes' : 'Show episodes'}
              size={24}
            />
            {
              !isSmallScreen &&
                <span>&nbsp;</span>
            }
          </Link>

          {
            isSmallScreen ?
              <Menu
                className={styles.actionsMenu}
                alignMenu={align.RIGHT}
                enforceMaxHeight={false}
              >
                <MenuButton>
                  <Icon
                    name={icons.ACTIONS}
                    size={22}
                  />
                </MenuButton>

                <MenuContent className={styles.actionsMenuContent}>
                  <MenuItem
                    isDisabled={isSearching || !hasMonitoredEpisodes || !seriesMonitored}
                    onPress={onSearchPress}
                  >
                    <SpinnerIcon
                      className={styles.actionMenuIcon}
                      name={icons.SEARCH}
                      isSpinning={isSearching}
                    />

                    Search
                  </MenuItem>

                  <MenuItem
                    onPress={this.onInteractiveSearchPress}
                    isDisabled={!totalEpisodeCount}
                  >
                    <Icon
                      className={styles.actionMenuIcon}
                      name={icons.INTERACTIVE}
                    />

                    Interactive Search
                  </MenuItem>

                  <MenuItem
                    onPress={this.onOrganizePress}
                    isDisabled={!episodeFileCount}
                  >
                    <Icon
                      className={styles.actionMenuIcon}
                      name={icons.ORGANIZE}
                    />

                    Preview Rename
                  </MenuItem>

                  <MenuItem
                    onPress={this.onManageEpisodesPress}
                    isDisabled={!episodeFileCount}
                  >
                    <Icon
                      className={styles.actionMenuIcon}
                      name={icons.EPISODE_FILE}
                    />

                    Manage Episodes
                  </MenuItem>

                  <MenuItem
                    onPress={this.onHistoryPress}
                    isDisabled={!totalEpisodeCount}
                  >
                    <Icon
                      className={styles.actionMenuIcon}
                      name={icons.HISTORY}
                    />

                    History
                  </MenuItem>
                </MenuContent>
              </Menu> :

              <div className={styles.actions}>
                <SpinnerIconButton
                  className={styles.actionButton}
                  name={icons.SEARCH}
                  title={hasMonitoredEpisodes && seriesMonitored ? 'Search for monitored episodes in this season' : 'No monitored episodes in this season'}
                  size={24}
                  isSpinning={isSearching}
                  isDisabled={isSearching || !hasMonitoredEpisodes || !seriesMonitored}
                  onPress={onSearchPress}
                />

                <IconButton
                  className={styles.actionButton}
                  name={icons.INTERACTIVE}
                  title="Interactive search for all episodes in this season"
                  size={24}
                  isDisabled={!totalEpisodeCount}
                  onPress={this.onInteractiveSearchPress}
                />

                <IconButton
                  className={styles.actionButton}
                  name={icons.ORGANIZE}
                  title="Preview rename for this season"
                  size={24}
                  isDisabled={!episodeFileCount}
                  onPress={this.onOrganizePress}
                />

                <IconButton
                  className={styles.actionButton}
                  name={icons.EPISODE_FILE}
                  title="Manage episode files in this season"
                  size={24}
                  isDisabled={!episodeFileCount}
                  onPress={this.onManageEpisodesPress}
                />

                <IconButton
                  className={styles.actionButton}
                  name={icons.HISTORY}
                  title="View history for this season"
                  size={24}
                  isDisabled={!totalEpisodeCount}
                  onPress={this.onHistoryPress}
                />
              </div>
          }

        </div>

        <div>
          {
            isExpanded &&
              <div className={styles.episodes}>
                {
                  items.length ?
                    <Table
                      columns={columns}
                      onTableOptionChange={onTableOptionChange}
                    >
                      <TableBody>
                        {
                          items.map((item) => {
                            return (
                              <EpisodeRowConnector
                                key={item.id}
                                columns={columns}
                                {...item}
                                onMonitorEpisodePress={this.onMonitorEpisodePress}
                              />
                            );
                          })
                        }
                      </TableBody>
                    </Table> :

                    <div className={styles.noEpisodes}>
                      No episodes in this season
                    </div>
                }
                <div className={styles.collapseButtonContainer}>
                  <IconButton
                    iconClassName={styles.collapseButtonIcon}
                    name={icons.COLLAPSE}
                    size={20}
                    title="Hide episodes"
                    onPress={this.onExpandPress}
                  />
                </div>
              </div>
          }
        </div>

        <OrganizePreviewModalConnector
          isOpen={isOrganizeModalOpen}
          seriesId={seriesId}
          seasonNumber={seasonNumber}
          onModalClose={this.onOrganizeModalClose}
        />

        <InteractiveImportModal
          isOpen={isManageEpisodesOpen}
          seriesId={seriesId}
          seasonNumber={seasonNumber}
          title={title}
          folder={path}
          initialSortKey="relativePath"
          initialSortDirection={sortDirections.DESCENDING}
          showSeries={false}
          allowSeriesChange={false}
          autoSelectRow={false}
          showDelete={true}
          showImportMode={false}
          onModalClose={this.onManageEpisodesModalClose}
        />

        <SeriesHistoryModal
          isOpen={isHistoryModalOpen}
          seriesId={seriesId}
          seasonNumber={seasonNumber}
          onModalClose={this.onHistoryModalClose}
        />

        <SeasonInteractiveSearchModalConnector
          isOpen={isInteractiveSearchModalOpen}
          seriesId={seriesId}
          seasonNumber={seasonNumber}
          onModalClose={this.onInteractiveSearchModalClose}
        />
      </div>
    );
  }
}

SeriesDetailsSeason.propTypes = {
  seriesId: PropTypes.number.isRequired,
  path: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  seasonNumber: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  statistics: PropTypes.object.isRequired,
  isSaving: PropTypes.bool,
  isExpanded: PropTypes.bool,
  isSearching: PropTypes.bool.isRequired,
  seriesMonitored: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onTableOptionChange: PropTypes.func.isRequired,
  onMonitorSeasonPress: PropTypes.func.isRequired,
  onExpandPress: PropTypes.func.isRequired,
  onMonitorEpisodePress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

SeriesDetailsSeason.defaultProps = {
  statistics: {}
};

export default SeriesDetailsSeason;
