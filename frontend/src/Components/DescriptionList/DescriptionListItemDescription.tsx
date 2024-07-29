import React, { ReactNode } from 'react';
import styles from './DescriptionListItemDescription.css';

export interface DescriptionListItemDescriptionProps {
  className?: string;
  children?: ReactNode;
}

function DescriptionListItemDescription(
  props: DescriptionListItemDescriptionProps
) {
  const { className = styles.description, children } = props;

  return <dd className={className}>{children}</dd>;
}

export default DescriptionListItemDescription;
