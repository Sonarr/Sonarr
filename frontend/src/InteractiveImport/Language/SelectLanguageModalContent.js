import PropTypes from 'prop-types';
import React from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes } from 'Helpers/Props';

function SelectLanguageModalContent(props) {
  const {
    languageId,
    isFetching,
    isPopulated,
    error,
    items,
    modalTitle,
    onModalClose,
    onLanguageSelect
  } = props;

  const languageOptions = items.map(({ language }) => {
    return {
      key: language.id,
      value: language.name
    };
  });

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {modalTitle} - Select Language
      </ModalHeader>

      <ModalBody>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && !!error &&
            <div>Unable to load languages</div>
        }

        {
          isPopulated && !error &&
            <Form>
              <FormGroup>
                <FormLabel>Language</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="language"
                  value={languageId}
                  values={languageOptions}
                  onChange={onLanguageSelect}
                />
              </FormGroup>
            </Form>
        }
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>
          Cancel
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

SelectLanguageModalContent.propTypes = {
  languageId: PropTypes.number.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  modalTitle: PropTypes.string.isRequired,
  onLanguageSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectLanguageModalContent;
