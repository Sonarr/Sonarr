import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import ImportSeriesTitle from './ImportSeriesTitle';
import styles from './ImportSeriesSearchResult.css';

class ImportSeriesSearchResult extends Component {

  //
  // Listeners

  onPress = () => {
    this.props.onPress(this.props.tvdbId);
  }

  //
  // Render

  render() {
    const {
      title,
      year,
      network,
      isExistingSeries
    } = this.props;

    return (
      <Link
        className={styles.series}
        onPress={this.onPress}
      >
        <ImportSeriesTitle
          title={title}
          year={year}
          network={network}
          isExistingSeries={isExistingSeries}
        />
      </Link>
    );
  }
}

ImportSeriesSearchResult.propTypes = {
  tvdbId: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  network: PropTypes.string,
  isExistingSeries: PropTypes.bool.isRequired,
  onPress: PropTypes.func.isRequired
};

export default ImportSeriesSearchResult;
