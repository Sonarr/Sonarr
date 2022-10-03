import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { reprocessInteractiveImportItems, updateInteractiveImportItems } from 'Store/Actions/interactiveImportActions';
import createLanguagesSelector from 'Store/Selectors/createLanguagesSelector';
import SelectLanguageModalContent from './SelectLanguageModalContent';

function createMapStateToProps() {
  return createSelector(
    createLanguagesSelector(),
    (languages) => {
      const {
        isFetching,
        isPopulated,
        error,
        items
      } = languages;

      const filterItems = ['Any', 'Original'];
      const filteredLanguages = items.filter((lang) => !filterItems.includes(lang.name));

      return {
        isFetching,
        isPopulated,
        error,
        items: filteredLanguages
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchUpdateInteractiveImportItems: updateInteractiveImportItems,
  dispatchReprocessInteractiveImportItems: reprocessInteractiveImportItems
};

class SelectLanguageModalContentConnector extends Component {

  //
  // Listeners

  onLanguageSelect = ({ languageIds }) => {
    const {
      ids,
      dispatchUpdateInteractiveImportItems,
      dispatchReprocessInteractiveImportItems
    } = this.props;

    const languages = [];

    languageIds.forEach((languageId) => {
      const language = _.find(this.props.items,
        (item) => item.id === parseInt(languageId));

      if (language !== undefined) {
        languages.push(language);
      }
    });

    dispatchUpdateInteractiveImportItems({
      ids,
      languages
    });

    dispatchReprocessInteractiveImportItems({ ids });

    this.props.onModalClose(true);
  };

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
  dispatchUpdateInteractiveImportItems: PropTypes.func.isRequired,
  dispatchReprocessInteractiveImportItems: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectLanguageModalContentConnector);
