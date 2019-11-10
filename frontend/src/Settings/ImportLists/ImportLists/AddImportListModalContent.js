import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds } from 'Helpers/Props';
import Alert from 'Components/Alert';
import Button from 'Components/Link/Button';
import FieldSet from 'Components/FieldSet';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import AddImportListItem from './AddImportListItem';
import styles from './AddImportListModalContent.css';
import titleCase from 'Utilities/String/titleCase';

class AddImportListModalContent extends Component {

  //
  // Render

  render() {
    const {
      isSchemaFetching,
      isSchemaPopulated,
      schemaError,
      listGroups,
      onImportListSelect,
      onModalClose
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Add List
        </ModalHeader>

        <ModalBody>
          {
            isSchemaFetching ?
              <LoadingIndicator /> :
              null
          }

          {
            !isSchemaFetching && !!schemaError ?
              <div>Unable to add a new list, please try again.</div> :
              null
          }

          {
            isSchemaPopulated && !schemaError ?
              <div>

                <Alert kind={kinds.INFO}>
                  <div>Sonarr supports multiple lists for importing Series into the database.</div>
                  <div>For more information on the individual lists, click on the info buttons.</div>
                </Alert>
                {
                  Object.keys(listGroups).map((key) => {
                    return (
                      <FieldSet legend={`${titleCase(key)} List`} key={key}>
                        <div className={styles.lists}>
                          {
                            listGroups[key].map((list) => {
                              return (
                                <AddImportListItem
                                  key={list.implementation}
                                  implementation={list.implementation}
                                  {...list}
                                  onImportListSelect={onImportListSelect}
                                />
                              );
                            })
                          }
                        </div>
                      </FieldSet>
                    );
                  })
                }
              </div> :
              null
          }
        </ModalBody>
        <ModalFooter>
          <Button
            onPress={onModalClose}
          >
            Close
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

AddImportListModalContent.propTypes = {
  isSchemaFetching: PropTypes.bool.isRequired,
  isSchemaPopulated: PropTypes.bool.isRequired,
  schemaError: PropTypes.object,
  listGroups: PropTypes.object.isRequired,
  onImportListSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AddImportListModalContent;
