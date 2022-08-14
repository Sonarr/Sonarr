import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createRouteMatchShape from 'Helpers/Props/Shapes/createRouteMatchShape';
import { setAddSeriesDefault } from 'Store/Actions/addSeriesActions';
import { clearImportSeries, importSeries, setImportSeriesValue } from 'Store/Actions/importSeriesActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import ImportSeries from './ImportSeries';

function createMapStateToProps() {
  return createSelector(
    (state, { match }) => match,
    (state) => state.rootFolders,
    (state) => state.addSeries,
    (state) => state.importSeries,
    (state) => state.settings.qualityProfiles,
    (
      match,
      rootFolders,
      addSeries,
      importSeriesState,
      qualityProfiles
    ) => {
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
        qualityProfiles: qualityProfiles.items,
        defaultQualityProfileId: addSeries.defaults.qualityProfileId
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
  dispatchSetImportSeriesValue: setImportSeriesValue,
  dispatchImportSeries: importSeries,
  dispatchClearImportSeries: clearImportSeries,
  dispatchFetchRootFolders: fetchRootFolders,
  dispatchSetAddSeriesDefault: setAddSeriesDefault
};

class ImportSeriesConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      rootFolderId,
      qualityProfiles,
      defaultQualityProfileId,
      dispatchFetchRootFolders,
      dispatchSetAddSeriesDefault
    } = this.props;

    dispatchFetchRootFolders({ id: rootFolderId, timeout: false });

    let setDefaults = false;
    const setDefaultPayload = {};

    if (
      !defaultQualityProfileId ||
      !qualityProfiles.some((p) => p.id === defaultQualityProfileId)
    ) {
      setDefaults = true;
      setDefaultPayload.qualityProfileId = qualityProfiles[0].id;
    }

    if (setDefaults) {
      dispatchSetAddSeriesDefault(setDefaultPayload);
    }
  }

  componentWillUnmount() {
    this.props.dispatchClearImportSeries();
  }

  //
  // Listeners

  onInputChange = (ids, name, value) => {
    this.props.dispatchSetAddSeriesDefault({ [name]: value });

    ids.forEach((id) => {
      this.props.dispatchSetImportSeriesValue({
        id,
        [name]: value
      });
    });
  };

  onImportPress = (ids) => {
    this.props.dispatchImportSeries({ ids });
  };

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
  rootFolderId: PropTypes.number.isRequired,
  rootFoldersFetching: PropTypes.bool.isRequired,
  rootFoldersPopulated: PropTypes.bool.isRequired,
  qualityProfiles: PropTypes.arrayOf(PropTypes.object).isRequired,
  defaultQualityProfileId: PropTypes.number.isRequired,
  dispatchSetImportSeriesValue: PropTypes.func.isRequired,
  dispatchImportSeries: PropTypes.func.isRequired,
  dispatchClearImportSeries: PropTypes.func.isRequired,
  dispatchFetchRootFolders: PropTypes.func.isRequired,
  dispatchSetAddSeriesDefault: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportSeriesConnector);
