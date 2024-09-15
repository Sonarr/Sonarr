import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import EditAddUserModal from './EditAddUserModal/EditAddUserModal';
import styles from './AddUsers.css';

export default function AddUsers() {
  const {
    error,
    isFetching,
    isPopulated
  } = useSelector(
    createSortedSectionSelector('settings.users', sortByProp('username'))
  );

  const dispatch = useDispatch();
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

  const onEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, [setIsEditModalOpen]);

  const onEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, [setIsEditModalOpen]);

  useEffect(() => {
    dispatch(fetchRootFolders());
  }, [dispatch]);

  return (
    <FieldSet legend={translate('AddUser')}>
      <PageSectionContent
        errorMessage={translate('AddUserLoadError')}
        error={error}
        isFetching={isFetching}
        isPopulated={isPopulated}
      >
        <div className={styles.AddUser}>
          <Card
            className={styles.addAddUser}
            onPress={onEditPress}
          >
            <div className={styles.center}>
              <Icon
                name={icons.ADD}
                size={45}
              />
            </div>
          </Card>
        </div>

        <EditAddUserModal
          isOpen={isEditModalOpen}
          onModalClose={onEditModalClose}
        />

      </PageSectionContent>
    </FieldSet>
  );
}
