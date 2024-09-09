import { initializeConfig } from 'intializeConfig';
import PropTypes from 'prop-types';
import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import ClipboardButton from 'Components/Link/ClipboardButton';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import { saveUser, setUserValue } from 'Store/Actions/Settings/users';
import { createProviderSettingsSelectorHook } from 'Store/Selectors/createProviderSettingsSelector';
import translate from 'Utilities/String/translate';
import styles from './EditAddUserModalContent.css';

export default function EditAddUserModalContent(props) {
  const {
    id,
    onModalClose,
    onDeletePress
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

  const isEditMode = Boolean(id);

  const [resetApiKey, setResetApikey] = useState(false);

  const onResetApiKeyPress = useCallback(() => {
    dispatch(setUserValue({ name: 'ResetApiKey', value: !resetApiKey }));
    setResetApikey(!resetApiKey);
  }, [dispatch, resetApiKey]);

  const onInputChange = useCallback(({ name, value }) => {
    dispatch(setUserValue({ name, value }));
  }, [dispatch]);

  const onSavePress = useCallback(() => {
    dispatch(saveUser({ id }));
  }, [dispatch, id]);

  const isSavingRef = useRef();

  // Todo: Make a seamless tranistion for a user resetting their own Apikey.
  useEffect(() => {
    if (isSavingRef.current && !isSaving && !saveError) {
      onModalClose();
      if (resetApiKey) {
        initializeConfig();
      }
    }

    isSavingRef.current = isSaving;
  }, [isSaving, saveError, onModalClose, resetApiKey]);

  const roleOptions = [
    {
      key: 'admin',
      get value() {
        return translate('Admin');
      }
    },
    {
      key: 'readOnly',
      get value() {
        return translate('ReadOnly');
      }
    }
  ];

  const {
    username,
    password,
    passwordConfirmation,
    role = { value: 'Admin' },
    apiKey
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? translate('EditUser') : translate('AddUser')}
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

                  <FormGroup>
                    <FormLabel>{translate('PasswordConfirmation')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.PASSWORD}
                      name="passwordConfirmation"
                      {...passwordConfirmation}
                      onChange={onInputChange}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>{translate('Role')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="role"
                      values={roleOptions}
                      onChange={onInputChange}
                      {...role}
                    />
                  </FormGroup>

                  {isEditMode && (
                    <FormGroup>
                      <FormLabel>{translate('ApiKey')}</FormLabel>

                      <FormInputGroup
                        type={inputTypes.TEXT}
                        name="apiKey"
                        readOnly={true}
                        helpTextWarning={resetApiKey ? translate('ApiRefreshText'): null}
                        buttons={[
                          <ClipboardButton
                            key="copy"
                            value={apiKey.value}
                            kind={kinds.DEFAULT}
                          />,
                          <FormInputButton
                            key="reset"
                            onPress={onResetApiKeyPress}
                          >
                            <Icon
                              name={resetApiKey ? icons.CLOSE : icons.REFRESH}
                            />
                          </FormInputButton>
                        ]}
                        onChange={onInputChange}
                        {...apiKey}
                      />
                    </FormGroup>
                  )}

                </Form>
              </div> :
              null
          }
        </div>
      </ModalBody>
      <ModalFooter>
        {id && (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeletePress}
          >
            {translate('Delete')}
          </Button>
        )}

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
  onModalClose: PropTypes.func.isRequired,
  onDeletePress: PropTypes.func,
  userId: PropTypes.number
};
