import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { fetchUsers } from 'Store/Actions/settingsActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import EditAddUserModal from './EditAddUserModal/EditAddUserModal';
import styles from './AddUsers.css';

export default function AddUsers() {
  const {
    error,
    items,
    isDeleting,
    isFetching,
    isPopulated
  } = useSelector(
    createSortedSectionSelector('settings.users', sortByProp('username'))
  );

  // const userList = useSelector(createUserDetailsSelector());
  const dispatch = useDispatch();
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

  // const onClonePress = useCallback((id) => {
  //   dispatch(cloneAutoTagging({ id }));

  //   setTagsFromId(id);
  //   setIsEditModalOpen(true);
  // }, [dispatch, setIsEditModalOpen]);

  const onEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, [setIsEditModalOpen]);

  const onEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, [setIsEditModalOpen]);

  // const onConfirmDelete = useCallback((id) => {
  //   dispatch(deleteAutoTagging({ id }));
  // }, [dispatch]);

  useEffect(() => {
    dispatch(fetchUsers());
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
