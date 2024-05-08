import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons, kinds, sizes } from 'Helpers/Props';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import translate from 'Utilities/String/translate';
import EpisodeAiringConnector from './EpisodeAiringConnector';
import EpisodeFileRow from './EpisodeFileRow';
import styles from './EpisodeSummary.css';

const columns = [
  {
    name: 'path',
    label: () => translate('Path'),
    isSortable: false,
    isVisible: true
  },
  {
    name: 'size',
    label: () => translate('Size'),
    isSortable: false,
    isVisible: true
  },
  {
    name: 'languages',
    label: () => translate('Languages'),
    isSortable: false,
    isVisible: true
  },
  {
    name: 'quality',
    label: () => translate('Quality'),
    isSortable: false,
    isVisible: true
  },
  {
    name: 'customFormats',
    label: () => translate('Formats'),
    isSortable: false,
    isVisible: true
  },
  {
    name: 'customFormatScore',
    label: React.createElement(Icon, {
      name: icons.SCORE,
      title: () => translate('CustomFormatScore')
    }),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'actions',
    label: '',
    isSortable: false,
    isVisible: true
  }
];

class EpisodeSummary extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isRemoveEpisodeFileModalOpen: false
    };
  }

  //
  // Listeners

  onRemoveEpisodeFilePress = () => {
    this.setState({ isRemoveEpisodeFileModalOpen: true });
  };

  onConfirmRemoveEpisodeFile = () => {
    this.props.onDeleteEpisodeFile();
    this.setState({ isRemoveEpisodeFileModalOpen: false });
  };

  onRemoveEpisodeFileModalClose = () => {
    this.setState({ isRemoveEpisodeFileModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      qualityProfileId,
      network,
      overview,
      airDateUtc,
      mediaInfo,
      path,
      size,
      languages,
      quality,
      customFormats,
      customFormatScore,
      qualityCutoffNotMet,
      onDeleteEpisodeFile
    } = this.props;

    const hasOverview = !!overview;

    return (
      <div>
        <div>
          <span className={styles.infoTitle}>{translate('Airs')}</span>

          <EpisodeAiringConnector
            airDateUtc={airDateUtc}
            network={network}
          />
        </div>

        <div>
          <span className={styles.infoTitle}>{translate('QualityProfile')}</span>

          <Label
            kind={kinds.PRIMARY}
            size={sizes.MEDIUM}
          >
            <QualityProfileNameConnector
              qualityProfileId={qualityProfileId}
            />
          </Label>
        </div>

        <div className={styles.overview}>
          {
            hasOverview ?
              overview :
              translate('NoEpisodeOverview')
          }
        </div>

        {
          path ?
            <Table columns={columns}>
              <TableBody>
                <EpisodeFileRow
                  path={path}
                  size={size}
                  languages={languages}
                  quality={quality}
                  qualityCutoffNotMet={qualityCutoffNotMet}
                  customFormats={customFormats}
                  customFormatScore={customFormatScore}
                  mediaInfo={mediaInfo}
                  columns={columns}
                  onDeleteEpisodeFile={onDeleteEpisodeFile}
                />
              </TableBody>
            </Table> :
            null
        }

        <ConfirmModal
          isOpen={this.state.isRemoveEpisodeFileModalOpen}
          kind={kinds.DANGER}
          title={translate('DeleteEpisodeFile')}
          message={translate('DeleteEpisodeFileMessage', { path })}
          confirmLabel={translate('Delete')}
          onConfirm={this.onConfirmRemoveEpisodeFile}
          onCancel={this.onRemoveEpisodeFileModalClose}
        />
      </div>
    );
  }
}

EpisodeSummary.propTypes = {
  episodeFileId: PropTypes.number.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  network: PropTypes.string.isRequired,
  overview: PropTypes.string,
  airDateUtc: PropTypes.string.isRequired,
  mediaInfo: PropTypes.object,
  path: PropTypes.string,
  size: PropTypes.number,
  languages: PropTypes.arrayOf(PropTypes.object),
  quality: PropTypes.object,
  qualityCutoffNotMet: PropTypes.bool,
  customFormats: PropTypes.arrayOf(PropTypes.object),
  customFormatScore: PropTypes.number.isRequired,
  onDeleteEpisodeFile: PropTypes.func.isRequired
};

export default EpisodeSummary;
