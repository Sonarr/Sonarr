import React, { Component } from 'react';
import { connect } from 'react-redux';
import getDisplayName from 'Helpers/getDisplayName';

function connectSection(mapStateToProps, mapDispatchToProps, mergeProps, options = {}, sectionOptions = {}) {
  return function wrap(WrappedComponent) {
    const ConnectedComponent = connect(mapStateToProps, mapDispatchToProps, mergeProps, options)(WrappedComponent);

    class Section extends Component {

      //
      // Control

      getWrappedInstance = () => {
        if (this._wrappedInstance) {
          return this._wrappedInstance.getWrappedInstance();
        }
      }

      //
      // Listeners

      setWrappedInstanceRef = (ref) => {
        this._wrappedInstance = ref;
      }

      //
      // Render

      render() {
        if (options.withRef) {
          return (
            <ConnectedComponent
              ref={this.setWrappedInstanceRef}
              {...sectionOptions}
              {...this.props}
            />
          );
        }

        return (
          <ConnectedComponent
            {...sectionOptions}
            {...this.props}
          />
        );
      }
    }

    Section.displayName = `Section(${getDisplayName(WrappedComponent)})`;
    Section.WrappedComponent = WrappedComponent;

    return Section;
  };
}

export default connectSection;
