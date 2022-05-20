import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchImportListSchema, selectImportListSchema } from 'Store/Actions/settingsActions';
import AddImportListModalContent from './AddImportListModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.importLists,
    (importLists) => {
      const {
        isSchemaFetching,
        isSchemaPopulated,
        schemaError,
        schema
      } = importLists;

      const listGroups = _.chain(schema)
        .sortBy((o) => o.listOrder)
        .groupBy('listType')
        .value();

      return {
        isSchemaFetching,
        isSchemaPopulated,
        schemaError,
        listGroups
      };
    }
  );
}

const mapDispatchToProps = {
  fetchImportListSchema,
  selectImportListSchema
};

class AddImportListModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchImportListSchema();
  }

  //
  // Listeners

  onImportListSelect = ({ implementation, name }) => {
    this.props.selectImportListSchema({ implementation, presetName: name });
    this.props.onModalClose({ listSelected: true });
  };

  //
  // Render

  render() {
    return (
      <AddImportListModalContent
        {...this.props}
        onImportListSelect={this.onImportListSelect}
      />
    );
  }
}

AddImportListModalContentConnector.propTypes = {
  fetchImportListSchema: PropTypes.func.isRequired,
  selectImportListSchema: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddImportListModalContentConnector);
