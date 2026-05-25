import React from 'react';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import ManageCustomFormatsModal from './ManageCustomFormatsModal';

interface ManageCustomFormatsToolbarButtonProps {
  isOpen?: boolean;
  onPress?: () => void;
  onModalClose?: () => void;
}

const noop = () => undefined;

function ManageCustomFormatsToolbarButton({
  isOpen,
  onPress,
  onModalClose,
}: ManageCustomFormatsToolbarButtonProps) {
  const [internalOpen, openInternal, closeInternal] = useModalOpenState(false);

  const isControlled = isOpen !== undefined;
  const effectiveOpen = isControlled ? isOpen : internalOpen;
  const handlePress = isControlled ? onPress ?? noop : openInternal;
  // In controlled mode, falling back to closeInternal would desync from the parent's isOpen.
  const handleClose = isControlled ? onModalClose ?? noop : closeInternal;

  return (
    <>
      <PageToolbarButton
        label={translate('ManageFormats')}
        iconName={icons.MANAGE}
        onPress={handlePress}
      />

      <ManageCustomFormatsModal
        isOpen={effectiveOpen}
        onModalClose={handleClose}
      />
    </>
  );
}

export default ManageCustomFormatsToolbarButton;
