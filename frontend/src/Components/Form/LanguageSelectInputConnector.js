import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import EnhancedSelectInput from './EnhancedSelectInput';

function createMapStateToProps() {
  return createSelector(
    (state, { values }) => values,
    ( languages ) => {

      const minId = languages.reduce((min, v) => (v.key < 1 ? v.key : min), languages[0].key);

      const values = languages.map(({ key, value }) => {
        return {
          key,
          value,
          dividerAfter: minId < 1 ? key === minId : false
        };
      });

      return {
        values
      };
    }
  );
}

class LanguageSelectInputConnector extends Component {

  //
  // Render

  render() {

    return (
      <EnhancedSelectInput
        {...this.props}
        onChange={this.props.onChange}
      />
    );
  }
}

LanguageSelectInputConnector.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.oneOfType([PropTypes.arrayOf(PropTypes.number), PropTypes.number]).isRequired,
  values: PropTypes.arrayOf(PropTypes.object).isRequired,
  onChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps)(LanguageSelectInputConnector);
