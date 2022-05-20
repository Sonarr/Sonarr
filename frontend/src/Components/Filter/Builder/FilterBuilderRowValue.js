import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TagInput from 'Components/Form/TagInput';
import { filterBuilderTypes, filterBuilderValueTypes, kinds } from 'Helpers/Props';
import tagShape from 'Helpers/Props/Shapes/tagShape';
import convertToBytes from 'Utilities/Number/convertToBytes';
import formatBytes from 'Utilities/Number/formatBytes';
import FilterBuilderRowValueTag from './FilterBuilderRowValueTag';

export const NAME = 'value';

function getTagDisplayValue(value, selectedFilterBuilderProp) {
  if (selectedFilterBuilderProp.valueType === filterBuilderValueTypes.BYTES) {
    return formatBytes(value);
  }

  return value;
}

function getValue(input, selectedFilterBuilderProp) {
  if (selectedFilterBuilderProp.valueType === filterBuilderValueTypes.BYTES) {
    const match = input.match(/^(\d+)([kmgt](i?b)?)$/i);

    if (match && match.length > 1) {
      const [, value, unit] = input.match(/^(\d+)([kmgt](i?b)?)$/i);

      switch (unit.toLowerCase()) {
        case 'k':
          return convertToBytes(value, 1, true);
        case 'm':
          return convertToBytes(value, 2, true);
        case 'g':
          return convertToBytes(value, 3, true);
        case 't':
          return convertToBytes(value, 4, true);
        case 'kb':
          return convertToBytes(value, 1, true);
        case 'mb':
          return convertToBytes(value, 2, true);
        case 'gb':
          return convertToBytes(value, 3, true);
        case 'tb':
          return convertToBytes(value, 4, true);
        case 'kib':
          return convertToBytes(value, 1, true);
        case 'mib':
          return convertToBytes(value, 2, true);
        case 'gib':
          return convertToBytes(value, 3, true);
        case 'tib':
          return convertToBytes(value, 4, true);
        default:
          return parseInt(value);
      }
    }
  }

  if (selectedFilterBuilderProp.type === filterBuilderTypes.NUMBER) {
    return parseInt(input);
  }

  return input;
}

class FilterBuilderRowValue extends Component {

  //
  // Listeners

  onTagAdd = (tag) => {
    const {
      filterValue,
      selectedFilterBuilderProp,
      onChange
    } = this.props;

    let value = tag.id;

    if (value == null) {
      value = getValue(tag.name, selectedFilterBuilderProp);
    }

    onChange({
      name: NAME,
      value: [...filterValue, value]
    });
  };

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
  };

  //
  // Render

  render() {
    const {
      filterValue,
      selectedFilterBuilderProp,
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
        name: getTagDisplayValue(id, selectedFilterBuilderProp)
      };
    });

    return (
      <TagInput
        name={NAME}
        tags={tags}
        tagList={tagList}
        allowNew={!tagList.length}
        kind={kinds.DEFAULT}
        delimiters={['Tab', 'Enter']}
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
  filterValue: PropTypes.arrayOf(PropTypes.oneOfType([PropTypes.bool, PropTypes.string, PropTypes.number])).isRequired,
  selectedFilterBuilderProp: PropTypes.object.isRequired,
  tagList: PropTypes.arrayOf(PropTypes.shape(tagShape)).isRequired,
  onChange: PropTypes.func.isRequired
};

FilterBuilderRowValue.defaultProps = {
  filterValue: []
};

export default FilterBuilderRowValue;
