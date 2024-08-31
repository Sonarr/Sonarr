
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import split from 'Utilities/String/split';
import TagInput from './TagInput';

function createMapStateToProps() {
  return createSelector(
    (state, { value }) => value,
    (tags) => {
      const tagsArray = Array.isArray(tags) ? tags : split(tags);

      return {
        tags: tagsArray.reduce((result, tag) => {
          if (tag) {
            result.push({
              id: tag,
              name: tag
            });
          }

          return result;
        }, []),
        valueArray: tagsArray
      };
    }
  );
}

class TextTagInputConnector extends Component {

  //
  // Listeners

  onTagAdd = (tag) => {
    const {
      name,
      valueArray,
      onChange
    } = this.props;

    // Split and trim tags before adding them to the list, this will
    // cleanse tags pasted in that had commas and spaces which leads
    // to oddities with restrictions (as an example).

    const newValue = [...valueArray];
    const newTags = tag.name.startsWith('/') ? [tag.name] : split(tag.name);

    newTags.forEach((newTag) => {
      const newTagValue = newTag.trim();

      if (newTagValue) {
        newValue.push(newTagValue);
      }
    });

    onChange({ name, value: newValue });
  };

  onTagDelete = ({ index }) => {
    const {
      name,
      valueArray,
      onChange
    } = this.props;

    const newValue = [...valueArray];
    newValue.splice(index, 1);

    onChange({
      name,
      value: newValue
    });
  };

  onTagReplace = (tagToReplace, newTag) => {
    const {
      name,
      valueArray,
      onChange
    } = this.props;

    const newValue = [...valueArray];
    newValue.splice(tagToReplace.index, 1);

    const newTagValue = newTag.name.trim();

    if (newTagValue) {
      newValue.push(newTagValue);
    }

    onChange({ name, value: newValue });
  };

  //
  // Render

  render() {
    return (
      <TagInput
        delimiters={['Tab', 'Enter', ',']}
        tagList={[]}
        onTagAdd={this.onTagAdd}
        onTagDelete={this.onTagDelete}
        onTagReplace={this.onTagReplace}
        {...this.props}
      />
    );
  }
}

TextTagInputConnector.propTypes = {
  name: PropTypes.string.isRequired,
  valueArray: PropTypes.arrayOf(PropTypes.string).isRequired,
  onChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, null)(TextTagInputConnector);
