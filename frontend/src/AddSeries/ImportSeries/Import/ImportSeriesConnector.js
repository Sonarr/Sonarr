import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setImportSeriesValue, importSeries, clearImportSeries } from 'Store/Actions/importSeriesActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { setAddSeriesDefault } from 'Store/Actions/addSeriesActions';
import createRouteMatchShape from 'Helpers/Props/Shapes/createRouteMatchShape';
import ImportSeries from './ImportSeries';

function createMapStateToProps() {
  return createSelector(
    (state, { match }) => match,
    (state) => state.rootFolders,
    (state) => state.addSeries,
    (state) => state.importSeries,
    (state) => state.settings.languageProfiles,
    (match, rootFolders, addSeries, importSeriesState, languageProfiles) => {
      const {
        isFetching: rootFoldersFetching,
        isPopulated: rootFoldersPopulated,
        error: rootFoldersError,
        items
      } = rootFolders;

      const rootFolderId = parseInt(match.params.rootFolderId);

      const result = {
        rootFolderId,
        rootFoldersFetching,
        rootFoldersPopulated,
        rootFoldersError,
        showLanguageProfile: languageProfiles.items.length > 1
      };

      if (items.length) {
        const rootFolder = _.find(items, { id: rootFolderId });

        return {
          ...result,
          ...rootFolder,
          items: importSeriesState.items
        };
      }

      return result;
    }
  );
}

const mapDispatchToProps = {
  setImportSeriesValue,
  importSeries,
  clearImportSeries,
  fetchRootFolders,
  setAddSeriesDefault
};

class ImportSeriesConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    if (!this.props.rootFoldersPopulated) {
      this.props.fetchRootFolders();
    }
  }

  componentWillUnmount() {
    this.props.clearImportSeries();
  }

  //
  // Listeners

  onInputChange = (ids, name, value) => {
    this.props.setAddSeriesDefault({ [name]: value });

    ids.forEach((id) => {
      this.props.setImportSeriesValue({
        id,
        [name]: value
      });
    });
  }

  onImportPress = (ids) => {
    this.props.importSeries({ ids });
  }

  //
  // Render

  render() {
    return (
      <ImportSeries
        {...this.props}
        onInputChange={this.onInputChange}
        onImportPress={this.onImportPress}
      />
    );
  }
}

const routeMatchShape = createRouteMatchShape({
  rootFolderId: PropTypes.string.isRequired
});

ImportSeriesConnector.propTypes = {
  match: routeMatchShape.isRequired,
  rootFoldersPopulated: PropTypes.bool.isRequired,
  setImportSeriesValue: PropTypes.func.isRequired,
  importSeries: PropTypes.func.isRequired,
  clearImportSeries: PropTypes.func.isRequired,
  fetchRootFolders: PropTypes.func.isRequired,
  setAddSeriesDefault: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportSeriesConnector);
