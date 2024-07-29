import React from 'react';
import styles from './DragPreviewLayer.css';

interface DragPreviewLayerProps {
  className?: string;
  children?: React.ReactNode;
}

function DragPreviewLayer({
  className = styles.dragLayer,
  children,
  ...otherProps
}: DragPreviewLayerProps) {
  return (
    <div className={className} {...otherProps}>
      {children}
    </div>
  );
}

export default DragPreviewLayer;
