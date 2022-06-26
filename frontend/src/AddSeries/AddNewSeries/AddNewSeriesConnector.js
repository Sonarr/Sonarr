import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearAddSeries, lookupSeries } from 'Store/Actions/addSeriesActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import parseUrl from 'Utilities/String/parseUrl';
import AddNewSeries from './AddNewSeries';

function createMapStateToProps() {
  return createSelector(
    (state) => state.addSeries,
    (state) => state.series.items.length,
    (state) => state.router.location,
    (addSeries, existingSeriesCount, location) => {
      const { params } = parseUrl(location.search);

      return {
        ...addSeries,
        term: params.term,
        hasExistingSeries: existingSeriesCount > 0
      };
    }
  );
}

const mapDispatchToProps = {
  lookupSeries,
  clearAddSeries,
  fetchRootFolders
};

class AddNewSeriesConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._seriesLookupTimeout = null;
  }

  componentDidMount() {
    this.props.fetchRootFolders();
  }

  componentWillUnmount() {
    if (this._seriesLookupTimeout) {
      clearTimeout(this._seriesLookupTimeout);
    }

    this.props.clearAddSeries();
  }

  //
  // Listeners

  onSeriesLookupChange = (term) => {
    if (this._seriesLookupTimeout) {
      clearTimeout(this._seriesLookupTimeout);
    }

    if (term.trim() === '') {
      this.props.clearAddSeries();
    } else {
      this._seriesLookupTimeout = setTimeout(() => {
        this.props.lookupSeries({ term });
      }, 300);
    }
  };

  onClearSeriesLookup = () => {
    this.props.clearAddSeries();
  };

  //
  // Render

  render() {
    const {
      term,
      ...otherProps
    } = this.props;

    return (
      <AddNewSeries
        term={term}
        {...otherProps}
        onSeriesLookupChange={this.onSeriesLookupChange}
        onClearSeriesLookup={this.onClearSeriesLookup}
      />
    );
  }
}

AddNewSeriesConnector.propTypes = {
  term: PropTypes.string,
  lookupSeries: PropTypes.func.isRequired,
  clearAddSeries: PropTypes.func.isRequired,
  fetchRootFolders: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddNewSeriesConnector);
