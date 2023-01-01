import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
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
    this.props.onDeleteSelectedPress(deleteFiles, addImportListExclusion);
  };

  //
  // Render

  render() {
    const {
      series,
      deleteOptions,
      onModalClose,
      setDeleteOption
    } = this.props;

    const {
      deleteFiles
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Delete Selected Series
        </ModalHeader>

        <ModalBody>
          <div>
            <FormGroup>
              <FormLabel>Add List Exclusion</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="addImportListExclusion"
                value={deleteOptions.addImportListExclusion}
                helpText="Prevent series from being added to Sonarr by lists"
                onChange={setDeleteOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{`Delete Series Folder${series.length > 1 ? 's' : ''}`}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="deleteFiles"
                value={deleteFiles}
                helpText={`Delete Series Folder${series.length > 1 ? 's' : ''} and all contents`}
                kind={kinds.DANGER}
                onChange={this.onDeleteFilesChange}
              />
            </FormGroup>
          </div>

          <div className={styles.message}>
            {`Are you sure you want to delete ${series.length} selected series${deleteFiles ? ' and all contents' : ''}?`}
          </div>

          <ul>
            {
              series.map((s) => {
                return (
                  <li key={s.title}>
                    <span>{s.title}</span>

                    {
                      deleteFiles &&
                        <span className={styles.pathContainer}>
                          -
                          <span className={styles.path}>
                            {s.path}
                          </span>
                        </span>
                    }
                  </li>
                );
              })
            }
          </ul>
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            Cancel
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
  series: PropTypes.arrayOf(PropTypes.object).isRequired,
  deleteOptions: PropTypes.arrayOf(PropTypes.object).isRequired,
  onModalClose: PropTypes.func.isRequired,
  setDeleteOption: PropTypes.func.isRequired,
  onDeleteSelectedPress: PropTypes.func.isRequired
};

export default DeleteSeriesModalContent;
