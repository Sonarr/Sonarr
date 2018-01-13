import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes, kinds } from 'Helpers/Props';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import Button from 'Components/Link/Button';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
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
  }

  onDeleteSeriesConfirmed = () => {
    const deleteFiles = this.state.deleteFiles;

    this.setState({ deleteFiles: false });
    this.props.onDeleteSelectedPress(deleteFiles);
  }

  //
  // Render

  render() {
    const {
      series,
      onModalClose
    } = this.props;
    const deleteFiles = this.state.deleteFiles;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Delete Selected Series
        </ModalHeader>

        <ModalBody>
          <div>
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
  onModalClose: PropTypes.func.isRequired,
  onDeleteSelectedPress: PropTypes.func.isRequired
};

export default DeleteSeriesModalContent;
