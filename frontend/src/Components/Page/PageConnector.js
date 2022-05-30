import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { withRouter } from 'react-router-dom';
import { createSelector } from 'reselect';
import { saveDimensions, setIsSidebarVisible } from 'Store/Actions/appActions';
import { fetchCustomFilters } from 'Store/Actions/customFilterActions';
import { fetchSeries } from 'Store/Actions/seriesActions';
import { fetchImportLists, fetchLanguageProfiles, fetchQualityProfiles, fetchUISettings } from 'Store/Actions/settingsActions';
import { fetchStatus } from 'Store/Actions/systemActions';
import { fetchTags } from 'Store/Actions/tagActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import ErrorPage from './ErrorPage';
import LoadingPage from './LoadingPage';
import Page from './Page';

function testLocalStorage() {
  const key = 'sonarrTest';

  try {
    localStorage.setItem(key, key);
    localStorage.removeItem(key);

    return true;
  } catch (e) {
    return false;
  }
}

const selectAppProps = createSelector(
  (state) => state.app.isSidebarVisible,
  (state) => state.app.version,
  (state) => state.app.isUpdated,
  (state) => state.app.isDisconnected,
  (isSidebarVisible, version, isUpdated, isDisconnected) => {
    return {
      isSidebarVisible,
      version,
      isUpdated,
      isDisconnected
    };
  }
);

const selectIsPopulated = createSelector(
  (state) => state.series.isPopulated,
  (state) => state.customFilters.isPopulated,
  (state) => state.tags.isPopulated,
  (state) => state.settings.ui.isPopulated,
  (state) => state.settings.qualityProfiles.isPopulated,
  (state) => state.settings.languageProfiles.isPopulated,
  (state) => state.settings.importLists.isPopulated,
  (state) => state.system.status.isPopulated,
  (
    seriesIsPopulated,
    customFiltersIsPopulated,
    tagsIsPopulated,
    uiSettingsIsPopulated,
    qualityProfilesIsPopulated,
    languageProfilesIsPopulated,
    importListsIsPopulated,
    systemStatusIsPopulated
  ) => {
    return (
      seriesIsPopulated &&
      customFiltersIsPopulated &&
      tagsIsPopulated &&
      uiSettingsIsPopulated &&
      qualityProfilesIsPopulated &&
      languageProfilesIsPopulated &&
      importListsIsPopulated &&
      systemStatusIsPopulated
    );
  }
);

const selectErrors = createSelector(
  (state) => state.series.error,
  (state) => state.customFilters.error,
  (state) => state.tags.error,
  (state) => state.settings.ui.error,
  (state) => state.settings.qualityProfiles.error,
  (state) => state.settings.languageProfiles.error,
  (state) => state.settings.importLists.error,
  (state) => state.system.status.error,
  (
    seriesError,
    customFiltersError,
    tagsError,
    uiSettingsError,
    qualityProfilesError,
    languageProfilesError,
    importListsError,
    systemStatusError
  ) => {
    const hasError = !!(
      seriesError ||
      customFiltersError ||
      tagsError ||
      uiSettingsError ||
      qualityProfilesError ||
      languageProfilesError ||
      importListsError ||
      systemStatusError
    );

    return {
      hasError,
      seriesError,
      customFiltersError,
      tagsError,
      uiSettingsError,
      qualityProfilesError,
      languageProfilesError,
      importListsError,
      systemStatusError
    };
  }
);

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.ui.item.enableColorImpairedMode,
    selectIsPopulated,
    selectErrors,
    selectAppProps,
    createDimensionsSelector(),
    createSystemStatusSelector(),
    (
      enableColorImpairedMode,
      isPopulated,
      errors,
      app,
      dimensions,
      systemStatus
    ) => {
      return {
        ...app,
        ...errors,
        isPopulated,
        isSmallScreen: dimensions.isSmallScreen,
        authenticationEnabled: systemStatus.authentication !== 'none',
        enableColorImpairedMode
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchFetchSeries() {
      dispatch(fetchSeries());
    },
    dispatchFetchCustomFilters() {
      dispatch(fetchCustomFilters());
    },
    dispatchFetchTags() {
      dispatch(fetchTags());
    },
    dispatchFetchQualityProfiles() {
      dispatch(fetchQualityProfiles());
    },
    dispatchFetchLanguageProfiles() {
      dispatch(fetchLanguageProfiles());
    },
    dispatchFetchImportLists() {
      dispatch(fetchImportLists());
    },
    dispatchFetchUISettings() {
      dispatch(fetchUISettings());
    },
    dispatchFetchStatus() {
      dispatch(fetchStatus());
    },
    onResize(dimensions) {
      dispatch(saveDimensions(dimensions));
    },
    onSidebarVisibleChange(isSidebarVisible) {
      dispatch(setIsSidebarVisible({ isSidebarVisible }));
    }
  };
}

class PageConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isLocalStorageSupported: testLocalStorage()
    };
  }

  componentDidMount() {
    if (!this.props.isPopulated) {
      this.props.dispatchFetchSeries();
      this.props.dispatchFetchCustomFilters();
      this.props.dispatchFetchTags();
      this.props.dispatchFetchQualityProfiles();
      this.props.dispatchFetchLanguageProfiles();
      this.props.dispatchFetchImportLists();
      this.props.dispatchFetchUISettings();
      this.props.dispatchFetchStatus();
    }
  }

  //
  // Listeners

  onSidebarToggle = () => {
    this.props.onSidebarVisibleChange(!this.props.isSidebarVisible);
  };

  //
  // Render

  render() {
    const {
      isPopulated,
      hasError,
      dispatchFetchSeries,
      dispatchFetchTags,
      dispatchFetchQualityProfiles,
      dispatchFetchLanguageProfiles,
      dispatchFetchImportLists,
      dispatchFetchUISettings,
      dispatchFetchStatus,
      ...otherProps
    } = this.props;

    if (hasError || !this.state.isLocalStorageSupported) {
      return (
        <ErrorPage
          {...this.state}
          {...otherProps}
        />
      );
    }

    if (isPopulated) {
      return (
        <Page
          {...otherProps}
          onSidebarToggle={this.onSidebarToggle}
        />
      );
    }

    return (
      <LoadingPage />
    );
  }
}

PageConnector.propTypes = {
  isPopulated: PropTypes.bool.isRequired,
  hasError: PropTypes.bool.isRequired,
  isSidebarVisible: PropTypes.bool.isRequired,
  dispatchFetchSeries: PropTypes.func.isRequired,
  dispatchFetchCustomFilters: PropTypes.func.isRequired,
  dispatchFetchTags: PropTypes.func.isRequired,
  dispatchFetchQualityProfiles: PropTypes.func.isRequired,
  dispatchFetchLanguageProfiles: PropTypes.func.isRequired,
  dispatchFetchImportLists: PropTypes.func.isRequired,
  dispatchFetchUISettings: PropTypes.func.isRequired,
  dispatchFetchStatus: PropTypes.func.isRequired,
  onSidebarVisibleChange: PropTypes.func.isRequired
};

export default withRouter(
  connect(createMapStateToProps, createMapDispatchToProps)(PageConnector)
);
