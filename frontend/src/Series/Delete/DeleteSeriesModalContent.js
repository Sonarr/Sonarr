import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './DeleteSeriesModalContent.css';

class DeleteSeriesModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      deleteFiles: false
    };
  }

  //
  // Listeners

  onDeleteFilesChange = ({ value }) => {
    this.setState({ deleteFiles: value });
  };

  onDeleteSeriesConfirmed = () => {
    const deleteFiles = this.state.deleteFiles;
    const addImportListExclusion = this.props.deleteOptions.addImportListExclusion;

    this.setState({ deleteFiles: false });
    this.props.onDeletePress(deleteFiles, addImportListExclusion);
  };

  //
  // Render

  render() {
    const {
      title,
      path,
      statistics,
      deleteOptions,
      onModalClose,
      onDeleteOptionChange
    } = this.props;

    const {
      episodeFileCount,
      sizeOnDisk
    } = statistics;

    const deleteFiles = this.state.deleteFiles;
    const addImportListExclusion = deleteOptions.addImportListExclusion;

    return (
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          {translate('DeleteSeriesModalHeader', { title })}
        </ModalHeader>

        <ModalBody>
          <div className={styles.pathContainer}>
            <Icon
              className={styles.pathIcon}
              name={icons.FOLDER}
            />

            {path}
          </div>

          <FormGroup>
            <FormLabel>{translate('AddListExclusion')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="addImportListExclusion"
              value={addImportListExclusion}
              helpText={translate('AddListExclusionSeriesHelpText')}
              onChange={onDeleteOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{episodeFileCount === 0 ? translate('DeleteSeriesFolder') : translate('DeleteEpisodesFiles', { episodeFileCount })}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="deleteFiles"
              value={deleteFiles}
              helpText={episodeFileCount === 0 ? translate('DeleteSeriesFolderHelpText') : translate('DeleteEpisodesFilesHelpText')}
              kind={kinds.DANGER}
              onChange={this.onDeleteFilesChange}
            />
          </FormGroup>

          {
            deleteFiles &&
              <div className={styles.deleteFilesMessage}>
                <div><InlineMarkdown data={translate('DeleteSeriesFolderConfirmation', { path })} blockClassName={styles.folderPath} /></div>
                {
                  !!episodeFileCount &&
                    <div>{translate('DeleteSeriesFolderEpisodeCount', { episodeFileCount, size: formatBytes(sizeOnDisk) })}</div>
                }
              </div>
          }

        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            {translate('Close')}
          </Button>

          <Button
            kind={kinds.DANGER}
            onPress={this.onDeleteSeriesConfirmed}
          >
            {translate('Delete')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

DeleteSeriesModalContent.propTypes = {
  title: PropTypes.string.isRequired,
  path: PropTypes.string.isRequired,
  statistics: PropTypes.object.isRequired,
  deleteOptions: PropTypes.object.isRequired,
  onDeleteOptionChange: PropTypes.func.isRequired,
  onDeletePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

DeleteSeriesModalContent.defaultProps = {
  statistics: {
    episodeFileCount: 0
  }
};

export default DeleteSeriesModalContent;
