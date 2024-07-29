import React from 'react';
import styles from './DescriptionList.css';

interface DescriptionListProps {
  className?: string;
  children?: React.ReactNode;
}

function DescriptionList(props: DescriptionListProps) {
  const { className = styles.descriptionList, children } = props;

  return <dl className={className}>{children}</dl>;
}

export default DescriptionList;
