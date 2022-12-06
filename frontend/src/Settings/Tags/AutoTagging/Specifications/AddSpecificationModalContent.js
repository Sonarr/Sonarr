import PropTypes from 'prop-types';
import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Alert from 'Components/Alert';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import {
  fetchAutoTaggingSpecificationSchema,
  selectAutoTaggingSpecificationSchema
} from 'Store/Actions/settingsActions';
import AddSpecificationItem from './AddSpecificationItem';
import styles from './AddSpecificationModalContent.css';

export default function AddSpecificationModalContent(props) {
  const {
    onModalClose
  } = props;

  const {
    isSchemaFetching,
    isSchemaPopulated,
    schemaError,
    schema
  } = useSelector(
    (state) => state.settings.autoTaggingSpecifications
  );

  const dispatch = useDispatch();

  const onSpecificationSelect = useCallback(({ implementation, name }) => {
    dispatch(selectAutoTaggingSpecificationSchema({ implementation, presetName: name }));
    onModalClose({ specificationSelected: true });
  }, [dispatch, onModalClose]);

  useEffect(() => {
    dispatch(fetchAutoTaggingSpecificationSchema());
  }, [dispatch]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Add Condition
      </ModalHeader>

      <ModalBody>
        {
          isSchemaFetching ? <LoadingIndicator /> : null
        }

        {
          !isSchemaFetching && !!schemaError ?
            <div>
              {'Unable to add a new condition, please try again.'}
            </div> :
            null
        }

        {
          isSchemaPopulated && !schemaError ?
            <div>

              <Alert kind={kinds.INFO}>
                <div>
                  {'Sonarr supports the follow properties for auto tagging rules'}
                </div>
              </Alert>

              <div className={styles.specifications}>
                {
                  schema.map((specification) => {
                    return (
                      <AddSpecificationItem
                        key={specification.implementation}
                        {...specification}
                        onSpecificationSelect={onSpecificationSelect}
                      />
                    );
                  })
                }
              </div>

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

AddSpecificationModalContent.propTypes = {
  onModalClose: PropTypes.func.isRequired
};
