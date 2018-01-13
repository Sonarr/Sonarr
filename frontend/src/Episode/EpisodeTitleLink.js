import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import EpisodeDetailsModal from 'Episode/EpisodeDetailsModal';
import styles from './EpisodeTitleLink.css';

class EpisodeTitleLink extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isDetailsModalOpen: false
    };
  }

  //
  // Listeners

  onLinkPress = () => {
    this.setState({ isDetailsModalOpen: true });
  }

  onModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      episodeTitle,
      ...otherProps
    } = this.props;

    return (
      <div>
        <Link
          className={styles.link}
          onPress={this.onLinkPress}
        >
          {episodeTitle}
        </Link>

        <EpisodeDetailsModal
          isOpen={this.state.isDetailsModalOpen}
          episodeTitle={episodeTitle}
          {...otherProps}
          onModalClose={this.onModalClose}
        />
      </div>
    );
  }
}

EpisodeTitleLink.propTypes = {
  episodeTitle: PropTypes.string.isRequired
};

EpisodeTitleLink.defaultProps = {
  showSeriesButton: false
};

export default EpisodeTitleLink;
