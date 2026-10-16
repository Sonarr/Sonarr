import classNames from 'classnames';
import React, { ForwardedRef, forwardRef, ReactNode } from 'react';
import { useModalContext } from './ModalContext';
import styles from './ModalHeader.module.css';

interface ModalHeaderProps extends React.HTMLAttributes<HTMLHeadingElement> {
  children: ReactNode;
  subtitle?: ReactNode;
}

const ModalHeader = forwardRef(
  (
    { className, children, subtitle, ...otherProps }: ModalHeaderProps,
    ref: ForwardedRef<HTMLHeadingElement>
  ) => {
    const { headerId } = useModalContext();

    return (
      <h2
        ref={ref}
        id={headerId}
        className={classNames(styles.modalHeader, className)}
        {...otherProps}
      >
        {children}
        {subtitle ? <span className={styles.subtitle}>{subtitle}</span> : null}
      </h2>
    );
  }
);

export default ModalHeader;
