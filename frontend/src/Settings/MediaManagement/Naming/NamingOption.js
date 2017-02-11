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
      name,
      value,
      token,
      tokenCase,
      isFullFilename,
      onInputChange
    } = this.props;

    let newValue = token;

    if (tokenCase === 'lower') {
      newValue = token.toLowerCase();
    } else if (tokenCase === 'upper') {
      newValue = token.toUpperCase();
    }

    if (isFullFilename) {
      onInputChange({ name, value: newValue });
    } else {
      onInputChange({
        name,
        value: `${value}${newValue}`
      });
    }
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
  name: PropTypes.string.isRequired,
  value: PropTypes.string.isRequired,
  token: PropTypes.string.isRequired,
  example: PropTypes.string.isRequired,
  tokenCase: PropTypes.string.isRequired,
  isFullFilename: PropTypes.bool.isRequired,
  size: PropTypes.oneOf([sizes.SMALL, sizes.LARGE]),
  onInputChange: PropTypes.func.isRequired
};

NamingOption.defaultProps = {
  size: sizes.SMALL,
  isFullFilename: false
};

export default NamingOption;
