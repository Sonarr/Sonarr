import React, { ForwardedRef, forwardRef, ReactNode } from 'react';
import styles from './ModalFooter.css';

interface ModalFooterProps extends React.HTMLAttributes<HTMLDivElement> {
  children: ReactNode;
}

const ModalFooter = forwardRef(
  (
    { children, ...otherProps }: ModalFooterProps,
    ref: ForwardedRef<HTMLDivElement>
  ) => {
    return (
      <div ref={ref} className={styles.modalFooter} {...otherProps}>
        {children}
      </div>
    );
  }
);

export default ModalFooter;
