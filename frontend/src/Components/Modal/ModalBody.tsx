import React from 'react';
import Scroller from 'Components/Scroller/Scroller';
import { ScrollDirection } from 'Helpers/Props/scrollDirections';
import styles from './ModalBody.css';

interface ModalBodyProps {
  className?: string;
  innerClassName?: string;
  children?: React.ReactNode;
  scrollDirection?: ScrollDirection;
}

function ModalBody({
  innerClassName = styles.innerModalBody,
  scrollDirection = 'vertical',
  children,
  ...otherProps
}: ModalBodyProps) {
  let className = otherProps.className;
  const hasScroller = scrollDirection !== 'none';

  if (!className) {
    className = hasScroller ? styles.modalScroller : styles.modalBody;
  }

  return (
    <Scroller
      {...otherProps}
      className={className}
      scrollDirection={scrollDirection}
      scrollTop={0}
    >
      {hasScroller ? (
        <div className={innerClassName}>{children}</div>
      ) : (
        children
      )}
    </Scroller>
  );
}

export default ModalBody;
