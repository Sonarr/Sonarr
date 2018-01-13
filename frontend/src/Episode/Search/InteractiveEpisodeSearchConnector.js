import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { createSelector } from 'reselect';
import connectSection from 'Store/connectSection';
import * as releaseActions from 'Store/Actions/releaseActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import InteractiveEpisodeSearch from './InteractiveEpisodeSearch';

function createMapStateToProps() {
  return createSelector(
    (state) => state.releases.items.length,
    createClientSideCollectionSelector(),
    createUISettingsSelector(),
    (totalReleasesCount, releases, uiSettings) => {
      return {
        totalReleasesCount,
        longDateFormat: uiSettings.longDateFormat,
        timeFormat: uiSettings.timeFormat,
        ...releases
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchFetchReleases() {
      dispatch(releaseActions.fetchReleases({ episodeId: props.episodeId }));
    },

    onSortPress(sortKey, sortDirection) {
      dispatch(releaseActions.setReleasesSort({ sortKey, sortDirection }));
    },

    onFilterSelect(selectedFilterKey) {
      dispatch(releaseActions.setReleasesFilter({ selectedFilterKey }));
    },

    onGrabPress(guid, indexerId) {
      dispatch(releaseActions.grabRelease({ guid, indexerId }));
    }
  };
}

class InteractiveEpisodeSearchConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      episodeId,
      isPopulated
    } = this.props;

    // If search results are not yet isPopulated fetch them,
    // otherwise re-show the existing props.

    if (!isPopulated) {
      this.props.dispatchFetchReleases({
        episodeId
      });
    }
  }

  //
  // Render

  render() {
    const {
      dispatchFetchReleases,
      ...otherProps
    } = this.props;

    return (

      <InteractiveEpisodeSearch
        {...otherProps}
      />
    );
  }
}

InteractiveEpisodeSearchConnector.propTypes = {
  episodeId: PropTypes.number.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  dispatchFetchReleases: PropTypes.func.isRequired
};

export default connectSection(
  createMapStateToProps,
  createMapDispatchToProps,
  undefined,
  undefined,
  { section: 'releases' }
)(InteractiveEpisodeSearchConnector);
