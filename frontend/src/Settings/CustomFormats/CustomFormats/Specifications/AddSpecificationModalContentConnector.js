import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchCustomFormatSpecificationSchema, selectCustomFormatSpecificationSchema } from 'Store/Actions/settingsActions';
import AddSpecificationModalContent from './AddSpecificationModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.customFormatSpecifications,
    (specifications) => {
      const {
        isSchemaFetching,
        isSchemaPopulated,
        schemaError,
        schema
      } = specifications;

      return {
        isSchemaFetching,
        isSchemaPopulated,
        schemaError,
        schema
      };
    }
  );
}

const mapDispatchToProps = {
  fetchCustomFormatSpecificationSchema,
  selectCustomFormatSpecificationSchema
};

class AddSpecificationModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchCustomFormatSpecificationSchema();
  }

  //
  // Listeners

  onSpecificationSelect = ({ implementation, name }) => {
    this.props.selectCustomFormatSpecificationSchema({ implementation, presetName: name });
    this.props.onModalClose({ specificationSelected: true });
  };

  //
  // Render

  render() {
    return (
      <AddSpecificationModalContent
        {...this.props}
        onSpecificationSelect={this.onSpecificationSelect}
      />
    );
  }
}

AddSpecificationModalContentConnector.propTypes = {
  fetchCustomFormatSpecificationSchema: PropTypes.func.isRequired,
  selectCustomFormatSpecificationSchema: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddSpecificationModalContentConnector);
