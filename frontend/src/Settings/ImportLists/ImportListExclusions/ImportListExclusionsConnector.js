import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as importListExclusionActions from 'Store/Actions/Settings/importListExclusions';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import ImportListExclusions from './ImportListExclusions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.importListExclusions,
    (importListExclusions) => {
      return {
        ...importListExclusions
      };
    }
  );
}

const mapDispatchToProps = {
  ...importListExclusionActions
};

class ImportListExclusionsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      useCurrentPage,
      fetchImportListExclusions,
      gotoImportListExclusionFirstPage
    } = this.props;

    registerPagePopulator(this.repopulate);

    if (useCurrentPage) {
      fetchImportListExclusions();
    } else {
      gotoImportListExclusionFirstPage();
    }
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.repopulate);
  }

  //
  // Control

  repopulate = () => {
    this.props.fetchImportListExclusions();
  };

  //
  // Listeners

  onConfirmDeleteImportListExclusion = (id) => {
    this.props.deleteImportListExclusion({ id });
    this.repopulate();
  };

  onFirstPagePress = () => {
    this.props.gotoImportListExclusionFirstPage();
  };

  onPreviousPagePress = () => {
    this.props.gotoImportListExclusionPreviousPage();
  };

  onNextPagePress = () => {
    this.props.gotoImportListExclusionNextPage();
  };

  onLastPagePress = () => {
    this.props.gotoImportListExclusionLastPage();
  };

  onPageSelect = (page) => {
    this.props.gotoImportListExclusionPage({ page });
  };

  onSortPress = (sortKey) => {
    this.props.setImportListExclusionSort({ sortKey });
  };

  onTableOptionChange = (payload) => {
    this.props.setImportListExclusionTableOption(payload);

    if (payload.pageSize) {
      this.props.gotoImportListExclusionFirstPage();
    }
  };

  //
  // Render

  render() {
    return (
      <ImportListExclusions
        {...this.state}
        {...this.props}
        onFirstPagePress={this.onFirstPagePress}
        onPreviousPagePress={this.onPreviousPagePress}
        onNextPagePress={this.onNextPagePress}
        onLastPagePress={this.onLastPagePress}
        onPageSelect={this.onPageSelect}
        onConfirmDeleteImportListExclusion={this.onConfirmDeleteImportListExclusion}
        onSortPress={this.onSortPress}
        onTableOptionChange={this.onTableOptionChange}
      />
    );
  }
}

ImportListExclusionsConnector.propTypes = {
  useCurrentPage: PropTypes.number,
  fetchImportListExclusions: PropTypes.func.isRequired,
  gotoImportListExclusionFirstPage: PropTypes.func.isRequired,
  gotoImportListExclusionPreviousPage: PropTypes.func.isRequired,
  gotoImportListExclusionNextPage: PropTypes.func.isRequired,
  gotoImportListExclusionLastPage: PropTypes.func.isRequired,
  gotoImportListExclusionPage: PropTypes.func.isRequired,
  deleteImportListExclusion: PropTypes.func.isRequired,
  setImportListExclusionSort: PropTypes.func.isRequired,
  setImportListExclusionTableOption: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportListExclusionsConnector);
