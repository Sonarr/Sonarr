import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { NotificationTemplateAppState } from 'App/State/SettingsAppState';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons } from 'Helpers/Props';
import { fetchNotificationTemplates } from 'Store/Actions/Settings/notificationTemplates';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import translate from 'Utilities/String/translate';
import EditNotificationTemplateModal from './EditNotificationTemplateModal';
import NotificationTemplateItem from './NotificationTemplateItem';
import styles from './NotificationTemplates.css';

function NotificationTemplates() {
  const { items, isFetching, isPopulated, error }: NotificationTemplateAppState =
    useSelector(createClientSideCollectionSelector('settings.notificationTemplates'));

  const dispatch = useDispatch();

  const [
    isAddNotificationTemplateModalOpen,
    setAddNotificationTemplateModalOpen,
    setAddNotificationTemplateModalClosed,
  ] = useModalOpenState(false);

  useEffect(() => {
    dispatch(fetchNotificationTemplates());
  }, [dispatch]);

  return (
    <FieldSet legend={translate('NotificationTemplates')}>
      <PageSectionContent
        errorMessage={translate('NotificationTemplatesLoadError')}
        isFetching={isFetching}
        isPopulated={isPopulated}
        error={error}
      >
        <div className={styles.notificationTemplates}>
          {items.map((item) => {
            return (
              <NotificationTemplateItem
                key={item.id}
                {...item}
              />
            );
          })}

          <Card
            className={styles.addNotificationTemplate}
            onPress={setAddNotificationTemplateModalOpen}
          >
            <div className={styles.center}>
              <Icon name={icons.ADD} size={45} />
            </div>
          </Card>
        </div>

        <EditNotificationTemplateModal
          isOpen={isAddNotificationTemplateModalOpen}
          onModalClose={setAddNotificationTemplateModalClosed}
        />
      </PageSectionContent>
    </FieldSet>
  );
}

export default NotificationTemplates;
