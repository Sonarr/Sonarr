import React from 'react';
import IconButton from 'Components/Link/IconButton';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ModalContent.module.css';

interface ModalContentProps extends React.HTMLAttributes<HTMLDivElement> {
  className?: string;
  children?: React.ReactNode;
  showCloseButton?: boolean;
  onModalClose: () => void;
}

function ModalContent({
  className = styles.modalContent,
  children,
  showCloseButton = true,
  onModalClose,
  ...otherProps
}: ModalContentProps) {
  return (
    <div className={className} {...otherProps}>
      {showCloseButton && (
        <IconButton
          className={styles.closeButton}
          name={icons.CLOSE}
          size={18}
          title={translate('Close')}
          aria-label={translate('Close')}
          onPress={onModalClose}
        />
      )}

      {children}
    </div>
  );
}

export default ModalContent;
