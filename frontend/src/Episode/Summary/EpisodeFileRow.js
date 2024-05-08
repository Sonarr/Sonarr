import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import EpisodeFormats from 'Episode/EpisodeFormats';
import EpisodeLanguages from 'Episode/EpisodeLanguages';
import EpisodeQuality from 'Episode/EpisodeQuality';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import translate from 'Utilities/String/translate';
import MediaInfo from './MediaInfo';
import styles from './EpisodeFileRow.css';

class EpisodeFileRow extends Component {

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
      path,
      size,
      languages,
      quality,
      customFormats,
      customFormatScore,
      qualityCutoffNotMet,
      mediaInfo,
      columns
    } = this.props;

    return (
      <TableRow>
        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (name === 'path') {
              return (
                <TableRowCell key={name}>
                  {path}
                </TableRowCell>
              );
            }

            if (name === 'size') {
              return (
                <TableRowCell key={name}>
                  {formatBytes(size)}
                </TableRowCell>
              );
            }

            if (name === 'languages') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.languages}
                >
                  <EpisodeLanguages languages={languages} />
                </TableRowCell>
              );
            }

            if (name === 'quality') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.quality}
                >
                  <EpisodeQuality
                    quality={quality}
                    isCutoffNotMet={qualityCutoffNotMet}
                  />
                </TableRowCell>
              );
            }

            if (name === 'customFormats') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.customFormats}
                >
                  <EpisodeFormats
                    formats={customFormats}
                  />
                </TableRowCell>
              );
            }

            if (name === 'customFormatScore') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.customFormatScore}
                >
                  {formatCustomFormatScore(customFormatScore, customFormats.length)}
                </TableRowCell>
              );
            }

            if (name === 'actions') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.actions}
                >
                  {
                    mediaInfo ?
                      <Popover
                        anchor={
                          <Icon
                            name={icons.MEDIA_INFO}
                          />
                        }
                        title={translate('MediaInfo')}
                        body={<MediaInfo {...mediaInfo} />}
                        position={tooltipPositions.LEFT}
                      /> :
                      null
                  }

                  <IconButton
                    title={translate('DeleteEpisodeFromDisk')}
                    name={icons.REMOVE}
                    onPress={this.onRemoveEpisodeFilePress}
                  />
                </TableRowCell>
              );
            }

            return null;
          })
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
      </TableRow>
    );
  }

}

EpisodeFileRow.propTypes = {
  path: PropTypes.string.isRequired,
  size: PropTypes.number.isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  quality: PropTypes.object.isRequired,
  qualityCutoffNotMet: PropTypes.bool.isRequired,
  customFormats: PropTypes.arrayOf(PropTypes.object),
  customFormatScore: PropTypes.number.isRequired,
  mediaInfo: PropTypes.object,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onDeleteEpisodeFile: PropTypes.func.isRequired
};

export default EpisodeFileRow;
