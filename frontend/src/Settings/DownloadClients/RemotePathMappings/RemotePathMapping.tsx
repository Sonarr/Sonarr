import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import { useDispatch } from 'react-redux';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { icons, kinds } from 'Helpers/Props';
import { deleteRemotePathMapping } from 'Store/Actions/settingsActions';
import translate from 'Utilities/String/translate';
import EditRemotePathMappingModal from './EditRemotePathMappingModal';
import styles from './RemotePathMapping.css';

interface RemotePathMappingProps {
  id: number;
  host: string;
  remotePath: string;
  localPath: string;
}

function RemotePathMapping({
  id,
  host,
  remotePath,
  localPath,
}: RemotePathMappingProps) {
  const dispatch = useDispatch();

  const [
    isEditRemotePathMappingModalOpen,
    setIsEditRemotePathMappingModalOpen,
  ] = useState(false);

  const [
    isDeleteRemotePathMappingModalOpen,
    setIsDeleteRemotePathMappingModalOpen,
  ] = useState(false);

  const handleEditRemotePathMappingPress = useCallback(() => {
    setIsEditRemotePathMappingModalOpen(true);
  }, []);

  const handleEditRemotePathMappingModalClose = useCallback(() => {
    setIsEditRemotePathMappingModalOpen(false);
  }, []);

  const handleDeleteRemotePathMappingPress = useCallback(() => {
    setIsEditRemotePathMappingModalOpen(false);
    setIsDeleteRemotePathMappingModalOpen(true);
  }, []);

  const handleDeleteRemotePathMappingModalClose = useCallback(() => {
    setIsDeleteRemotePathMappingModalOpen(false);
  }, []);

  const handleConfirmDeleteRemotePathMapping = useCallback(() => {
    dispatch(deleteRemotePathMapping({ id }));
  }, [id, dispatch]);

  return (
    <div className={classNames(styles.remotePathMapping)}>
      <div className={styles.host}>{host}</div>
      <div className={styles.path}>{remotePath}</div>
      <div className={styles.path}>{localPath}</div>

      <div className={styles.actions}>
        <Link onPress={handleEditRemotePathMappingPress}>
          <Icon name={icons.EDIT} />
        </Link>
      </div>

      <EditRemotePathMappingModal
        id={id}
        isOpen={isEditRemotePathMappingModalOpen}
        onModalClose={handleEditRemotePathMappingModalClose}
        onDeleteRemotePathMappingPress={handleDeleteRemotePathMappingPress}
      />

      <ConfirmModal
        isOpen={isDeleteRemotePathMappingModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteRemotePathMapping')}
        message={translate('DeleteRemotePathMappingMessageText')}
        confirmLabel={translate('Delete')}
        onConfirm={handleConfirmDeleteRemotePathMapping}
        onCancel={handleDeleteRemotePathMappingModalClose}
      />
    </div>
  );
}

export default RemotePathMapping;
