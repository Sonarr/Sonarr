import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import { fetchNotificationSchema } from 'Store/Actions/settingsActions';
import translate from 'Utilities/String/translate';
import AddNotificationItem from './AddNotificationItem';
import styles from './AddNotificationModalContent.css';

export interface AddNotificationModalContentProps {
  onNotificationSelect: () => void;
  onModalClose: () => void;
}

function AddNotificationModalContent({
  onNotificationSelect,
  onModalClose,
}: AddNotificationModalContentProps) {
  const dispatch = useDispatch();

  const { isSchemaFetching, isSchemaPopulated, schemaError, schema } =
    useSelector((state: AppState) => state.settings.notifications);

  useEffect(() => {
    dispatch(fetchNotificationSchema());
  }, [dispatch]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('AddNotification')}</ModalHeader>

      <ModalBody>
        {isSchemaFetching ? <LoadingIndicator /> : null}

        {!isSchemaFetching && !!schemaError ? (
          <Alert kind={kinds.DANGER}>{translate('AddNotificationError')}</Alert>
        ) : null}

        {isSchemaPopulated && !schemaError ? (
          <div>
            <Alert kind={kinds.INFO}>
              <div>{translate('SupportedNotifications')}</div>
              <div>{translate('SupportedNotificationsMoreInfo')}</div>
            </Alert>

            <FieldSet legend={translate('Email')}>
              <div className={styles.notifications}>
                {schema.map((notification) => {
                  return (
                    <AddNotificationItem
                      key={notification.implementation}
                      {...notification}
                      implementation={notification.implementation}
                      onNotificationSelect={onNotificationSelect}
                    />
                  );
                })}
              </div>
            </FieldSet>
          </div>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default AddNotificationModalContent;
