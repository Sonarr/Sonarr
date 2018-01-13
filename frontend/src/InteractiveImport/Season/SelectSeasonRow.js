import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import styles from './SelectSeasonRow.css';

class SelectSeasonRow extends Component {

  //
  // Listeners

  onPress = () => {
    this.props.onSeasonSelect(this.props.seasonNumber);
  }

  //
  // Render

  render() {
    const seasonNumber = this.props.seasonNumber;

    return (
      <Link
        className={styles.season}
        component="div"
        onPress={this.onPress}
      >
        {
          seasonNumber === 0 ? 'Specials' : `Season ${seasonNumber}`
        }
      </Link>
    );
  }
}

SelectSeasonRow.propTypes = {
  seasonNumber: PropTypes.number.isRequired,
  onSeasonSelect: PropTypes.func.isRequired
};

export default SelectSeasonRow;
