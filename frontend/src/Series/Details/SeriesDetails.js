import _ from 'lodash';
import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextTruncate from 'react-text-truncate';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Measure from 'Components/Measure';
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
import { align, icons, kinds, sizes, sortDirections, tooltipPositions } from 'Helpers/Props';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import OrganizePreviewModalConnector from 'Organize/OrganizePreviewModalConnector';
import DeleteSeriesModal from 'Series/Delete/DeleteSeriesModal';
import EditSeriesModalConnector from 'Series/Edit/EditSeriesModalConnector';
import SeriesHistoryModal from 'Series/History/SeriesHistoryModal';
import MonitoringOptionsModal from 'Series/MonitoringOptions/MonitoringOptionsModal';
import SeriesPoster from 'Series/SeriesPoster';
import { getSeriesStatusDetails } from 'Series/SeriesStatus';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import fonts from 'Styles/Variables/fonts';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import SeriesAlternateTitles from './SeriesAlternateTitles';
import SeriesDetailsLinks from './SeriesDetailsLinks';
import SeriesDetailsSeasonConnector from './SeriesDetailsSeasonConnector';
import SeriesGenres from './SeriesGenres';
import SeriesTagsConnector from './SeriesTagsConnector';
import styles from './SeriesDetails.css';

const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

function getFanartUrl(images) {
  return _.find(images, { coverType: 'fanart' })?.url;
}

function getExpandedState(newState) {
  return {
    allExpanded: newState.allSelected,
    allCollapsed: newState.allUnselected,
    expandedState: newState.selectedState
  };
}

function getDateYear(date) {
  const dateDate = moment(date);

  return dateDate.format('YYYY');
}

