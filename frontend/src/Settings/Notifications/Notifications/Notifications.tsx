import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { NotificationAppState } from 'App/State/SettingsAppState';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import { fetchNotifications } from 'Store/Actions/settingsActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import NotificationModel from 'typings/Notification';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import AddNotificationModal from './AddNotificationModal';
import EditNotificationModal from './EditNotificationModal';
import Notification from './Notification';
import styles from './Notifications.css';

function Notifications() {
  const dispatch = useDispatch();

  const { error, isFetching, isPopulated, items } = useSelector(
    createSortedSectionSelector<NotificationModel, NotificationAppState>(
      'settings.notifications',
      sortByProp('name')
    )
  );

  const [isAddNotificationModalOpen, setIsAddNotificationModalOpen] =
    useState(false);

  const [isEditNotificationModalOpen, setIsEditNotificationModalOpen] =
    useState(false);

  const handleAddNotificationPress = useCallback(() => {
    setIsAddNotificationModalOpen(true);
  }, []);

  const handleNotificationSelect = useCallback(() => {
    setIsAddNotificationModalOpen(false);
    setIsEditNotificationModalOpen(true);
  }, []);

  const handleAddNotificationModalClose = useCallback(() => {
    setIsAddNotificationModalOpen(false);
  }, []);

  const handleEditNotificationModalClose = useCallback(() => {
    setIsEditNotificationModalOpen(false);
  }, []);

  useEffect(() => {
    dispatch(fetchNotifications());
  }, [dispatch]);

  return (
    <FieldSet legend={translate('Connections')}>
      <PageSectionContent
        errorMessage={translate('NotificationsLoadError')}
        error={error}
        isFetching={isFetching}
        isPopulated={isPopulated}
      >
        <div className={styles.notifications}>
          {items.map((item) => (
            <Notification key={item.id} {...item} />
          ))}

          <Card
            className={styles.addNotification}
            onPress={handleAddNotificationPress}
          >
            <div className={styles.center}>
              <Icon name={icons.ADD} size={45} />
            </div>
          </Card>
        </div>

        <AddNotificationModal
          isOpen={isAddNotificationModalOpen}
          onNotificationSelect={handleNotificationSelect}
          onModalClose={handleAddNotificationModalClose}
        />

        <EditNotificationModal
          isOpen={isEditNotificationModalOpen}
          onModalClose={handleEditNotificationModalClose}
        />
      </PageSectionContent>
    </FieldSet>
  );
}

export default Notifications;
