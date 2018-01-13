import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import OrganizeSeriesModalContent from './OrganizeSeriesModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { seriesIds }) => seriesIds,
    createAllSeriesSelector(),
    (seriesIds, allSeries) => {
      const series = _.intersectionWith(allSeries, seriesIds, (s, id) => {
        return s.id === id;
      });

      const sortedSeries = _.orderBy(series, 'sortTitle');
      const seriesTitles = _.map(sortedSeries, 'title');

      return {
        seriesTitles
      };
    }
  );
}

const mapDispatchToProps = {
  executeCommand
};

class OrganizeSeriesModalContentConnector extends Component {

  //
  // Listeners

  onOrganizeSeriesPress = () => {
    this.props.executeCommand({
      name: commandNames.RENAME_SERIES,
      seriesIds: this.props.seriesIds
    });

    this.props.onModalClose(true);
  }

  //
  // Render

  render(props) {
    return (
      <OrganizeSeriesModalContent
        {...this.props}
        onOrganizeSeriesPress={this.onOrganizeSeriesPress}
      />
    );
  }
}

OrganizeSeriesModalContentConnector.propTypes = {
  seriesIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  onModalClose: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(OrganizeSeriesModalContentConnector);
