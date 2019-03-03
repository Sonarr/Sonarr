import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Autosuggest from 'react-autosuggest';
import classNames from 'classnames';
import { kinds } from 'Helpers/Props';
import tagShape from 'Helpers/Props/Shapes/tagShape';
import TagInputInput from './TagInputInput';
import TagInputTag from './TagInputTag';
import styles from './TagInput.css';

function getTag(value, selectedIndex, suggestions, allowNew) {
  if (selectedIndex == null && value) {
    const existingTag = _.find(suggestions, { name: value });

    if (existingTag) {
      return existingTag;
    } else if (allowNew) {
      return { name: value };
    }
  } else if (selectedIndex != null) {
    return suggestions[selectedIndex];
  }
}

class TagInput extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      value: '',
      suggestions: [],
      isFocused: false
    };

    this._autosuggestRef = null;
  }

  componentWillUnmount() {
    this.addTag.cancel();
  }

  //
  // Control

  _setAutosuggestRef = (ref) => {
    this._autosuggestRef = ref;
  }

  getSuggestionValue({ name }) {
    return name;
  }

  shouldRenderSuggestions = (value) => {
    return value.length >= this.props.minQueryLength;
  }

  renderSuggestion({ name }) {
    return name;
  }

  addTag = _.debounce((tag) => {
    this.props.onTagAdd(tag);

    this.setState({
      value: '',
      suggestions: []
    });
  }, 250, { leading: true, trailing: false })

  //
  // Listeners

  onInputContainerPress = () => {
    this._autosuggestRef.input.focus();
  }

  onInputChange = (event, { newValue, method }) => {
    const value = _.isObject(newValue) ? newValue.name : newValue;

    if (method === 'type') {
      this.setState({ value });
    }
  }

  onInputKeyDown = (event) => {
    const {
      tags,
      allowNew,
      delimiters,
      onTagDelete
    } = this.props;

    const {
      value,
      suggestions
    } = this.state;

    const keyCode = event.keyCode;

    if (keyCode === 8 && !value.length) {
      const index = tags.length - 1;

      if (index >= 0) {
        onTagDelete({ index, id: tags[index].id });
      }

      setTimeout(() => {
        this.onSuggestionsFetchRequested({ value: '' });
      });

      event.preventDefault();
    }

    if (delimiters.includes(keyCode)) {
      const selectedIndex = this._autosuggestRef.highlightedSuggestionIndex;
      const tag = getTag(value, selectedIndex, suggestions, allowNew);

      if (tag) {
        this.addTag(tag);
        event.preventDefault();
      }
    }
  }

  onInputFocus = () => {
    this.setState({ isFocused: true });
  }

  onInputBlur = () => {
    this.setState({ isFocused: false });

    if (!this._autosuggestRef) {
      return;
    }

    const {
      allowNew
    } = this.props;

    const {
      value,
      suggestions
    } = this.state;

    const selectedIndex = this._autosuggestRef.highlightedSuggestionIndex;
    const tag = getTag(value, selectedIndex, suggestions, allowNew);

    if (tag) {
      this.addTag(tag);
    }
  }

  onSuggestionsFetchRequested = ({ value }) => {
    const lowerCaseValue = value.toLowerCase();

    const {
      tags,
      tagList
    } = this.props;

    const suggestions = tagList.filter((tag) => {
      return (
        tag.name.toLowerCase().includes(lowerCaseValue) &&
        !tags.some((t) => t.id === tag.id));
    });

    this.setState({ suggestions });
  }

  onSuggestionsClearRequested = () => {
    // Required because props aren't always rendered, but no-op
    // because we don't want to reset the paths after a path is selected.
  }

  onSuggestionSelected = (event, { suggestion }) => {
    this.addTag(suggestion);
  }

  //
  // Render

  renderInputComponent = (inputProps) => {
    const {
      tags,
      kind,
      tagComponent,
      onTagDelete
    } = this.props;

    return (
      <TagInputInput
        tags={tags}
        kind={kind}
        inputProps={inputProps}
        isFocused={this.state.isFocused}
        tagComponent={tagComponent}
        onTagDelete={onTagDelete}
        onInputContainerPress={this.onInputContainerPress}
      />
    );
  }

  render() {
    const {
      className,
      inputClassName,
      placeholder,
      hasError,
      hasWarning
    } = this.props;

    const {
      value,
      suggestions,
      isFocused
    } = this.state;

    const inputProps = {
      className: inputClassName,
      name,
      value,
      placeholder,
      autoComplete: 'off',
      spellCheck: false,
      onChange: this.onInputChange,
      onKeyDown: this.onInputKeyDown,
      onFocus: this.onInputFocus,
      onBlur: this.onInputBlur
    };

    const theme = {
      container: classNames(
        className,
        isFocused && styles.isFocused,
        hasError && styles.hasError,
        hasWarning && styles.hasWarning,
      ),
      containerOpen: styles.containerOpen,
      suggestionsContainer: styles.suggestionsContainer,
      suggestionsList: styles.suggestionsList,
      suggestion: styles.suggestion,
      suggestionHighlighted: styles.suggestionHighlighted
    };

    return (
      <Autosuggest
        ref={this._setAutosuggestRef}
        id={name}
        inputProps={inputProps}
        theme={theme}
        suggestions={suggestions}
        getSuggestionValue={this.getSuggestionValue}
        shouldRenderSuggestions={this.shouldRenderSuggestions}
        focusInputOnSuggestionClick={false}
        renderSuggestion={this.renderSuggestion}
        renderInputComponent={this.renderInputComponent}
        onSuggestionSelected={this.onSuggestionSelected}
        onSuggestionsFetchRequested={this.onSuggestionsFetchRequested}
        onSuggestionsClearRequested={this.onSuggestionsClearRequested}
      />
    );
  }
}

TagInput.propTypes = {
  className: PropTypes.string.isRequired,
  inputClassName: PropTypes.string.isRequired,
  tags: PropTypes.arrayOf(PropTypes.shape(tagShape)).isRequired,
  tagList: PropTypes.arrayOf(PropTypes.shape(tagShape)).isRequired,
  allowNew: PropTypes.bool.isRequired,
  kind: PropTypes.oneOf(kinds.all).isRequired,
  placeholder: PropTypes.string.isRequired,
  delimiters: PropTypes.arrayOf(PropTypes.number).isRequired,
  minQueryLength: PropTypes.number.isRequired,
  hasError: PropTypes.bool,
  hasWarning: PropTypes.bool,
  tagComponent: PropTypes.func.isRequired,
  onTagAdd: PropTypes.func.isRequired,
  onTagDelete: PropTypes.func.isRequired
};

TagInput.defaultProps = {
  className: styles.inputContainer,
  inputClassName: styles.input,
  allowNew: true,
  kind: kinds.INFO,
  placeholder: '',
  // Tab, enter, space and comma
  delimiters: [9, 13, 32, 188],
  minQueryLength: 1,
  tagComponent: TagInputTag
};

export default TagInput;
