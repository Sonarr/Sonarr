import classNames from 'classnames';
import React, { ForwardedRef, forwardRef, ReactNode } from 'react';
import { useModalContext } from './ModalContext';
import styles from './ModalHeader.css';

interface ModalHeaderProps extends React.HTMLAttributes<HTMLDivElement> {
  children: ReactNode;
}

const ModalHeader = forwardRef(
  (
    { className, children, ...otherProps }: ModalHeaderProps,
    ref: ForwardedRef<HTMLDivElement>
  ) => {
    const { headerId } = useModalContext();

    return (
      <div
        ref={ref}
        id={headerId}
        className={classNames(styles.modalHeader, className)}
        {...otherProps}
      >
        {children}
      </div>
    );
  }
);

export default ModalHeader;
