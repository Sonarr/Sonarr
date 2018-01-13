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
      tokenSeparator,
      tokenCase,
      isFullFilename,
      onPress
    } = this.props;

    let tokenValue = token;

    tokenValue = tokenValue.replace(/ /g, tokenSeparator);

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
      tokenSeparator,
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
        <div className={styles.token}>
          {token.replace(/ /g, tokenSeparator)}
        </div>

        <div className={styles.example}>
          {example.replace(/ /g, tokenSeparator)}
        </div>
      </Link>
    );
  }
}

NamingOption.propTypes = {
  token: PropTypes.string.isRequired,
  example: PropTypes.string.isRequired,
  tokenSeparator: PropTypes.string.isRequired,
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