class SeriesDetails extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isOrganizeModalOpen: false,
      isManageEpisodesOpen: false,
      isEditSeriesModalOpen: false,
      isDeleteSeriesModalOpen: false,
      isSeriesHistoryModalOpen: false,
      isMonitorOptionsModalOpen: false,
      allExpanded: false,
      allCollapsed: false,
      expandedState: {},
      overviewHeight: 0
    };
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

  onEditSeriesPress = () => {
    this.setState({ isEditSeriesModalOpen: true });
  };

  onEditSeriesModalClose = () => {
    this.setState({ isEditSeriesModalOpen: false });
  };

  onDeleteSeriesPress = () => {
    this.setState({
      isEditSeriesModalOpen: false,
      isDeleteSeriesModalOpen: true
    });
  };

  onDeleteSeriesModalClose = () => {
    this.setState({ isDeleteSeriesModalOpen: false });
  };

  onSeriesHistoryPress = () => {
    this.setState({ isSeriesHistoryModalOpen: true });
  };

  onSeriesHistoryModalClose = () => {
    this.setState({ isSeriesHistoryModalOpen: false });
  };

  onMonitorOptionsPress = () => {
    this.setState({ isMonitorOptionsModalOpen: true });
  };

  onMonitorOptionsClose = () => {
    this.setState({ isMonitorOptionsModalOpen: false });
  };

  onExpandAllPress = () => {
    const {
      allExpanded,
      expandedState
    } = this.state;

    this.setState(getExpandedState(selectAll(expandedState, !allExpanded)));
  };

  onExpandPress = (seasonNumber, isExpanded) => {
    this.setState((state) => {
      const convertedState = {
        allSelected: state.allExpanded,
        allUnselected: state.allCollapsed,
        selectedState: state.expandedState
      };

      const newState = toggleSelected(convertedState, [], seasonNumber, isExpanded, false);

      return getExpandedState(newState);
    });
  };

  onMeasure = ({ height }) => {
    this.setState({ overviewHeight: height });
  };

  //
  // Render

  render() {
    const {
      id,
      tvdbId,
      tvMazeId,
      imdbId,
      title,
      runtime,
      ratings,
      path,
      statistics,
      qualityProfileId,
      monitored,
      status,
      network,
      overview,
      images,
      seasons,
      alternateTitles,
      genres,
      tags,
      year,
      lastAired,
      isSaving,
      isRefreshing,
      isSearching,
      isFetching,
      isPopulated,
      episodesError,
      episodeFilesError,
      hasEpisodes,
      hasMonitoredEpisodes,
      hasEpisodeFiles,
      previousSeries,
      nextSeries,
      onMonitorTogglePress,
      onRefreshPress,
      onSearchPress
    } = this.props;

    const {
      episodeFileCount,
      sizeOnDisk
    } = statistics;

    const {
      isOrganizeModalOpen,
      isManageEpisodesOpen,
      isEditSeriesModalOpen,
      isDeleteSeriesModalOpen,
      isSeriesHistoryModalOpen,
      isMonitorOptionsModalOpen,
      allExpanded,
      allCollapsed,
      expandedState,
      overviewHeight
    } = this.state;

    const statusDetails = getSeriesStatusDetails(status);
    const runningYears = statusDetails.title === translate('Ended') ? `${year}-${getDateYear(lastAired)}` : `${year}-`;

    let episodeFilesCountMessage = translate('SeriesDetailsNoEpisodeFiles');

    if (episodeFileCount === 1) {
      episodeFilesCountMessage = translate('SeriesDetailsOneEpisodeFile');
    } else if (episodeFileCount > 1) {
      episodeFilesCountMessage = translate('SeriesDetailsCountEpisodeFiles', { episodeFileCount });
    }

    let expandIcon = icons.EXPAND_INDETERMINATE;

    if (allExpanded) {
      expandIcon = icons.COLLAPSE;
    } else if (allCollapsed) {
      expandIcon = icons.EXPAND;
    }

    const fanartUrl = getFanartUrl(images);

    return (
      <PageContent title={title}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label={translate('RefreshAndScan')}
              iconName={icons.REFRESH}
              spinningName={icons.REFRESH}
              title={translate('RefreshAndScanTooltip')}
              isSpinning={isRefreshing}
              onPress={onRefreshPress}
            />

            <PageToolbarButton
              label={translate('SearchMonitored')}
              iconName={icons.SEARCH}
              isDisabled={!monitored || !hasMonitoredEpisodes || !hasEpisodes}
              isSpinning={isSearching}
              title={hasMonitoredEpisodes ? undefined : translate('NoMonitoredEpisodes')}
              onPress={onSearchPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('PreviewRename')}
              iconName={icons.ORGANIZE}
              isDisabled={!hasEpisodeFiles}
              onPress={this.onOrganizePress}
            />

            <PageToolbarButton
              label={translate('ManageEpisodes')}
              iconName={icons.EPISODE_FILE}
              onPress={this.onManageEpisodesPress}
            />

            <PageToolbarButton
              label={translate('History')}
              iconName={icons.HISTORY}
              isDisabled={!hasEpisodes}
              onPress={this.onSeriesHistoryPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('SeriesMonitoring')}
              iconName={icons.MONITORED}
              onPress={this.onMonitorOptionsPress}
            />

            <PageToolbarButton
              label={translate('Edit')}
              iconName={icons.EDIT}
              onPress={this.onEditSeriesPress}
            />

            <PageToolbarButton
              label={translate('Delete')}
              iconName={icons.DELETE}
              onPress={this.onDeleteSeriesPress}
            />

          </PageToolbarSection>

          <PageToolbarSection alignContent={align.RIGHT}>
            <PageToolbarButton
              label={allExpanded ? translate('CollapseAll') : translate('ExpandAll')}
              iconName={expandIcon}
              onPress={this.onExpandAllPress}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody innerClassName={styles.innerContentBody}>
          <div className={styles.header}>
            <div
              className={styles.backdrop}
              style={
                fanartUrl ?
                  { backgroundImage: `url(${fanartUrl})` } :
                  null
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
              />

              <div className={styles.info}>
                <div className={styles.titleRow}>
                  <div className={styles.titleContainer}>
                    <div className={styles.toggleMonitoredContainer}>
                      <MonitorToggleButton
                        className={styles.monitorToggleButton}
                        monitored={monitored}
                        isSaving={isSaving}
                        size={40}
                        onPress={onMonitorTogglePress}
                      />
                    </div>

                    <div className={styles.title}>
                      {title}
                    </div>

                    {
                      !!alternateTitles.length &&
                        <div className={styles.alternateTitlesIconContainer}>
                          <Popover
                            anchor={
                              <Icon
                                name={icons.ALTERNATE_TITLES}
                                size={20}
                              />
                            }
                            title={translate('AlternateTitles')}
                            body={<SeriesAlternateTitles alternateTitles={alternateTitles} />}
                            position={tooltipPositions.BOTTOM}
                          />
                        </div>
                    }
                  </div>

                  <div className={styles.seriesNavigationButtons}>
                    <IconButton
                      className={styles.seriesNavigationButton}
                      name={icons.ARROW_LEFT}
                      size={30}
                      title={translate('SeriesDetailsGoTo', { title: previousSeries.title })}
                      to={`/series/${previousSeries.titleSlug}`}
                    />

                    <IconButton
                      className={styles.seriesNavigationButton}
                      name={icons.ARROW_RIGHT}
                      size={30}
                      title={translate('SeriesDetailsGoTo', { title: nextSeries.title })}
                      to={`/series/${nextSeries.titleSlug}`}
                    />
                  </div>
                </div>

                <div className={styles.details}>
                  <div>
                    {
                      !!runtime &&
                        <span className={styles.runtime}>
                          {translate('SeriesDetailsRuntime', { runtime })}
                        </span>
                    }

                    <HeartRating
                      rating={ratings.value}
                      iconSize={20}
                    />

                    <SeriesGenres genres={genres} />

                    <span>
                      {runningYears}
                    </span>
                  </div>
                </div>

                <div className={styles.detailsLabels}>
                  <Label
                    className={styles.detailsLabel}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.FOLDER}
                      size={17}
                    />

                    <span className={styles.path}>
                      {path}
                    </span>
                  </Label>

                  <Tooltip
                    anchor={
                      <Label
                        className={styles.detailsLabel}
                        size={sizes.LARGE}
                      >
                        <Icon
                          name={icons.DRIVE}
                          size={17}
                        />

                        <span className={styles.sizeOnDisk}>
                          {
                            formatBytes(sizeOnDisk || 0)
                          }
                        </span>
                      </Label>
                    }
                    tooltip={
                      <span>
                        {episodeFilesCountMessage}
                      </span>
                    }
                    kind={kinds.INVERSE}
                    position={tooltipPositions.BOTTOM}
                  />

                  <Label
                    className={styles.detailsLabel}
                    title={translate('QualityProfile')}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.PROFILE}
                      size={17}
                    />

                    <span className={styles.qualityProfileName}>
                      {
                        <QualityProfileNameConnector
                          qualityProfileId={qualityProfileId}
                        />
                      }
                    </span>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={monitored ? icons.MONITORED : icons.UNMONITORED}
                      size={17}
                    />

                    <span className={styles.qualityProfileName}>
                      {monitored ? translate('Monitored') : translate('Unmonitored')}
                    </span>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    title={statusDetails.message}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={statusDetails.icon}
                      size={17}
                    />

                    <span className={styles.qualityProfileName}>
                      {statusDetails.title}
                    </span>
                  </Label>

                  {
                    !!network &&
                      <Label
                        className={styles.detailsLabel}
                        title={translate('Network')}
                        size={sizes.LARGE}
                      >
                        <Icon
                          name={icons.NETWORK}
                          size={17}
                        />

                        <span className={styles.qualityProfileName}>
                          {network}
                        </span>
                      </Label>
                  }

                  <Tooltip
                    anchor={
                      <Label
                        className={styles.detailsLabel}
                        size={sizes.LARGE}
                      >
                        <Icon
                          name={icons.EXTERNAL_LINK}
                          size={17}
                        />

                        <span className={styles.links}>
                          {translate('Links')}
                        </span>
                      </Label>
                    }
                    tooltip={
                      <SeriesDetailsLinks
                        tvdbId={tvdbId}
                        tvMazeId={tvMazeId}
                        imdbId={imdbId}
                      />
                    }
                    kind={kinds.INVERSE}
                    position={tooltipPositions.BOTTOM}
                  />

                  {
                    !!tags.length &&
                      <Tooltip
                        anchor={
                          <Label
                            className={styles.detailsLabel}
                            size={sizes.LARGE}
                          >
                            <Icon
                              name={icons.TAGS}
                              size={17}
                            />

                            <span className={styles.tags}>
                              {translate('Tags')}
                            </span>
                          </Label>
                        }
                        tooltip={<SeriesTagsConnector seriesId={id} />}
                        kind={kinds.INVERSE}
                        position={tooltipPositions.BOTTOM}
                      />

                  }
                </div>

                <Measure onMeasure={this.onMeasure}>
                  <div className={styles.overview}>
                    <TextTruncate
                      line={Math.floor(overviewHeight / (defaultFontSize * lineHeight)) - 1}
                      text={overview}
                    />
                  </div>
                </Measure>

                <MetadataAttribution />
              </div>
            </div>
          </div>

          <div className={styles.contentContainer}>
            {
              !isPopulated && !episodesError && !episodeFilesError &&
                <LoadingIndicator />
            }

            {
              !isFetching && episodesError &&
                <div>{translate('EpisodesLoadError')}</div>
            }

            {
              !isFetching && episodeFilesError &&
                <div>{translate('EpisodeFilesLoadError')}</div>
            }

            {
              isPopulated && !!seasons.length &&
                <div>
                  {
                    seasons.slice(0).reverse().map((season) => {
                      return (
                        <SeriesDetailsSeasonConnector
                          key={season.seasonNumber}
                          seriesId={id}
                          {...season}
                          isExpanded={expandedState[season.seasonNumber]}
                          onExpandPress={this.onExpandPress}
                        />
                      );
                    })
                  }
                </div>
            }

            {
              isPopulated && !seasons.length &&
                <div>
                  {translate('NoEpisodeInformation')}
                </div>
            }

          </div>

          <OrganizePreviewModalConnector
            isOpen={isOrganizeModalOpen}
            seriesId={id}
            onModalClose={this.onOrganizeModalClose}
          />

          <InteractiveImportModal
            isOpen={isManageEpisodesOpen}
            seriesId={id}
            title={title}
            folder={path}
            initialSortKey="relativePath"
            initialSortDirection={sortDirections.DESCENDING}
            showSeries={false}
            allowSeriesChange={false}
            showDelete={true}
            showImportMode={false}
            modalTitle={translate('ManageEpisodes')}
            onModalClose={this.onManageEpisodesModalClose}
          />

          <SeriesHistoryModal
            isOpen={isSeriesHistoryModalOpen}
            seriesId={id}
            onModalClose={this.onSeriesHistoryModalClose}
          />

          <EditSeriesModalConnector
            isOpen={isEditSeriesModalOpen}
            seriesId={id}
            onModalClose={this.onEditSeriesModalClose}
            onDeleteSeriesPress={this.onDeleteSeriesPress}
          />

          <DeleteSeriesModal
            isOpen={isDeleteSeriesModalOpen}
            seriesId={id}
            onModalClose={this.onDeleteSeriesModalClose}
          />

          <MonitoringOptionsModal
            isOpen={isMonitorOptionsModalOpen}
            seriesId={id}
            onModalClose={this.onMonitorOptionsClose}
          />
        </PageContentBody>
      </PageContent>
    );
  }
}

