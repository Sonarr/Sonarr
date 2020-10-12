import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchLanguageProfileSchema } from 'Store/Actions/settingsActions';
import { updateInteractiveImportItems, reprocessInteractiveImportItems } from 'Store/Actions/interactiveImportActions';
import SelectLanguageModalContent from './SelectLanguageModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.languageProfiles,
    (languageProfiles) => {
      const {
        isSchemaFetching: isFetching,
        isSchemaPopulated: isPopulated,
        schemaError: error,
        schema
      } = languageProfiles;

      return {
        isFetching,
        isPopulated,
        error,
        items: schema.languages ? [...schema.languages].reverse() : []
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchLanguageProfileSchema: fetchLanguageProfileSchema,
  dispatchUpdateInteractiveImportItems: updateInteractiveImportItems,
  dispatchReprocessInteractiveImportItems: reprocessInteractiveImportItems
};

class SelectLanguageModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount = () => {
    if (!this.props.isPopulated) {
      this.props.dispatchFetchLanguageProfileSchema();
    }
  }

  //
  // Listeners

  onLanguageSelect = ({ value }) => {
    const {
      ids,
      dispatchUpdateInteractiveImportItems,
      dispatchReprocessInteractiveImportItems
    } = this.props;

    const languageId = parseInt(value);
    const language = _.find(this.props.items,
      (item) => item.language.id === languageId).language;

    dispatchUpdateInteractiveImportItems({
      ids,
      language
    });

    dispatchReprocessInteractiveImportItems({ ids });

    this.props.onModalClose(true);
  }

  //
  // Render

  render() {
    return (
      <SelectLanguageModalContent
        {...this.props}
        onLanguageSelect={this.onLanguageSelect}
      />
    );
  }
}

SelectLanguageModalContentConnector.propTypes = {
  ids: PropTypes.arrayOf(PropTypes.number).isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  dispatchFetchLanguageProfileSchema: PropTypes.func.isRequired,
  dispatchUpdateInteractiveImportItems: PropTypes.func.isRequired,
  dispatchReprocessInteractiveImportItems: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectLanguageModalContentConnector);
