import PropTypes from 'prop-types';
import React, { useCallback, useState } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditAddUserModalContent from './EditAddUserModalContent';

export default function EditAddUserModal(props) {
  const {
    isOpen,
    onModalClose: onOriginalModalClose,
    ...otherProps
  } = props;

  const dispatch = useDispatch();
  const [height, setHeight] = useState('auto');

  const onContentHeightChange = useCallback((h) => {
    if (height === 'auto' || h > height) {
      setHeight(h);
    }
  }, [height, setHeight]);

  const onModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section: 'settings.users' }));
    onOriginalModalClose();
  }, [dispatch, onOriginalModalClose]);

  return (
    <Modal
      style={{ height: height === 'auto' ? 'auto': `${height}px` }}
      isOpen={isOpen}
      size={sizes.LARGE}
      onModalClose={onModalClose}
    >
      <EditAddUserModalContent
        {...otherProps}
        onContentHeightChange={onContentHeightChange}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

EditAddUserModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

