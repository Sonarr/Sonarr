import React from 'react';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import ManageCustomFormatsModal from './ManageCustomFormatsModal';

function ManageCustomFormatsToolbarButton() {
  const [isManageModalOpen, openManageModal, closeManageModal] =
    useModalOpenState(false);

  return (
    <>
      <PageToolbarButton
        label={translate('ManageFormats')}
        iconName={icons.MANAGE}
        onPress={openManageModal}
      />

      <ManageCustomFormatsModal
        isOpen={isManageModalOpen}
        onModalClose={closeManageModal}
      />
    </>
  );
}

export default ManageCustomFormatsToolbarButton;
