import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Grid } from 'react-virtualized';
import styles from './VirtualTableBody.css';

class VirtualTableBody extends Component {

  //
  // Render

  render() {
    return (
      <Grid
        {...this.props}
        style={{
          boxSizing: undefined,
          direction: undefined,
          height: undefined,
          position: undefined,
          willChange: undefined,
          overflow: undefined,
          width: undefined
        }}
        containerStyle={{
          position: undefined
        }}
      />
    );
  }
}

VirtualTableBody.propTypes = {
  className: PropTypes.string.isRequired
};

VirtualTableBody.defaultProps = {
  className: styles.tableBodyContainer
};

export default VirtualTableBody;
