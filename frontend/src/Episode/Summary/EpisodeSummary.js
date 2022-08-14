import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { kinds, sizes } from 'Helpers/Props';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import EpisodeAiringConnector from './EpisodeAiringConnector';
import EpisodeFileRow from './EpisodeFileRow';
import styles from './EpisodeSummary.css';

const columns = [
  {
    name: 'path',
    label: 'Path',
    isSortable: false,
    isVisible: true
  },
  {
    name: 'size',
    label: 'Size',
    isSortable: false,
    isVisible: true
  },
  {
    name: 'languages',
    label: 'Languages',
    isSortable: false,
    isVisible: true
  },
  {
    name: 'quality',
    label: 'Quality',
    isSortable: false,
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
      languageCutoffNotMet,
      qualityCutoffNotMet,
      onDeleteEpisodeFile
    } = this.props;

    const hasOverview = !!overview;

    return (
      <div>
        <div>
          <span className={styles.infoTitle}>Airs</span>

          <EpisodeAiringConnector
            airDateUtc={airDateUtc}
            network={network}
          />
        </div>

        <div>
          <span className={styles.infoTitle}>Quality Profile</span>

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
              'No episode overview.'
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
                  languageCutoffNotMet={languageCutoffNotMet}
                  quality={quality}
                  qualityCutoffNotMet={qualityCutoffNotMet}
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
          title="Delete Episode File"
          message={`Are you sure you want to delete '${path}'?`}
          confirmLabel="Delete"
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
  languageCutoffNotMet: PropTypes.bool,
  quality: PropTypes.object,
  qualityCutoffNotMet: PropTypes.bool,
  onDeleteEpisodeFile: PropTypes.func.isRequired
};

export default EpisodeSummary;
