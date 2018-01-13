import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import styles from './PageJumpBarItem.css';

class PageJumpBarItem extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      label,
      onItemPress
    } = this.props;

    onItemPress(label);
  }

  //
  // Render

  render() {
    return (
      <Link
        className={styles.jumpBarItem}
        onPress={this.onPress}
      >
        {this.props.label.toUpperCase()}
      </Link>
    );
  }
}

PageJumpBarItem.propTypes = {
  label: PropTypes.string.isRequired,
  onItemPress: PropTypes.func.isRequired
};

export default PageJumpBarItem;
