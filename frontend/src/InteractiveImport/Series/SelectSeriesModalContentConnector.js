import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { reprocessInteractiveImportItems, updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import SelectSeriesModalContent from './SelectSeriesModalContent';

function createMapStateToProps() {
  return createSelector(
    createAllSeriesSelector(),
    (items) => {
      return {
        items: [...items].sort((a, b) => {
          if (a.sortTitle < b.sortTitle) {
            return -1;
          }

          if (a.sortTitle > b.sortTitle) {
            return 1;
          }

          return 0;
        })
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchReprocessInteractiveImportItems: reprocessInteractiveImportItems,
  dispatchUpdateInteractiveImportItem: updateInteractiveImportItem
};

class SelectSeriesModalContentConnector extends Component {

  //
  // Listeners

  onSeriesSelect = (seriesId) => {
    const {
      ids,
      items,
      dispatchUpdateInteractiveImportItem,
      dispatchReprocessInteractiveImportItems,
      onModalClose
    } = this.props;

    const series = items.find((s) => s.id === seriesId);

    ids.forEach((id) => {
      dispatchUpdateInteractiveImportItem({
        id,
        series,
        seasonNumber: undefined,
        episodes: []
      });
    });

    dispatchReprocessInteractiveImportItems({ ids });

    onModalClose(true);
  };

  //
  // Render

  render() {
    return (
      <SelectSeriesModalContent
        {...this.props}
        onSeriesSelect={this.onSeriesSelect}
      />
    );
  }
}

SelectSeriesModalContentConnector.propTypes = {
  ids: PropTypes.arrayOf(PropTypes.number).isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  dispatchReprocessInteractiveImportItems: PropTypes.func.isRequired,
  dispatchUpdateInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectSeriesModalContentConnector);
