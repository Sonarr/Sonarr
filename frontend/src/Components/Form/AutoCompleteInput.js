import jdu from 'jdu';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import AutoSuggestInput from './AutoSuggestInput';

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
  };

  onInputBlur = () => {
    this.setState({ suggestions: [] });
  };

  onSuggestionsFetchRequested = ({ value }) => {
    const { values } = this.props;
    const lowerCaseValue = jdu.replace(value).toLowerCase();

    const filteredValues = values.filter((v) => {
      return jdu.replace(v).toLowerCase().contains(lowerCaseValue);
    });

    this.setState({ suggestions: filteredValues });
  };

  onSuggestionsClearRequested = () => {
    this.setState({ suggestions: [] });
  };

  //
  // Render

  render() {
    const {
      name,
      value,
      ...otherProps
    } = this.props;

    const { suggestions } = this.state;

    return (
      <AutoSuggestInput
        {...otherProps}
        name={name}
        value={value}
        suggestions={suggestions}
        getSuggestionValue={this.getSuggestionValue}
        renderSuggestion={this.renderSuggestion}
        onInputBlur={this.onInputBlur}
        onSuggestionsFetchRequested={this.onSuggestionsFetchRequested}
        onSuggestionsClearRequested={this.onSuggestionsClearRequested}
      />
    );
  }
}

AutoCompleteInput.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.string,
  values: PropTypes.arrayOf(PropTypes.string).isRequired,
  onChange: PropTypes.func.isRequired
};

AutoCompleteInput.defaultProps = {
  value: ''
};

export default AutoCompleteInput;
