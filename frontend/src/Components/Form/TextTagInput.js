import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactTags from 'react-tag-autocomplete';
import classNames from 'classnames';
import { kinds } from 'Helpers/Props';
import styles from './TagInput.css';

class TextTagInput extends Component {

  //
  // Render

  render() {
    const {
      tags,
      allowNew,
      kind,
      placeholder,
      onTagAdd,
      onTagDelete
    } = this.props;

    const tagInputClassNames = {
      root: styles.container,
      rootFocused: styles.containerFocused,
      selected: styles.selectedTagContainer,
      selectedTag: classNames(styles.selectedTag, styles[kind]),
      search: styles.searchInputContainer,
      searchInput: styles.searchInput,
      suggestions: styles.suggestions,
      suggestionActive: styles.suggestionActive,
      suggestionDisabled: styles.suggestionDisabled
    };

    return (
      <ReactTags
        classNames={tagInputClassNames}
        tags={tags}
        allowNew={allowNew}
        minQueryLength={1}
        placeholder={placeholder}
        handleAddition={onTagAdd}
        handleDelete={onTagDelete}
      />
    );
  }
}

const tagShape = {
  id: PropTypes.string.isRequired,
  name: PropTypes.string.isRequired
};

TextTagInput.propTypes = {
  tags: PropTypes.arrayOf(PropTypes.shape(tagShape)).isRequired,
  allowNew: PropTypes.bool.isRequired,
  kind: PropTypes.string.isRequired,
  placeholder: PropTypes.string,
  onTagAdd: PropTypes.func.isRequired,
  onTagDelete: PropTypes.func.isRequired
};

TextTagInput.defaultProps = {
  allowNew: true,
  kind: kinds.INFO
};

export default TextTagInput;
