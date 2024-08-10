import PropTypes from 'prop-types';
import React, { useCallback, useEffect, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import { saveUser, setUserValue } from 'Store/Actions/Settings/users';
import { createProviderSettingsSelectorHook } from 'Store/Selectors/createProviderSettingsSelector';
import translate from 'Utilities/String/translate';

export default function EditAddUserModalContent(props) {
  const {
    id,
    onModalClose
  } = props;

  const {
    error,
    item,
    isFetching,
    isSaving,
    saveError,
    validationErrors,
    validationWarnings
  } = useSelector(createProviderSettingsSelectorHook('users', id));

  const dispatch = useDispatch();
  const onInputChange = useCallback(({ name, value }) => {
    dispatch(setUserValue({ name, value }));
  }, [dispatch]);

  const onSavePress = useCallback(() => {
    dispatch(saveUser({ id }));
  }, [dispatch, id]);

  const isSavingRef = useRef();

  useEffect(() => {
    if (isSavingRef.current && !isSaving && !saveError) {
      onModalClose();
    }

    isSavingRef.current = isSaving;
  }, [isSaving, saveError, onModalClose]);

  const {
    username,
    password
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? translate('EditAutoTag') : translate('AddUser')}
      </ModalHeader>

      <ModalBody>
        <div>
          {
            isFetching ? <LoadingIndicator />: null
          }

          {
            !isFetching && !!error ?
              <Alert kind={kinds.DANGER}>
                {translate('AddAutoTagError')}
              </Alert> :
              null
          }

          {
            !isFetching && !error ?
              <div>
                <Form
                  validationErrors={validationErrors}
                  validationWarnings={validationWarnings}
                >
                  <FormGroup>
                    <FormLabel>
                      {translate('Username')}
                    </FormLabel>

                    <FormInputGroup
                      type={inputTypes.TEXT}
                      name="username"
                      {...username}
                      onChange={onInputChange}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>{translate('Password')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.PASSWORD}
                      name="password"
                      {...password}
                      onChange={onInputChange}
                    />
                  </FormGroup>
                </Form>

              </div> :
              null
          }
        </div>
      </ModalBody>
      <ModalFooter>

        <Button
          onPress={onModalClose}
        >
          {translate('Cancel')}
        </Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={onSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

EditAddUserModalContent.propTypes = {
  id: PropTypes.number,
  onModalClose: PropTypes.func.isRequired
};
