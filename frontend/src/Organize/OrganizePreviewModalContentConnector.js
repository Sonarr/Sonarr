import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import { fetchOrganizePreview } from 'Store/Actions/organizePreviewActions';
import { fetchNamingSettings } from 'Store/Actions/settingsActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import OrganizePreviewModalContent from './OrganizePreviewModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.organizePreview,
    (state) => state.settings.naming,
    createSeriesSelector(),
    (organizePreview, naming, series) => {
      const props = { ...organizePreview };
      props.isFetching = organizePreview.isFetching || naming.isFetching;
      props.isPopulated = organizePreview.isPopulated && naming.isPopulated;
      props.error = organizePreview.error || naming.error;
      props.renameEpisodes = naming.item.renameEpisodes;
      props.episodeFormat = naming.item[`${series.seriesType}EpisodeFormat`];
      props.path = series.path;

      return props;
    }
  );
}

const mapDispatchToProps = {
  fetchOrganizePreview,
  fetchNamingSettings,
  executeCommand
};

class OrganizePreviewModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      seriesId,
      seasonNumber
    } = this.props;

    this.props.fetchOrganizePreview({
      seriesId,
      seasonNumber
    });

    this.props.fetchNamingSettings();
  }

  //
  // Listeners

  onOrganizePress = (files) => {
    this.props.executeCommand({
      name: commandNames.RENAME_FILES,
      seriesId: this.props.seriesId,
      files
    });

    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <OrganizePreviewModalContent
        {...this.props}
        onOrganizePress={this.onOrganizePress}
      />
    );
  }
}

OrganizePreviewModalContentConnector.propTypes = {
  seriesId: PropTypes.number.isRequired,
  seasonNumber: PropTypes.number,
  fetchOrganizePreview: PropTypes.func.isRequired,
  fetchNamingSettings: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(OrganizePreviewModalContentConnector);
