import PropTypes from 'prop-types';
import React, { useCallback, useState } from 'react';
import { useDispatch } from 'react-redux';
import Card from 'Components/Card';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { kinds } from 'Helpers/Props';
import { deleteUser } from 'Store/Actions/settingsActions';
import translate from 'Utilities/String/translate';
import EditAddUserModal from '../AddUsers/EditAddUserModal/EditAddUserModal';
import styles from '../Users.css';

const User = ({ id, username }) => {

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

  const onEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, [setIsEditModalOpen]);

  const onEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, [setIsEditModalOpen]);

  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const dispatch = useDispatch();

  const onConfirmDeleteUser = useCallback((userId) => {
    dispatch(deleteUser({ id: userId }));
  }, [dispatch]);

  const onDeletePress = useCallback(() => {
    setIsEditModalOpen(false);
    setIsDeleteModalOpen(true);
  }, [setIsEditModalOpen, setIsDeleteModalOpen]);

  const onDeleteModalClose = useCallback(() => {
    setIsDeleteModalOpen(false);
  }, [setIsDeleteModalOpen]);

  const onConfirmDelete = useCallback(() => {
    onConfirmDeleteUser(id);
    setIsDeleteModalOpen(false);
  }, [onConfirmDeleteUser, id]);

  return (
    <>
      <Card className={styles.user}
        onPress={onEditPress}
      >
        <div className={styles.label}>
          {username}
        </div>
      </Card>

      <EditAddUserModal
        isOpen={isEditModalOpen}
        onModalClose={onEditModalClose}
        onDeletePress={onDeletePress}
        onConfirmDelete={onConfirmDelete}
        id={id}
      />

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteUser')}
        message={translate('DeleteUserHelperText', { username })}
        confirmLabel={translate('Delete')}
        onConfirm={onConfirmDelete}
        onCancel={onDeleteModalClose}
      />
    </>
  );
};

User.propTypes = {
  id: PropTypes.number.isRequired,
  username: PropTypes.string.isRequired
};

export default User;
