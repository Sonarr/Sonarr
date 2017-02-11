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
      children,
      onPress
    } = this.props;

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
  children: PropTypes.node.isRequired,
  onPress: PropTypes.func.isRequired
};

Card.defaultProps = {
  className: styles.card
};

export default Card;
