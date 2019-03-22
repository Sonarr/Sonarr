import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import { setSeasonPassSort, setSeasonPassFilter, saveSeasonPass } from 'Store/Actions/seasonPassActions';
import SeasonPass from './SeasonPass';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector('series', 'seasonPass'),
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

  onFilterSelect = (selectedFilterKey) => {
    this.props.setSeasonPassFilter({ selectedFilterKey });
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

export default connect(createMapStateToProps, mapDispatchToProps)(SeasonPassConnector);
