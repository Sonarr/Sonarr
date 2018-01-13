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
    (state) => state.settings.qualityProfiles,
    (state) => state.settings.languageProfiles,
    (
      match,
      rootFolders,
      addSeries,
      importSeriesState,
      qualityProfiles,
      languageProfiles
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
        languageProfiles: languageProfiles.items,
        showLanguageProfile: languageProfiles.items.length > 1,
        defaultQualityProfileId: addSeries.defaults.qualityProfileId,
        defaultLanguageProfileId: addSeries.defaults.languageProfileId
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
      qualityProfiles,
      languageProfiles,
      defaultQualityProfileId,
      defaultLanguageProfileId,
      dispatchFetchRootFolders,
      dispatchSetAddSeriesDefault
    } = this.props;

    if (!this.props.rootFoldersPopulated) {
      dispatchFetchRootFolders();
    }

    let setDefaults = false;
    const setDefaultPayload = {};

    if (
      !defaultQualityProfileId ||
      !qualityProfiles.some((p) => p.id === defaultQualityProfileId)
    ) {
      setDefaults = true;
      setDefaultPayload.qualityProfileId = qualityProfiles[0].id;
    }

    if (
      !defaultLanguageProfileId ||
      !languageProfiles.some((p) => p.id === defaultLanguageProfileId)
    ) {
      setDefaults = true;
      setDefaultPayload.languageProfileId = languageProfiles[0].id;
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
  }

  onImportPress = (ids) => {
    this.props.dispatchImportSeries({ ids });
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
  qualityProfiles: PropTypes.arrayOf(PropTypes.object).isRequired,
  languageProfiles: PropTypes.arrayOf(PropTypes.object).isRequired,
  defaultQualityProfileId: PropTypes.number.isRequired,
  defaultLanguageProfileId: PropTypes.number.isRequired,
  dispatchSetImportSeriesValue: PropTypes.func.isRequired,
  dispatchImportSeries: PropTypes.func.isRequired,
  dispatchClearImportSeries: PropTypes.func.isRequired,
  dispatchFetchRootFolders: PropTypes.func.isRequired,
  dispatchSetAddSeriesDefault: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportSeriesConnector);
