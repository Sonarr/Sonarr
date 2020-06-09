import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Autosuggest from 'react-autosuggest';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import keyboardShortcuts, { shortcuts } from 'Components/keyboardShortcuts';
import SeriesSearchResult from './SeriesSearchResult';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FuseWorker from './fuse.worker';
import styles from './SeriesSearchInput.css';

const ADD_NEW_TYPE = 'addNew';

class SeriesSearchInput extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._autosuggest = null;
    this._worker = null;

    this.state = {
      value: '',
      suggestions: []
    };
  }

  componentDidMount() {
    this.props.bindShortcut(shortcuts.SERIES_SEARCH_INPUT.key, this.focusInput);
  }

  componentWillUnmount() {
    if (this._worker) {
      this._worker.removeEventListener('message', this.onSuggestionsReceived, false);
      this._worker.terminate();
      this._worker = null;
    }
  }

  getWorker() {
    if (!this._worker) {
      this._worker = new FuseWorker();
      this._worker.addEventListener('message', this.onSuggestionsReceived, false);
    }

    return this._worker;
  }

  //
  // Control

  setAutosuggestRef = (ref) => {
    this._autosuggest = ref;
  }

  focusInput = (event) => {
    event.preventDefault();
    this._autosuggest.input.focus();
  }

  getSectionSuggestions(section) {
    return section.suggestions;
  }

  renderSectionTitle(section) {
    return (
      <div className={styles.sectionTitle}>
        {section.title}

        {
          section.loading &&
          <LoadingIndicator
            className={styles.loading}
            rippleClassName={styles.ripple}
            size={20}
          />
        }
      </div>
    );
  }

  getSuggestionValue({ title }) {
    return title;
  }

  renderSuggestion(item, { query }) {
    if (item.type === ADD_NEW_TYPE) {
      return (
        <div className={styles.addNewSeriesSuggestion}>
          Search for {query}
        </div>
      );
    }

    return (
      <SeriesSearchResult
        {...item.item}
        match={item.matches[0]}
      />
    );
  }

  goToSeries(item) {
    this.setState({ value: '' });
    this.props.onGoToSeries(item.item.titleSlug);
  }

  reset() {
    this.setState({
      value: '',
      suggestions: [],
      loading: false
    });
  }

  //
  // Listeners

  onChange = (event, { newValue, method }) => {
    if (method === 'up' || method === 'down') {
      return;
    }

    this.setState({ value: newValue });
  }

  onKeyDown = (event) => {
    if (event.shiftKey || event.altKey || event.ctrlKey) {
      return;
    }

    if (event.key === 'Escape') {
      this.reset();
      return;
    }

    if (event.key !== 'Tab' && event.key !== 'Enter') {
      return;
    }

    const {
      suggestions,
      value
    } = this.state;

    const {
      highlightedSectionIndex,
      highlightedSuggestionIndex
    } = this._autosuggest.state;

    if (!suggestions.length || highlightedSectionIndex) {
      this.props.onGoToAddNewSeries(value);
      this._autosuggest.input.blur();
      this.reset();

      return;
    }

    // If an suggestion is not selected go to the first series,
    // otherwise go to the selected series.

    if (highlightedSuggestionIndex == null) {
      this.goToSeries(suggestions[0]);
    } else {
      this.goToSeries(suggestions[highlightedSuggestionIndex]);
    }

    this._autosuggest.input.blur();
    this.reset();
  }

  onBlur = () => {
    this.reset();
  }

  onSuggestionsFetchRequested = ({ value }) => {
    if (!this.state.loading) {
      this.setState({
        loading: true
      });
    }

    this.requestSuggestions(value);
  };

  requestSuggestions = _.debounce((value) => {
    if (!this.state.loading) {
      return;
    }

    const requestLoading = this.state.requestLoading;

    this.setState({
      requestValue: value,
      requestLoading: true
    });

    if (!requestLoading) {
      const payload = {
        value,
        series: this.props.series
      };

      this.getWorker().postMessage(payload);
    }
  }, 250);

  onSuggestionsReceived = (message) => {
    const {
      value,
      suggestions
    } = message.data;

    if (!this.state.loading) {
      this.setState({
        requestValue: null,
        requestLoading: false
      });
    } else if (value === this.state.requestValue) {
      this.setState({
        suggestions,
        requestValue: null,
        requestLoading: false,
        loading: false
      });
    } else {
      this.setState({
        suggestions,
        requestLoading: true
      });

      const payload = {
        value: this.state.requestValue,
        series: this.props.series
      };

      this.getWorker().postMessage(payload);
    }
  }

  onSuggestionsClearRequested = () => {
    this.setState({
      suggestions: [],
      loading: false
    });
  }

  onSuggestionSelected = (event, { suggestion }) => {
    if (suggestion.type === ADD_NEW_TYPE) {
      this.props.onGoToAddNewSeries(this.state.value);
    } else {
      this.goToSeries(suggestion);
    }
  }

  //
  // Render

  render() {
    const {
      value,
      loading,
      suggestions
    } = this.state;

    const suggestionGroups = [];

    if (suggestions.length || loading) {
      suggestionGroups.push({
        title: 'Existing Series',
        loading,
        suggestions
      });
    }

    suggestionGroups.push({
      title: 'Add New Series',
      suggestions: [
        {
          type: ADD_NEW_TYPE,
          title: value
        }
      ]
    });

    const inputProps = {
      ref: this.setInputRef,
      className: styles.input,
      name: 'seriesSearch',
      value,
      placeholder: 'Search',
      autoComplete: 'off',
      spellCheck: false,
      onChange: this.onChange,
      onKeyDown: this.onKeyDown,
      onBlur: this.onBlur,
      onFocus: this.onFocus
    };

    const theme = {
      container: styles.container,
      containerOpen: styles.containerOpen,
      suggestionsContainer: styles.seriesContainer,
      suggestionsList: styles.list,
      suggestion: styles.listItem,
      suggestionHighlighted: styles.highlighted
    };

    return (
      <div className={styles.wrapper}>
        <Icon name={icons.SEARCH} />

        <Autosuggest
          ref={this.setAutosuggestRef}
          id={name}
          inputProps={inputProps}
          theme={theme}
          focusInputOnSuggestionClick={false}
          multiSection={true}
          suggestions={suggestionGroups}
          getSectionSuggestions={this.getSectionSuggestions}
          renderSectionTitle={this.renderSectionTitle}
          getSuggestionValue={this.getSuggestionValue}
          renderSuggestion={this.renderSuggestion}
          onSuggestionSelected={this.onSuggestionSelected}
          onSuggestionsFetchRequested={this.onSuggestionsFetchRequested}
          onSuggestionsClearRequested={this.onSuggestionsClearRequested}
        />
      </div>
    );
  }
}

SeriesSearchInput.propTypes = {
  series: PropTypes.arrayOf(PropTypes.object).isRequired,
  onGoToSeries: PropTypes.func.isRequired,
  onGoToAddNewSeries: PropTypes.func.isRequired,
  bindShortcut: PropTypes.func.isRequired
};

export default keyboardShortcuts(SeriesSearchInput);
