
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import split from 'Utilities/String/split';
import TextTagInput from './TextTagInput';

function createMapStateToProps() {
  return createSelector(
    (state, { value }) => value,
    (tags) => {
      return {
        tags: split(tags).reduce((result, tag) => {
          if (tag) {
            result.push({
              id: tag,
              name: tag
            });
          }

          return result;
        }, [])
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
      value
    } = this.props;

    const newValue = split(value);
    newValue.push(tag.name);

    this.props.onChange({ name, value: newValue.join(',') });
  }

  onTagDelete = (index) => {
    const {
      name,
      value
    } = this.props;

    const newValue = split(value);
    newValue.splice(index, 1);

    this.props.onChange({
      name,
      value: newValue.join(',')
    });
  }

  //
  // Render

  render() {
    return (
      <TextTagInput
        onTagAdd={this.onTagAdd}
        onTagDelete={this.onTagDelete}
        {...this.props}
      />
    );
  }
}

TextTagInputConnector.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.string,
  onChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, null)(TextTagInputConnector);
