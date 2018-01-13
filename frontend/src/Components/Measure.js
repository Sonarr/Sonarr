import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactMeasure from 'react-measure';

class Measure extends Component {

  //
  // Lifecycle

  componentWillUnmount() {
    this.onMeasure.cancel();
  }

  //
  // Listeners

  onMeasure = _.debounce((payload) => {
    this.props.onMeasure(payload);
  }, 250, { leading: true, trailing: false })

  //
  // Render

  render() {
    return (
      <ReactMeasure
        {...this.props}
      />
    );
  }
}

Measure.propTypes = {
  onMeasure: PropTypes.func.isRequired
};

export default Measure;