SeriesDetails.propTypes = {
  id: PropTypes.number.isRequired,
  tvdbId: PropTypes.number.isRequired,
  tvMazeId: PropTypes.number,
  imdbId: PropTypes.string,
  title: PropTypes.string.isRequired,
  runtime: PropTypes.number.isRequired,
  ratings: PropTypes.object.isRequired,
  path: PropTypes.string.isRequired,
  statistics: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  monitor: PropTypes.string,
  status: PropTypes.string.isRequired,
  network: PropTypes.string,
  overview: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  seasons: PropTypes.arrayOf(PropTypes.object).isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  year: PropTypes.number.isRequired,
  lastAired: PropTypes.string,
  previousAiring: PropTypes.string,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  isRefreshing: PropTypes.bool.isRequired,
  isSearching: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  episodesError: PropTypes.object,
  episodeFilesError: PropTypes.object,
  hasEpisodes: PropTypes.bool.isRequired,
  hasMonitoredEpisodes: PropTypes.bool.isRequired,
  hasEpisodeFiles: PropTypes.bool.isRequired,
  previousSeries: PropTypes.object.isRequired,
  nextSeries: PropTypes.object.isRequired,
  onMonitorTogglePress: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

SeriesDetails.defaultProps = {
  statistics: {},
  tags: [],
  isSaving: false
};

export default SeriesDetails;
