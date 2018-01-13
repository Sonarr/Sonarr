import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import styles from './Card.css';

class Card extends Component {

  //
  // Render

  render() {
    const {
      className,
      overlayClassName,
      overlayContent,
      children,
      onPress
    } = this.props;

    if (overlayContent) {
      return (
        <div className={className}>
          <Link
            className={styles.underlay}
            onPress={onPress}
          />

          <div className={overlayClassName}>
            {children}
          </div>
        </div>
      );
    }

    return (
      <Link
        className={className}
        onPress={onPress}
      >
        {children}
      </Link>
    );
  }
}

Card.propTypes = {
  className: PropTypes.string.isRequired,
  overlayClassName: PropTypes.string.isRequired,
  overlayContent: PropTypes.bool.isRequired,
  children: PropTypes.node.isRequired,
  onPress: PropTypes.func.isRequired
};

Card.defaultProps = {
  className: styles.card,
  overlayClassName: styles.overlay,
  overlayContent: false
};

export default Card;
