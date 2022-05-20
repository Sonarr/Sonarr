import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FileBrowserModal from 'Components/FileBrowser/FileBrowserModal';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import AutoSuggestInput from './AutoSuggestInput';
import FormInputButton from './FormInputButton';
import styles from './PathInput.css';

class PathInput extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._node = document.getElementById('portal-root');

    this.state = {
      value: props.value,
      isFileBrowserModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    const { value } = this.props;

    if (prevProps.value !== value) {
      this.setState({ value });
    }
  }

  //
  // Control

  getSuggestionValue({ path }) {
    return path;
  }

  renderSuggestion({ path }, { query }) {
    const lastSeparatorIndex = query.lastIndexOf('\\') || query.lastIndexOf('/');

    if (lastSeparatorIndex === -1) {
      return (
        <span>{path}</span>
      );
    }

    return (
      <span>
        <span className={styles.pathMatch}>
          {path.substr(0, lastSeparatorIndex)}
        </span>
        {path.substr(lastSeparatorIndex)}
      </span>
    );
  }

  //
  // Listeners

  onInputChange = ({ value }) => {
    this.setState({ value });
  };

  onInputKeyDown = (event) => {
    if (event.key === 'Tab') {
      event.preventDefault();
      const path = this.props.paths[0];

      if (path) {
        this.props.onChange({
          name: this.props.name,
          value: path.path
        });

        if (path.type !== 'file') {
          this.props.onFetchPaths(path.path);
        }
      }
    }
  };

  onInputBlur = () => {
    this.props.onChange({
      name: this.props.name,
      value: this.state.value
    });

    this.props.onClearPaths();
  };

  onSuggestionsFetchRequested = ({ value }) => {
    this.props.onFetchPaths(value);
  };

  onSuggestionsClearRequested = () => {
    // Required because props aren't always rendered, but no-op
    // because we don't want to reset the paths after a path is selected.
  };

  onSuggestionSelected = (event, { suggestionValue }) => {
    this.props.onFetchPaths(suggestionValue);
  };

  onFileBrowserOpenPress = () => {
    this.setState({ isFileBrowserModalOpen: true });
  };

  onFileBrowserModalClose = () => {
    this.setState({ isFileBrowserModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      className,
      name,
      paths,
      includeFiles,
      hasFileBrowser,
      onChange,
      ...otherProps
    } = this.props;

    const {
      value,
      isFileBrowserModalOpen
    } = this.state;

    return (
      <div className={className}>
        <AutoSuggestInput
          {...otherProps}
          className={hasFileBrowser ? styles.hasFileBrowser : undefined}
          name={name}
          value={value}
          suggestions={paths}
          getSuggestionValue={this.getSuggestionValue}
          renderSuggestion={this.renderSuggestion}
          onInputKeyDown={this.onInputKeyDown}
          onInputBlur={this.onInputBlur}
          onSuggestionSelected={this.onSuggestionSelected}
          onSuggestionsFetchRequested={this.onSuggestionsFetchRequested}
          onSuggestionsClearRequested={this.onSuggestionsClearRequested}
          onChange={this.onInputChange}
        />

        {
          hasFileBrowser &&
            <div>
              <FormInputButton
                className={styles.fileBrowserButton}
                onPress={this.onFileBrowserOpenPress}
              >
                <Icon name={icons.FOLDER_OPEN} />
              </FormInputButton>

              <FileBrowserModal
                isOpen={isFileBrowserModalOpen}
                name={name}
                value={value}
                includeFiles={includeFiles}
                onChange={onChange}
                onModalClose={this.onFileBrowserModalClose}
              />
            </div>
        }
      </div>
    );
  }
}

PathInput.propTypes = {
  className: PropTypes.string.isRequired,
  name: PropTypes.string.isRequired,
  value: PropTypes.string,
  paths: PropTypes.array.isRequired,
  includeFiles: PropTypes.bool.isRequired,
  hasFileBrowser: PropTypes.bool,
  onChange: PropTypes.func.isRequired,
  onFetchPaths: PropTypes.func.isRequired,
  onClearPaths: PropTypes.func.isRequired
};

PathInput.defaultProps = {
  className: styles.inputWrapper,
  value: '',
  hasFileBrowser: true
};

export default PathInput;
