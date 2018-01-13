import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchLanguageProfileSchema } from 'Store/Actions/settingsActions';
import { updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import SelectLanguageModalContent from './SelectLanguageModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.languageProfiles,
    (languageProfiles) => {
      const {
        isFetchingSchema: isFetching,
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
  fetchLanguageProfileSchema,
  updateInteractiveImportItem
};

class SelectLanguageModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount = () => {
    if (!this.props.isPopulated) {
      this.props.fetchLanguageProfileSchema();
    }
  }

  //
  // Listeners

  onLanguageSelect = ({ value }) => {
    const languageId = parseInt(value);
    const language = _.find(this.props.items,
      (item) => item.language.id === languageId).language;

    this.props.updateInteractiveImportItem({
      id: this.props.id,
      language
    });

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
  id: PropTypes.number.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchLanguageProfileSchema: PropTypes.func.isRequired,
  updateInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectLanguageModalContentConnector);
