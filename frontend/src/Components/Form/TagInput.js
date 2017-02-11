import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactTags from 'react-tag-autocomplete';
import classNames from 'classnames';
import { kinds } from 'Helpers/Props';
import styles from './TagInput.css';

class TagInput extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._tagsRef = null;
    this._inputRef = null;
  }

  //
  // Control

  _setTagsRef = (ref) => {
    this._tagsRef = ref;

    if (ref) {
      this._inputRef = this._tagsRef.input.input;

      this._inputRef.addEventListener('blur', this.onInputBlur);
    } else if (this._inputRef) {
      this._inputRef.removeEventListener('blur', this.onInputBlur);
    }
  }

  //
  // Listeners

  onInputBlur = () => {
    if (!this._tagsRef) {
      return;
    }

    const {
      tagList,
      allowNew
    } = this.props;

    const query = this._tagsRef.state.query.trim();

    if (query) {
      const existingTag = _.find(tagList, { name: query });

      if (existingTag) {
        this._tagsRef.addTag(existingTag);
      } else if (allowNew) {
        this._tagsRef.addTag({ name: query });
      }
    }
  }

  //
  // Render

  render() {
    const {
      tags,
      tagList,
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
        ref={this._setTagsRef}
        classNames={tagInputClassNames}
        tags={tags}
        suggestions={tagList}
        allowNew={allowNew}
        minQueryLength={1}
        placeholder={placeholder}
        delimiters={[9, 13, 32, 188]}
        handleAddition={onTagAdd}
        handleDelete={onTagDelete}
      />
    );
  }
}

const tagShape = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired
};

TagInput.propTypes = {
  tags: PropTypes.arrayOf(PropTypes.shape(tagShape)).isRequired,
  tagList: PropTypes.arrayOf(PropTypes.shape(tagShape)).isRequired,
  allowNew: PropTypes.bool.isRequired,
  kind: PropTypes.oneOf(kinds.all).isRequired,
  placeholder: PropTypes.string.isRequired,
  onTagAdd: PropTypes.func.isRequired,
  onTagDelete: PropTypes.func.isRequired
};

TagInput.defaultProps = {
  allowNew: true,
  kind: kinds.INFO,
  placeholder: ''
};

export default TagInput;
