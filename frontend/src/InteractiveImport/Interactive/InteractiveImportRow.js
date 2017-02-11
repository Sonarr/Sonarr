import PropTypes from 'prop-types';
import React, { Component } from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import Icon from 'Components/Icon';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRowCellButton from 'Components/Table/Cells/TableRowCellButton';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Popover from 'Components/Tooltip/Popover';
import EpisodeQuality from 'Episode/EpisodeQuality';
import EpisodeLanguage from 'Episode/EpisodeLanguage';
import SelectSeriesModal from 'InteractiveImport/Series/SelectSeriesModal';
import SelectSeasonModal from 'InteractiveImport/Season/SelectSeasonModal';
import SelectEpisodeModal from 'InteractiveImport/Episode/SelectEpisodeModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import SelectLanguageModal from 'InteractiveImport/Language/SelectLanguageModal';
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
      isSelectQualityModalOpen: false,
      isSelectLanguageModalOpen: false
    };
  }

  componentDidMount() {
    const {
      id,
      series,
      seasonNumber,
      episodes,
      quality,
      language
    } = this.props;

    if (
      series &&
      seasonNumber != null &&
      episodes.length &&
      quality &&
      language
    ) {
      this.props.onSelectedChange({ id, value: true });
    }
  }

  componentDidUpdate(prevProps) {
    const {
      id,
      series,
      seasonNumber,
      episodes,
      quality,
      language,
      isSelected,
      onValidRowChange
    } = this.props;

    if (
      prevProps.series === series &&
      prevProps.seasonNumber === seasonNumber &&
      !hasDifferentItems(prevProps.episodes, episodes) &&
      prevProps.quality === quality &&
      prevProps.language === language &&
      prevProps.isSelected === isSelected
    ) {
      return;
    }

    const isValid = !!(
      series &&
      seasonNumber != null &&
      episodes.length &&
      quality &&
      language
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
      isSelected
    } = this.props;

    if (!isSelected && value === true) {
      this.props.onSelectedChange({ id, value });
    }
  }

  //
  // Listeners

  onSelectSeriesPress = () => {
    this.setState({ isSelectSeriesModalOpen: true });
  }

  onSelectSeasonPress = () => {
    this.setState({ isSelectSeasonModalOpen: true });
  }

  onSelectEpisodePress = () => {
    this.setState({ isSelectEpisodeModalOpen: true });
  }

  onSelectQualityPress = () => {
    this.setState({ isSelectQualityModalOpen: true });
  }

  onSelectLanguagePress = () => {
    this.setState({ isSelectLanguageModalOpen: true });
  }

  onSelectSeriesModalClose = (changed) => {
    this.setState({ isSelectSeriesModalOpen: false });
    this.selectRowAfterChange(changed);
  }

  onSelectSeasonModalClose = (changed) => {
    this.setState({ isSelectSeasonModalOpen: false });
    this.selectRowAfterChange(changed);
  }

  onSelectEpisodeModalClose = (changed) => {
    this.setState({ isSelectEpisodeModalOpen: false });
    this.selectRowAfterChange(changed);
  }

  onSelectQualityModalClose = (changed) => {
    this.setState({ isSelectQualityModalOpen: false });
    this.selectRowAfterChange(changed);
  }

  onSelectLanguageModalClose = (changed) => {
    this.setState({ isSelectLanguageModalOpen: false });
    this.selectRowAfterChange(changed);
  }

  //
  // Render

  render() {
    const {
      id,
      relativePath,
      series,
      seasonNumber,
      episodes,
      quality,
      language,
      size,
      rejections,
      isSelected,
      onSelectedChange
    } = this.props;

    const {
      isSelectSeriesModalOpen,
      isSelectSeasonModalOpen,
      isSelectEpisodeModalOpen,
      isSelectQualityModalOpen,
      isSelectLanguageModalOpen
    } = this.state;

    const seriesTitle = series ? series.title : '';
    const episodeNumbers = episodes.map((episode) => episode.episodeNumber)
      .join(', ');

    const showSeriesPlaceholder = isSelected && !series;
    const showSeasonNumberPlaceholder = isSelected && !!series && isNaN(seasonNumber);
    const showEpisodeNumbersPlaceholder = isSelected && Number.isInteger(seasonNumber) && !episodes.length;

    return (
      <TableRow>
        <TableSelectCell
          id={id}
          isSelected={isSelected}
          onSelectedChange={onSelectedChange}
        />

        <TableRowCell
          className={styles.relativePath}
          title={relativePath}
        >
          {relativePath}
        </TableRowCell>

        <TableRowCellButton
          onPress={this.onSelectSeriesPress}
        >
          {
            showSeriesPlaceholder ? <InteractiveImportRowCellPlaceholder /> : seriesTitle
          }
        </TableRowCellButton>

        <TableRowCellButton
          isDisabled={!series}
          onPress={this.onSelectSeasonPress}
        >
          {
            showSeasonNumberPlaceholder ? <InteractiveImportRowCellPlaceholder /> : seasonNumber
          }
        </TableRowCellButton>

        <TableRowCellButton
          isDisabled={!series || isNaN(seasonNumber)}
          onPress={this.onSelectEpisodePress}
        >
          {
            showEpisodeNumbersPlaceholder ? <InteractiveImportRowCellPlaceholder /> : episodeNumbers
          }
        </TableRowCellButton>

        <TableRowCellButton
          className={styles.quality}
          onPress={this.onSelectQualityPress}
        >
          <EpisodeQuality
            quality={quality}
          />
        </TableRowCellButton>

        <TableRowCellButton
          className={styles.language}
          onPress={this.onSelectLanguagePress}
        >
          <EpisodeLanguage
            language={language}
          />
        </TableRowCellButton>

        <TableRowCell>
          {formatBytes(size)}
        </TableRowCell>

        <TableRowCell>
          {
            !!rejections.length &&
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
              />
          }
        </TableRowCell>

        <SelectSeriesModal
          isOpen={isSelectSeriesModalOpen}
          ids={[id]}
          onModalClose={this.onSelectSeriesModalClose}
        />

        <SelectSeasonModal
          isOpen={isSelectSeasonModalOpen}
          ids={[id]}
          seriesId={series && series.id}
          onModalClose={this.onSelectSeasonModalClose}
        />

        <SelectEpisodeModal
          isOpen={isSelectEpisodeModalOpen}
          id={id}
          seriesId={series && series.id}
          seasonNumber={seasonNumber}
          onModalClose={this.onSelectEpisodeModalClose}
        />

        <SelectQualityModal
          isOpen={isSelectQualityModalOpen}
          id={id}
          qualityId={quality.quality.id}
          proper={quality.revision.version > 1}
          real={quality.revision.real > 0}
          onModalClose={this.onSelectQualityModalClose}
        />

        <SelectLanguageModal
          isOpen={isSelectLanguageModalOpen}
          id={id}
          languageId={language.id}
          onModalClose={this.onSelectLanguageModalClose}
        />
      </TableRow>
    );
  }

}

InteractiveImportRow.propTypes = {
  id: PropTypes.number.isRequired,
  relativePath: PropTypes.string.isRequired,
  series: PropTypes.object,
  seasonNumber: PropTypes.number,
  episodes: PropTypes.arrayOf(PropTypes.object).isRequired,
  quality: PropTypes.object,
  language: PropTypes.object,
  size: PropTypes.number.isRequired,
  rejections: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired,
  onValidRowChange: PropTypes.func.isRequired
};

InteractiveImportRow.defaultProps = {
  episodes: []
};

export default InteractiveImportRow;
