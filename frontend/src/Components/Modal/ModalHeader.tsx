import React, { ForwardedRef, forwardRef, ReactNode } from 'react';
import { useModalContext } from './ModalContext';
import styles from './ModalHeader.css';

interface ModalHeaderProps extends React.HTMLAttributes<HTMLDivElement> {
  children: ReactNode;
}

const ModalHeader = forwardRef(
  (
    { children, ...otherProps }: ModalHeaderProps,
    ref: ForwardedRef<HTMLDivElement>
  ) => {
    const { headerId } = useModalContext();

    return (
      <div
        ref={ref}
        id={headerId}
        className={styles.modalHeader}
        {...otherProps}
      >
        {children}
      </div>
    );
  }
);

export default ModalHeader;
