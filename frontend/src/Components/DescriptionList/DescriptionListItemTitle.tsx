import React, { ReactNode } from 'react';
import styles from './DescriptionListItemTitle.css';

export interface DescriptionListItemTitleProps {
  className?: string;
  children?: ReactNode;
}

function DescriptionListItemTitle(props: DescriptionListItemTitleProps) {
  const { className = styles.title, children } = props;

  return <dt className={className}>{children}</dt>;
}

export default DescriptionListItemTitle;
