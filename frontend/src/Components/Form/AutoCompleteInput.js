import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Autosuggest from 'react-autosuggest';
import classNames from 'classnames';
import jdu from 'jdu';
import styles from './AutoCompleteInput.css';

class AutoCompleteInput extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      suggestions: []
    };
  }

  //
  // Control

  getSuggestionValue(item) {
    return item;
  }

  renderSuggestion(item) {
    return item;
  }

  //
  // Listeners

  onInputChange = (event, { newValue }) => {
    this.props.onChange({
      name: this.props.name,
      value: newValue
    });
  }

  onInputKeyDown = (event) => {
    const {
      name,
      value,
      onChange
    } = this.props;

    const { suggestions } = this.state;

    if (
      event.key === 'Tab' &&
      suggestions.length &&
      suggestions[0] !== this.props.value
    ) {
      event.preventDefault();

      if (value) {
        onChange({
          name,
          value: suggestions[0]
        });
      }
    }
  }

  onInputBlur = () => {
    this.setState({ suggestions: [] });
  }

  onSuggestionsFetchRequested = ({ value }) => {
    const { values } = this.props;
    const lowerCaseValue = jdu.replace(value).toLowerCase();

    const filteredValues = values.filter((v) => {
      return jdu.replace(v).toLowerCase().contains(lowerCaseValue);
    });

    this.setState({ suggestions: filteredValues });
  }

  onSuggestionsClearRequested = () => {
    this.setState({ suggestions: [] });
  }

  //
  // Render

  render() {
    const {
      className,
      inputClassName,
      name,
      value,
      placeholder,
      hasError,
      hasWarning
    } = this.props;

    const { suggestions } = this.state;

    const inputProps = {
      className: classNames(
        inputClassName,
        hasError && styles.hasError,
        hasWarning && styles.hasWarning,
      ),
      name,
      value,
      placeholder,
      autoComplete: 'off',
      spellCheck: false,
      onChange: this.onInputChange,
      onKeyDown: this.onInputKeyDown,
      onBlur: this.onInputBlur
    };

    const theme = {
      container: styles.inputContainer,
      containerOpen: styles.inputContainerOpen,
      suggestionsContainer: styles.container,
      suggestionsList: styles.list,
      suggestion: styles.listItem,
      suggestionHighlighted: styles.highlighted
    };

    return (
      <div className={className}>
        <Autosuggest
          id={name}
          inputProps={inputProps}
          theme={theme}
          suggestions={suggestions}
          getSuggestionValue={this.getSuggestionValue}
          renderSuggestion={this.renderSuggestion}
          onSuggestionsFetchRequested={this.onSuggestionsFetchRequested}
          onSuggestionsClearRequested={this.onSuggestionsClearRequested}
        />
      </div>
    );
  }
}

AutoCompleteInput.propTypes = {
  className: PropTypes.string.isRequired,
  inputClassName: PropTypes.string.isRequired,
  name: PropTypes.string.isRequired,
  value: PropTypes.string,
  values: PropTypes.arrayOf(PropTypes.string).isRequired,
  placeholder: PropTypes.string,
  hasError: PropTypes.bool,
  hasWarning: PropTypes.bool,
  onChange: PropTypes.func.isRequired
};

AutoCompleteInput.defaultProps = {
  className: styles.inputWrapper,
  inputClassName: styles.input,
  value: ''
};

export default AutoCompleteInput;
