import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRowCellButton from 'Components/Table/Cells/TableRowCellButton';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import EpisodeLanguages from 'Episode/EpisodeLanguages';
import EpisodeQuality from 'Episode/EpisodeQuality';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import SelectEpisodeModal from 'InteractiveImport/Episode/SelectEpisodeModal';
import SelectLanguageModal from 'InteractiveImport/Language/SelectLanguageModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import SelectReleaseGroupModal from 'InteractiveImport/ReleaseGroup/SelectReleaseGroupModal';
import SelectSeasonModal from 'InteractiveImport/Season/SelectSeasonModal';
import SelectSeriesModal from 'InteractiveImport/Series/SelectSeriesModal';
import formatBytes from 'Utilities/Number/formatBytes';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import InteractiveImportRowCellPlaceholder from './InteractiveImportRowCellPlaceholder';
import styles from './InteractiveImportRow.css';

class InteractiveImportRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isSelectSeriesModalOpen: false,
      isSelectSeasonModalOpen: false,
      isSelectEpisodeModalOpen: false,
      isSelectReleaseGroupModalOpen: false,
      isSelectQualityModalOpen: false,
      isSelectLanguageModalOpen: false
    };
  }

  componentDidMount() {
    const {
      allowSeriesChange,
      id,
      series,
      seasonNumber,
      episodes,
      quality,
      languages,
      episodeFileId,
      columns
    } = this.props;

    if (
      allowSeriesChange &&
      series &&
      seasonNumber != null &&
      episodes.length &&
      quality &&
      languages
    ) {
      this.props.onSelectedChange({
        id,
        hasEpisodeFileId: !!episodeFileId,
        value: true
      });
    }

    this.setState({
      isSeriesColumnVisible: columns.find((c) => c.name === 'series').isVisible
    });
  }

  componentDidUpdate(prevProps) {
    const {
      id,
      series,
      seasonNumber,
      episodes,
      quality,
      languages,
      isSelected,
      onValidRowChange
    } = this.props;

    if (
      prevProps.series === series &&
      prevProps.seasonNumber === seasonNumber &&
      !hasDifferentItems(prevProps.episodes, episodes) &&
      prevProps.quality === quality &&
      prevProps.languages === languages &&
      prevProps.isSelected === isSelected
    ) {
      return;
    }

    const isValid = !!(
      series &&
      seasonNumber != null &&
      episodes.length &&
      quality &&
      languages
    );

    if (isSelected && !isValid) {
      onValidRowChange(id, false);
    } else {
      onValidRowChange(id, true);
    }
  }

  //
  // Control

  selectRowAfterChange = (value) => {
    const {
      id,
      episodeFileId,
      isSelected
    } = this.props;

    if (!isSelected && value === true) {
      this.props.onSelectedChange({
        id,
        hasEpisodeFileId: !!episodeFileId,
        value
      });
    }
  };

  //
  // Listeners

  onSelectedChange = (result) => {
    const {
      episodeFileId,
      onSelectedChange
    } = this.props;

    onSelectedChange({
      ...result,
      hasEpisodeFileId: !!episodeFileId
    });
  };

  onSelectSeriesPress = () => {
    this.setState({ isSelectSeriesModalOpen: true });
  };

  onSelectSeasonPress = () => {
    this.setState({ isSelectSeasonModalOpen: true });
  };

  onSelectEpisodePress = () => {
    this.setState({ isSelectEpisodeModalOpen: true });
  };

  onSelectReleaseGroupPress = () => {
    this.setState({ isSelectReleaseGroupModalOpen: true });
  };

  onSelectQualityPress = () => {
    this.setState({ isSelectQualityModalOpen: true });
  };

  onSelectLanguagePress = () => {
    this.setState({ isSelectLanguageModalOpen: true });
  };

  onSelectSeriesModalClose = (changed) => {
    this.setState({ isSelectSeriesModalOpen: false });
    this.selectRowAfterChange(changed);
  };

  onSelectSeasonModalClose = (changed) => {
    this.setState({ isSelectSeasonModalOpen: false });
    this.selectRowAfterChange(changed);
  };

  onSelectEpisodeModalClose = (changed) => {
    this.setState({ isSelectEpisodeModalOpen: false });
    this.selectRowAfterChange(changed);
  };

  onSelectReleaseGroupModalClose = (changed) => {
    this.setState({ isSelectReleaseGroupModalOpen: false });
    this.selectRowAfterChange(changed);
  };

  onSelectQualityModalClose = (changed) => {
    this.setState({ isSelectQualityModalOpen: false });
    this.selectRowAfterChange(changed);
  };

  onSelectLanguageModalClose = (changed) => {
    this.setState({ isSelectLanguageModalOpen: false });
    this.selectRowAfterChange(changed);
  };

  //
  // Render

  render() {
    const {
      id,
      allowSeriesChange,
      relativePath,
      series,
      seasonNumber,
      episodes,
      quality,
      languages,
      releaseGroup,
      size,
      rejections,
      isReprocessing,
      isSelected,
      modalTitle
    } = this.props;

    const {
      isSelectSeriesModalOpen,
      isSelectSeasonModalOpen,
      isSelectEpisodeModalOpen,
      isSelectReleaseGroupModalOpen,
      isSelectQualityModalOpen,
      isSelectLanguageModalOpen
    } = this.state;

    const seriesTitle = series ? series.title : '';
    const isAnime = series ? series.seriesType === 'anime' : false;

    const episodeInfo = episodes.map((episode) => {
      return (
        <div key={episode.id}>
          {episode.episodeNumber}

          {
            isAnime && episode.absoluteEpisodeNumber != null ?
              ` (${episode.absoluteEpisodeNumber})` :
              ''
          }

          {` - ${episode.title}`}
        </div>
      );
    });

    const showSeriesPlaceholder = isSelected && !series;
    const showSeasonNumberPlaceholder = isSelected && !!series && isNaN(seasonNumber) && !isReprocessing;
    const showEpisodeNumbersPlaceholder = isSelected && Number.isInteger(seasonNumber) && !episodes.length;
    const showReleaseGroupPlaceholder = isSelected && !releaseGroup;
    const showQualityPlaceholder = isSelected && !quality;
    const showLanguagePlaceholder = isSelected && !languages;

    return (
      <TableRow>
        <TableSelectCell
          id={id}
          isSelected={isSelected}
          onSelectedChange={this.onSelectedChange}
        />

        <TableRowCell
          className={styles.relativePath}
          title={relativePath}
        >
          {relativePath}
        </TableRowCell>

        {
          this.state.isSeriesColumnVisible ?
            <TableRowCellButton
              isDisabled={!allowSeriesChange}
              title={allowSeriesChange ? 'Click to change series' : undefined}
              onPress={this.onSelectSeriesPress}
            >
              {
                showSeriesPlaceholder ? <InteractiveImportRowCellPlaceholder /> : seriesTitle
              }
            </TableRowCellButton> :
            null
        }

        <TableRowCellButton
          isDisabled={!series}
          title={series ? 'Click to change season' : undefined}
          onPress={this.onSelectSeasonPress}
        >
          {
            showSeasonNumberPlaceholder ? <InteractiveImportRowCellPlaceholder /> : seasonNumber
          }

          {
            isReprocessing && seasonNumber == null ?
              <LoadingIndicator className={styles.reprocessing}
                size={20}

              /> : null
          }
        </TableRowCellButton>

        <TableRowCellButton
          isDisabled={!series || isNaN(seasonNumber)}
          title={series && !isNaN(seasonNumber) ? 'Click to change episode' : undefined}
          onPress={this.onSelectEpisodePress}
        >
          {
            showEpisodeNumbersPlaceholder ? <InteractiveImportRowCellPlaceholder /> : episodeInfo
          }
        </TableRowCellButton>

        <TableRowCellButton
          title="Click to change release group"
          onPress={this.onSelectReleaseGroupPress}
        >
          {
            showReleaseGroupPlaceholder ?
              <InteractiveImportRowCellPlaceholder /> :
              releaseGroup
          }
        </TableRowCellButton>

        <TableRowCellButton
          className={styles.quality}
          title="Click to change quality"
          onPress={this.onSelectQualityPress}
        >
          {
            showQualityPlaceholder &&
              <InteractiveImportRowCellPlaceholder />
          }

          {
            !showQualityPlaceholder && !!quality &&
              <EpisodeQuality
                className={styles.label}
                quality={quality}
              />
          }
        </TableRowCellButton>

        <TableRowCellButton
          className={styles.language}
          title="Click to change language"
          onPress={this.onSelectLanguagePress}
        >
          {
            showLanguagePlaceholder &&
              <InteractiveImportRowCellPlaceholder />
          }

          {
            !showLanguagePlaceholder && !!languages &&
              <EpisodeLanguages
                className={styles.label}
                languages={languages}
              />
          }
        </TableRowCellButton>

        <TableRowCell>
          {formatBytes(size)}
        </TableRowCell>

        <TableRowCell>
          {
            rejections && rejections.length ?
              <Popover
                anchor={
                  <Icon
                    name={icons.DANGER}
                    kind={kinds.DANGER}
                  />
                }
                title="Release Rejected"
                body={
                  <ul>
                    {
                      rejections.map((rejection, index) => {
                        return (
                          <li key={index}>
                            {rejection.reason}
                          </li>
                        );
                      })
                    }
                  </ul>
                }
                position={tooltipPositions.LEFT}
              /> :
              null
          }
        </TableRowCell>

        <SelectSeriesModal
          isOpen={isSelectSeriesModalOpen}
          ids={[id]}
          modalTitle={modalTitle}
          onModalClose={this.onSelectSeriesModalClose}
        />

        <SelectSeasonModal
          isOpen={isSelectSeasonModalOpen}
          ids={[id]}
          seriesId={series && series.id}
          modalTitle={modalTitle}
          onModalClose={this.onSelectSeasonModalClose}
        />

        <SelectEpisodeModal
          isOpen={isSelectEpisodeModalOpen}
          ids={[id]}
          seriesId={series && series.id}
          isAnime={isAnime}
          seasonNumber={seasonNumber}
          relativePath={relativePath}
          modalTitle={modalTitle}
          onModalClose={this.onSelectEpisodeModalClose}
        />

        <SelectReleaseGroupModal
          isOpen={isSelectReleaseGroupModalOpen}
          ids={[id]}
          releaseGroup={releaseGroup ?? ''}
          modalTitle={modalTitle}
          onModalClose={this.onSelectReleaseGroupModalClose}
        />

        <SelectQualityModal
          isOpen={isSelectQualityModalOpen}
          ids={[id]}
          qualityId={quality ? quality.quality.id : 0}
          proper={quality ? quality.revision.version > 1 : false}
          real={quality ? quality.revision.real > 0 : false}
          modalTitle={modalTitle}
          onModalClose={this.onSelectQualityModalClose}
        />

        <SelectLanguageModal
          isOpen={isSelectLanguageModalOpen}
          ids={[id]}
          languageIds={languages ? languages.map((l) => l.id) : []}
          modalTitle={modalTitle}
          onModalClose={this.onSelectLanguageModalClose}
        />
      </TableRow>
    );
  }

}

InteractiveImportRow.propTypes = {
  id: PropTypes.number.isRequired,
  allowSeriesChange: PropTypes.bool.isRequired,
  relativePath: PropTypes.string.isRequired,
  series: PropTypes.object,
  seasonNumber: PropTypes.number,
  episodes: PropTypes.arrayOf(PropTypes.object).isRequired,
  releaseGroup: PropTypes.string,
  quality: PropTypes.object,
  languages: PropTypes.arrayOf(PropTypes.object),
  size: PropTypes.number.isRequired,
  rejections: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  episodeFileId: PropTypes.number,
  isReprocessing: PropTypes.bool,
  isSelected: PropTypes.bool,
  modalTitle: PropTypes.string.isRequired,
  onSelectedChange: PropTypes.func.isRequired,
  onValidRowChange: PropTypes.func.isRequired
};

InteractiveImportRow.defaultProps = {
  episodes: []
};

export default InteractiveImportRow;
