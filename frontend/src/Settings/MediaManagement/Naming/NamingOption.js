import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import { sizes } from 'Helpers/Props';
import Link from 'Components/Link/Link';
import styles from './NamingOption.css';

class NamingOption extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      token,
      tokenCase,
      isFullFilename,
      onPress
    } = this.props;

    let tokenValue = token;

    if (tokenCase === 'lower') {
      tokenValue = token.toLowerCase();
    } else if (tokenCase === 'upper') {
      tokenValue = token.toUpperCase();
    }

    onPress({ isFullFilename, tokenValue });
  }

  //
  // Render
  render() {
    const {
      token,
      example,
      tokenCase,
      isFullFilename,
      size
    } = this.props;

    return (
      <Link
        className={classNames(
          styles.option,
          styles[size],
          styles[tokenCase],
          isFullFilename && styles.isFullFilename
        )}
        onPress={this.onPress}
      >
        <div className={styles.token}>{token}</div>
        <div className={styles.example}>{example}</div>
      </Link>
    );
  }
}

NamingOption.propTypes = {
  token: PropTypes.string.isRequired,
  example: PropTypes.string.isRequired,
  tokenCase: PropTypes.string.isRequired,
  isFullFilename: PropTypes.bool.isRequired,
  size: PropTypes.oneOf([sizes.SMALL, sizes.LARGE]),
  onPress: PropTypes.func.isRequired
};

NamingOption.defaultProps = {
  size: sizes.SMALL,
  isFullFilename: false
};

export default NamingOption;
