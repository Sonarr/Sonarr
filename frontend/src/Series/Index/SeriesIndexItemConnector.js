import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { isCommandExecuting } from 'Utilities/Command';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createQualityProfileSelector from 'Store/Selectors/createQualityProfileSelector';
import createLanguageProfileSelector from 'Store/Selectors/createLanguageProfileSelector';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';

function selectShowSearchAction() {
  return createSelector(
    (state) => state.seriesIndex,
    (seriesIndex) => {
      const view = seriesIndex.view;

      switch (view) {
        case 'posters':
          return seriesIndex.posterOptions.showSearchAction;
        case 'overview':
          return seriesIndex.overviewOptions.showSearchAction;
        default:
          return seriesIndex.tableOptions.showSearchAction;
      }
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    createSeriesSelector(),
    createQualityProfileSelector(),
    createLanguageProfileSelector(),
    selectShowSearchAction(),
    createCommandsSelector(),
    (
      series,
      qualityProfile,
      languageProfile,
      showSearchAction,
      commands
    ) => {
      const isRefreshingSeries = commands.some((command) => {
        return (
          command.name === commandNames.REFRESH_SERIES &&
          command.body.seriesId === series.id &&
          isCommandExecuting(command)
        );
      });

      const isSearchingSeries = commands.some((command) => {
        return (
          command.name === commandNames.SERIES_SEARCH &&
          command.body.seriesId === series.id &&
          isCommandExecuting(command)
        );
      });

      const latestSeason = _.maxBy(series.seasons, (season) => season.seasonNumber);

      return {
        ...series,
        qualityProfile,
        languageProfile,
        latestSeason,
        showSearchAction,
        isRefreshingSeries,
        isSearchingSeries
      };
    }
  );
}

const mapDispatchToProps = {
  executeCommand
};

class SeriesIndexItemConnector extends Component {

  //
  // Listeners

  onRefreshSeriesPress = () => {
    this.props.executeCommand({
      name: commandNames.REFRESH_SERIES,
      seriesId: this.props.id
    });
  }

  onSearchPress = () => {
    this.props.executeCommand({
      name: commandNames.SERIES_SEARCH,
      seriesId: this.props.id
    });
  }

  //
  // Render

  render() {
    const {
      component: ItemComponent,
      ...otherProps
    } = this.props;

    return (
      <ItemComponent
        {...otherProps}
        onRefreshSeriesPress={this.onRefreshSeriesPress}
        onSearchPress={this.onSearchPress}
      />
    );
  }
}

SeriesIndexItemConnector.propTypes = {
  id: PropTypes.number.isRequired,
  component: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SeriesIndexItemConnector);
