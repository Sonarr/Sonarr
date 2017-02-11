import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { createSelector } from 'reselect';
import connectSection from 'Store/connectSection';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import { setSeasonPassSort, setSeasonPassFilter, saveSeasonPass } from 'Store/Actions/seasonPassActions';
import SeasonPass from './SeasonPass';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector(),
    (series) => {
      return {
        ...series
      };
    }
  );
}

const mapDispatchToProps = {
  setSeasonPassSort,
  setSeasonPassFilter,
  saveSeasonPass
};

class SeasonPassConnector extends Component {

  //
  // Listeners

  onSortPress = (sortKey) => {
    this.props.setSeasonPassSort({ sortKey });
  }

  onFilterSelect = (filterKey, filterValue, filterType) => {
    this.props.setSeasonPassFilter({ filterKey, filterValue, filterType });
  }

  onUpdateSelectedPress = (payload) => {
    this.props.saveSeasonPass(payload);
  }

  //
  // Render

  render() {
    return (
      <SeasonPass
        {...this.props}
        onSortPress={this.onSortPress}
        onFilterSelect={this.onFilterSelect}
        onUpdateSelectedPress={this.onUpdateSelectedPress}
      />
    );
  }
}

SeasonPassConnector.propTypes = {
  setSeasonPassSort: PropTypes.func.isRequired,
  setSeasonPassFilter: PropTypes.func.isRequired,
  saveSeasonPass: PropTypes.func.isRequired
};

export default connectSection(
  createMapStateToProps,
  mapDispatchToProps,
  undefined,
  undefined,
  { section: 'series', uiSection: 'seasonPass' }
)(SeasonPassConnector);
