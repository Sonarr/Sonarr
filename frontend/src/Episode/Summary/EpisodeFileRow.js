import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import EpisodeLanguage from 'Episode/EpisodeLanguage';
import EpisodeQuality from 'Episode/EpisodeQuality';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';
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
      language,
      quality,
      languageCutoffNotMet,
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

            if (name === 'language') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.language}
                >
                  <EpisodeLanguage
                    language={language}
                    isCutoffNotMet={languageCutoffNotMet}
                  />
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
                        title="Media Info"
                        body={<MediaInfo {...mediaInfo} />}
                        position={tooltipPositions.LEFT}
                      /> :
                      null
                  }

                  <IconButton
                    title="Delete episode from disk"
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
          title="Delete Episode File"
          message={`Are you sure you want to delete '${path}'?`}
          confirmLabel="Delete"
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
  language: PropTypes.object.isRequired,
  languageCutoffNotMet: PropTypes.bool.isRequired,
  quality: PropTypes.object.isRequired,
  qualityCutoffNotMet: PropTypes.bool.isRequired,
  mediaInfo: PropTypes.object,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onDeleteEpisodeFile: PropTypes.func.isRequired
};

export default EpisodeFileRow;
