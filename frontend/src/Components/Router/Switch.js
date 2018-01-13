import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Switch as RouterSwitch } from 'react-router-dom';
import getPathWithUrlBase from 'Utilities/getPathWithUrlBase';
import { map } from 'Helpers/elementChildren';

class Switch extends Component {

  //
  // Render

  render() {
    const {
      children
    } = this.props;

    return (
      <RouterSwitch>
        {
          map(children, (child) => {
            const {
              path: childPath,
              addUrlBase = true
            } = child.props;

            if (!childPath) {
              return child;
            }

            const path = addUrlBase ? getPathWithUrlBase(childPath) : childPath;

            return React.cloneElement(child, { path });
          })
        }
      </RouterSwitch>
    );
  }
}

Switch.propTypes = {
  children: PropTypes.node.isRequired
};

export default Switch;
