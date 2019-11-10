import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes, kinds } from 'Helpers/Props';
import { stringSettingShape, numberSettingShape } from 'Helpers/Props/Shapes/settingShape';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import styles from './EditImportListExclusionModalContent.css';

function EditImportListExclusionModalContent(props) {
  const {
    id,
    isFetching,
    error,
    isSaving,
    saveError,
    item,
    onInputChange,
    onSavePress,
    onModalClose,
    onDeleteImportListExclusionPress,
    ...otherProps
  } = props;

  const {
    title,
    tvdbId
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? 'Edit Import List Exclusion' : 'Add Import List Exclusion'}
      </ModalHeader>

      <ModalBody className={styles.body}>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && !!error &&
            <div>Unable to add a new import list exclusion, please try again.</div>
        }

        {
          !isFetching && !error &&
            <Form
              {...otherProps}
            >
              <FormGroup>
                <FormLabel>Title</FormLabel>

                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="title"
                  helpText="The name of the series to exclude"
                  {...title}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>TVDB ID</FormLabel>

                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="tvdbId"
                  helpText="The TVDB ID of the series to exclude"
                  {...tvdbId}
                  onChange={onInputChange}
                />
              </FormGroup>
            </Form>
        }
      </ModalBody>

      <ModalFooter>
        {
          id &&
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteImportListExclusionPress}
            >
              Delete
            </Button>
        }

        <Button
          onPress={onModalClose}
        >
          Cancel
        </Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={onSavePress}
        >
          Save
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

const ImportListExclusionShape = {
  title: PropTypes.shape(stringSettingShape).isRequired,
  tvdbId: PropTypes.shape(numberSettingShape).isRequired
};

EditImportListExclusionModalContent.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.shape(ImportListExclusionShape).isRequired,
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteImportListExclusionPress: PropTypes.func
};

export default EditImportListExclusionModalContent;
