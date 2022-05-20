import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';
import styles from './DeleteSeriesModalContent.css';

class DeleteSeriesModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      deleteFiles: false,
      addImportListExclusion: false
    };
  }

  //
  // Listeners

  onDeleteFilesChange = ({ value }) => {
    this.setState({ deleteFiles: value });
  };

  onAddImportListExclusionChange = ({ value }) => {
    this.setState({ addImportListExclusion: value });
  };

  onDeleteSeriesConfirmed = () => {
    const deleteFiles = this.state.deleteFiles;
    const addImportListExclusion = this.state.addImportListExclusion;

    this.setState({ deleteFiles: false, addImportListExclusion: false });
    this.props.onDeletePress(deleteFiles, addImportListExclusion);
  };

  //
  // Render

  render() {
    const {
      title,
      path,
      statistics,
      onModalClose
    } = this.props;

    const {
      episodeFileCount,
      sizeOnDisk
    } = statistics;

    const deleteFiles = this.state.deleteFiles;
    const addImportListExclusion = this.state.addImportListExclusion;
    let deleteFilesLabel = `Delete ${episodeFileCount} Episode Files`;
    let deleteFilesHelpText = 'Delete the episode files and series folder';

    if (episodeFileCount === 0) {
      deleteFilesLabel = 'Delete Series Folder';
      deleteFilesHelpText = 'Delete the series folder and its contents';
    }

    return (
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          Delete - {title}
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
            <FormLabel>Add List Exclusion</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="addImportListExclusion"
              value={addImportListExclusion}
              helpText="Prevent series from being added to Sonarr by lists"
              onChange={this.onAddImportListExclusionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{deleteFilesLabel}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="deleteFiles"
              value={deleteFiles}
              helpText={deleteFilesHelpText}
              kind={kinds.DANGER}
              onChange={this.onDeleteFilesChange}
            />
          </FormGroup>

          {
            deleteFiles &&
              <div className={styles.deleteFilesMessage}>
                <div>The series folder <strong>{path}</strong> and all of its content will be deleted.</div>

                {
                  !!episodeFileCount &&
                    <div>{episodeFileCount} episode files totaling {formatBytes(sizeOnDisk)}</div>
                }
              </div>
          }

        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            Close
          </Button>

          <Button
            kind={kinds.DANGER}
            onPress={this.onDeleteSeriesConfirmed}
          >
            Delete
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
  onDeletePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

DeleteSeriesModalContent.defaultProps = {
  statistics: {
    episodeFileCount: 0
  }
};

export default DeleteSeriesModalContent;
