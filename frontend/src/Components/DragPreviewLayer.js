import PropTypes from 'prop-types';
import React from 'react';
import styles from './DragPreviewLayer.css';

function DragPreviewLayer({ children, ...otherProps }) {
  return (
    <div {...otherProps}>
      {children}
    </div>
  );
}

DragPreviewLayer.propTypes = {
  children: PropTypes.node,
  className: PropTypes.string
};

DragPreviewLayer.defaultProps = {
  className: styles.dragLayer
};

export default DragPreviewLayer;
