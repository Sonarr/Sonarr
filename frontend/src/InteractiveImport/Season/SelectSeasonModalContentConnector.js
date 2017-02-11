import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import SelectSeasonModalContent from './SelectSeasonModalContent';

function createMapStateToProps() {
  return createSelector(
    createSeriesSelector(),
    (series) => {
      return {
        items: series.seasons
      };
    }
  );
}

const mapDispatchToProps = {
  updateInteractiveImportItem
};

class SelectSeasonModalContentConnector extends Component {

  //
  // Listeners

  onSeasonSelect = (seasonNumber) => {
    this.props.ids.forEach((id) => {
      this.props.updateInteractiveImportItem({
        id,
        seasonNumber,
        episodes: []
      });
    });

    this.props.onModalClose(true);
  }

  //
  // Render

  render() {
    return (
      <SelectSeasonModalContent
        {...this.props}
        onSeasonSelect={this.onSeasonSelect}
      />
    );
  }
}

SelectSeasonModalContentConnector.propTypes = {
  ids: PropTypes.arrayOf(PropTypes.number).isRequired,
  seriesId: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  updateInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectSeasonModalContentConnector);
