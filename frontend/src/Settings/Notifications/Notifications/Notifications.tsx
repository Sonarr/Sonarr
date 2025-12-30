import React, { useCallback, useState } from 'react';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import { SelectedSchema } from 'Settings/useProviderSchema';
import translate from 'Utilities/String/translate';
import { useConnections, useSortedConnections } from '../useConnections';
import AddNotificationModal from './AddNotificationModal';
import EditNotificationModal from './EditNotificationModal';
import Notification from './Notification';
import styles from './Notifications.css';

function Notifications() {
  const { error, isFetching, isFetched } = useConnections();
  const items = useSortedConnections();

  const [selectedSchema, setSelectedSchema] = useState<
    SelectedSchema | undefined
  >(undefined);

  const [isAddNotificationModalOpen, setIsAddNotificationModalOpen] =
    useState(false);

  const [isEditNotificationModalOpen, setIsEditNotificationModalOpen] =
    useState(false);

  const handleAddNotificationPress = useCallback(() => {
    setIsAddNotificationModalOpen(true);
  }, []);

  const handleNotificationSelect = useCallback((selected: SelectedSchema) => {
    setSelectedSchema(selected);
    setIsAddNotificationModalOpen(false);
    setIsEditNotificationModalOpen(true);
  }, []);

  const handleAddNotificationModalClose = useCallback(() => {
    setIsAddNotificationModalOpen(false);
  }, []);

  const handleEditNotificationModalClose = useCallback(() => {
    setIsEditNotificationModalOpen(false);
  }, []);

  return (
    <FieldSet legend={translate('Connections')}>
      <PageSectionContent
        errorMessage={translate('NotificationsLoadError')}
        error={error}
        isFetching={isFetching}
        isPopulated={isFetched}
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
          selectedSchema={selectedSchema}
          onModalClose={handleEditNotificationModalClose}
        />
      </PageSectionContent>
    </FieldSet>
  );
}

export default Notifications;
