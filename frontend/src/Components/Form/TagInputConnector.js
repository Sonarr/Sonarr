import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { addTag } from 'Store/Actions/tagActions';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import TagInput from './TagInput';

const validTagRegex = new RegExp('[^-_a-z0-9]', 'i');

function isValidTag(tagName) {
  try {
    return !validTagRegex.test(tagName);
  } catch (e) {
    return false;
  }
}

function createMapStateToProps() {
  return createSelector(
    (state, { value }) => value,
    createTagsSelector(),
    (tags, tagList) => {
      const sortedTags = _.sortBy(tagList, 'label');
      const filteredTagList = _.filter(sortedTags, (tag) => _.indexOf(tags, tag.id) === -1);

      return {
        tags: tags.reduce((acc, tag) => {
          const matchingTag = _.find(tagList, { id: tag });

          if (matchingTag) {
            acc.push({
              id: tag,
              name: matchingTag.label
            });
          }

          return acc;
        }, []),

        tagList: filteredTagList.map(({ id, label: name }) => {
          return {
            id,
            name
          };
        }),

        allTags: sortedTags
      };
    }
  );
}

const mapDispatchToProps = {
  addTag
};

class TagInputConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      name,
      value,
      tags,
      onChange
    } = this.props;

    if (value.length !== tags.length) {
      onChange({ name, value: tags.map((tag) => tag.id) });
    }
  }

  //
  // Listeners

  onTagAdd = (tag) => {
    const {
      name,
      value,
      allTags
    } = this.props;

    if (!tag.id) {
      const existingTag =_.some(allTags, { label: tag.name });

      if (isValidTag(tag.name) && !existingTag) {
        this.props.addTag({
          tag: { label: tag.name },
          onTagCreated: this.onTagCreated
        });
      }

      return;
    }

    const newValue = value.slice();
    newValue.push(tag.id);

    this.props.onChange({ name, value: newValue });
  }

  onTagDelete = ({ index }) => {
    const {
      name,
      value
    } = this.props;

    const newValue = value.slice();
    newValue.splice(index, 1);

    this.props.onChange({
      name,
      value: newValue
    });
  }

  onTagCreated = (tag) => {
    const {
      name,
      value
    } = this.props;

    const newValue = value.slice();
    newValue.push(tag.id);

    this.props.onChange({ name, value: newValue });
  }

  //
  // Render

  render() {
    return (
      <TagInput
        onTagAdd={this.onTagAdd}
        onTagDelete={this.onTagDelete}
        {...this.props}
      />
    );
  }
}

TagInputConnector.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.arrayOf(PropTypes.number).isRequired,
  tags: PropTypes.arrayOf(PropTypes.object).isRequired,
  allTags: PropTypes.arrayOf(PropTypes.object).isRequired,
  onChange: PropTypes.func.isRequired,
  addTag: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(TagInputConnector);
