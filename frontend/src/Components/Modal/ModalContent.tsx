import React from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ModalContent.css';

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
        <Link className={styles.closeButton} onPress={onModalClose}>
          <Icon name={icons.CLOSE} size={18} title={translate('Close')} />
        </Link>
      )}

      {children}
    </div>
  );
}

export default ModalContent;
