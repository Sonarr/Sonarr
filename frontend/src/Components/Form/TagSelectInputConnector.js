import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import TagInput from './TagInput';

function createMapStateToProps() {
  return createSelector(
    (state, { value }) => value,
    (state, { values }) => values,
    (tags, tagList) => {
      const sortedTags = _.sortBy(tagList, 'value');

      return {
        tags: tags.reduce((acc, tag) => {
          const matchingTag = _.find(tagList, { key: tag });

          if (matchingTag) {
            acc.push({
              id: tag,
              name: matchingTag.value
            });
          }

          return acc;
        }, []),

        tagList: sortedTags.map(({ key: id, value: name }) => {
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

class TagSelectInputConnector extends Component {

  //
  // Listeners

  onTagAdd = (tag) => {
    const {
      name,
      value,
      allTags
    } = this.props;

    const existingTag =_.some(allTags, { key: tag.id });

    const newValue = value.slice();

    if (existingTag) {
      newValue.push(tag.id);
    }

    this.props.onChange({ name, value: newValue });
  };

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
  };

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

TagSelectInputConnector.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.arrayOf(PropTypes.number).isRequired,
  values: PropTypes.arrayOf(PropTypes.object).isRequired,
  allTags: PropTypes.arrayOf(PropTypes.object).isRequired,
  onChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps)(TagSelectInputConnector);
