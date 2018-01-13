import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { scrollDirections } from 'Helpers/Props';
import Scroller from 'Components/Scroller/Scroller';
import styles from './ModalBody.css';

class ModalBody extends Component {

  //
  // Render

  render() {
    const {
      innerClassName,
      scrollDirection,
      children,
      ...otherProps
    } = this.props;

    let className = this.props.className;
    const hasScroller = scrollDirection !== scrollDirections.NONE;

    if (!className) {
      className = hasScroller ? styles.modalScroller : styles.modalBody;
    }

    return (
      <Scroller
        className={className}
        scrollDirection={scrollDirection}
        scrollTop={0}
        {...otherProps}
      >
        {
          hasScroller ?
            <div className={innerClassName}>
              {children}
            </div> :
            children
        }
      </Scroller>
    );
  }

}

ModalBody.propTypes = {
  className: PropTypes.string,
  innerClassName: PropTypes.string,
  children: PropTypes.node,
  scrollDirection: PropTypes.oneOf([scrollDirections.NONE, scrollDirections.HORIZONTAL, scrollDirections.VERTICAL])
};

ModalBody.defaultProps = {
  innerClassName: styles.innerModalBody,
  scrollDirection: scrollDirections.VERTICAL
};

export default ModalBody;
