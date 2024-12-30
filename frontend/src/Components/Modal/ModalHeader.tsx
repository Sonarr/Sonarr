import React, { ForwardedRef, forwardRef, ReactNode } from 'react';
import styles from './ModalHeader.css';

interface ModalHeaderProps extends React.HTMLAttributes<HTMLDivElement> {
  children: ReactNode;
}

const ModalHeader = forwardRef(
  (
    { children, ...otherProps }: ModalHeaderProps,
    ref: ForwardedRef<HTMLDivElement>
  ) => {
    return (
      <div ref={ref} className={styles.modalHeader} {...otherProps}>
        {children}
      </div>
    );
  }
);

export default ModalHeader;
