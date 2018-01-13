import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { queueLookupSeries, setImportSeriesValue } from 'Store/Actions/importSeriesActions';
import createImportSeriesItemSelector from 'Store/Selectors/createImportSeriesItemSelector';
import ImportSeriesSelectSeries from './ImportSeriesSelectSeries';

function createMapStateToProps() {
  return createSelector(
    (state) => state.importSeries.isLookingUpSeries,
    createImportSeriesItemSelector(),
    (isLookingUpSeries, item) => {
      return {
        isLookingUpSeries,
        ...item
      };
    }
  );
}

const mapDispatchToProps = {
  queueLookupSeries,
  setImportSeriesValue
};

class ImportSeriesSelectSeriesConnector extends Component {

  //
  // Listeners

  onSearchInputChange = (term) => {
    this.props.queueLookupSeries({
      name: this.props.id,
      term,
      topOfQueue: true
    });
  }

  onSeriesSelect = (tvdbId) => {
    const {
      id,
      items
    } = this.props;

    this.props.setImportSeriesValue({
      id,
      selectedSeries: _.find(items, { tvdbId })
    });
  }

  //
  // Render

  render() {
    return (
      <ImportSeriesSelectSeries
        {...this.props}
        onSearchInputChange={this.onSearchInputChange}
        onSeriesSelect={this.onSeriesSelect}
      />
    );
  }
}

ImportSeriesSelectSeriesConnector.propTypes = {
  id: PropTypes.string.isRequired,
  items: PropTypes.arrayOf(PropTypes.object),
  selectedSeries: PropTypes.object,
  isSelected: PropTypes.bool,
  queueLookupSeries: PropTypes.func.isRequired,
  setImportSeriesValue: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportSeriesSelectSeriesConnector);
