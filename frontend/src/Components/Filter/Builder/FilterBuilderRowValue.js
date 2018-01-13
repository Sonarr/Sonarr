import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds, filterBuilderTypes } from 'Helpers/Props';
import TagInput, { tagShape } from 'Components/Form/TagInput';
import FilterBuilderRowValueTag from './FilterBuilderRowValueTag';

const NAME = 'value';

class FilterBuilderRowValue extends Component {

  //
  // Listeners

  onTagAdd = (tag) => {
    const {
      filterValue,
      selectedFilterBuilderProp,
      onChange
    } = this.props;

    let id = tag.id;

    if (id == null) {
      id = selectedFilterBuilderProp.type === filterBuilderTypes.NUMBER ?
        parseInt(tag.name) :
        tag.name;
    }

    onChange({
      name: NAME,
      value: [...filterValue, id]
    });
  }

  onTagDelete = ({ index }) => {
    const {
      filterValue,
      onChange
    } = this.props;

    const value = filterValue.filter((v, i) => i !== index);

    onChange({
      name: NAME,
      value
    });
  }

  //
  // Render

  render() {
    const {
      filterValue,
      tagList
    } = this.props;

    const hasItems = !!tagList.length;

    const tags = filterValue.map((id) => {
      if (hasItems) {
        const tag = tagList.find((t) => t.id === id);

        return {
          id,
          name: tag && tag.name
        };
      }
      return {
        id,
        name: id
      };
    });

    return (
      <TagInput
        name={NAME}
        tags={tags}
        tagList={tagList}
        allowNew={!tagList.length}
        kind={kinds.DEFAULT}
        delimiters={[9, 13]}
        maxSuggestionsLength={100}
        minQueryLength={0}
        tagComponent={FilterBuilderRowValueTag}
        onTagAdd={this.onTagAdd}
        onTagDelete={this.onTagDelete}
      />
    );
  }
}

FilterBuilderRowValue.propTypes = {
  filterValue: PropTypes.arrayOf(PropTypes.oneOfType([PropTypes.string, PropTypes.number])).isRequired,
  selectedFilterBuilderProp: PropTypes.object.isRequired,
  tagList: PropTypes.arrayOf(PropTypes.shape(tagShape)).isRequired,
  onChange: PropTypes.func.isRequired
};

export default FilterBuilderRowValue;
