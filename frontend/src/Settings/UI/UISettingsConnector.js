import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import { fetchUISettings, saveUISettings, setUISettingsValue } from 'Store/Actions/settingsActions';
import createLanguagesSelector from 'Store/Selectors/createLanguagesSelector';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import UISettings from './UISettings';

const SECTION = 'ui';
const FILTER_LANGUAGES = ['Any', 'Unknown', 'Original'];

function createFilteredLanguagesSelector() {
  return createSelector(
    createLanguagesSelector(),
    (languages) => {
      if (!languages || !languages.items) {
        return [];
      }

      const newItems = languages.items
        .filter((lang) => !FILTER_LANGUAGES.includes(lang.name))
        .map((item) => {
          return {
            key: item.id,
            value: item.name
          };
        });

      return {
        ...languages,
        items: newItems
      };
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createSettingsSectionSelector(SECTION),
    createFilteredLanguagesSelector(),
    (advancedSettings, sectionSettings, languages) => {
      return {
        advancedSettings,
        languages: languages.items,
        isLanguagesPopulated: languages.isPopulated,
        ...sectionSettings,
        isFetching: sectionSettings.isFetching || languages.isFetching,
        error: sectionSettings.error || languages.error
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetUISettingsValue: setUISettingsValue,
  dispatchSaveUISettings: saveUISettings,
  dispatchFetchUISettings: fetchUISettings,
  dispatchClearPendingChanges: clearPendingChanges
};

class UISettingsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      dispatchFetchUISettings
    } = this.props;

    dispatchFetchUISettings();
  }

  componentWillUnmount() {
    this.props.dispatchClearPendingChanges({ section: `settings.${SECTION}` });
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.dispatchSetUISettingsValue({ name, value });
  };

  onSavePress = () => {
    this.props.dispatchSaveUISettings();
  };

  //
  // Render

  render() {
    return (
      <UISettings
        onInputChange={this.onInputChange}
        onSavePress={this.onSavePress}
        {...this.props}
      />
    );
  }
}

UISettingsConnector.propTypes = {
  isLanguagesPopulated: PropTypes.bool.isRequired,
  dispatchSetUISettingsValue: PropTypes.func.isRequired,
  dispatchSaveUISettings: PropTypes.func.isRequired,
  dispatchFetchUISettings: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(UISettingsConnector);
