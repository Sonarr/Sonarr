import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './EnhancedSelectInputSelectedValue.css';

interface EnhancedSelectInputSelectedValueProps {
  className?: string;
  children: ReactNode;
  isDisabled?: boolean;
}

function EnhancedSelectInputSelectedValue({
  className = styles.selectedValue,
  children,
  isDisabled = false,
}: EnhancedSelectInputSelectedValueProps) {
  return (
    <div className={classNames(className, isDisabled && styles.isDisabled)}>
      {children}
    </div>
  );
}

export default EnhancedSelectInputSelectedValue;
