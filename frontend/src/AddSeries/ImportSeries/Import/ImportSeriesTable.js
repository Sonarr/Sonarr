import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import VirtualTable from 'Components/Table/VirtualTable';
import ImportSeriesHeader from './ImportSeriesHeader';
import ImportSeriesRowConnector from './ImportSeriesRowConnector';

class ImportSeriesTable extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._table = null;
  }

  componentDidMount() {
    const {
      unmappedFolders,
      defaultMonitor,
      defaultQualityProfileId,
      defaultLanguageProfileId,
      defaultSeriesType,
      defaultSeasonFolder,
      onSeriesLookup,
      onSetImportSeriesValue
    } = this.props;

    const values = {
      monitor: defaultMonitor,
      qualityProfileId: defaultQualityProfileId,
      languageProfileId: defaultLanguageProfileId,
      seriesType: defaultSeriesType,
      seasonFolder: defaultSeasonFolder
    };

    unmappedFolders.forEach((unmappedFolder) => {
      const id = unmappedFolder.name;

      onSeriesLookup(id, unmappedFolder.path);

      onSetImportSeriesValue({
        id,
        ...values
      });
    });
  }

  // This isn't great, but it's the most reliable way to ensure the items
  // are checked off even if they aren't actually visible since the cells
  // are virtualized.

  componentDidUpdate(prevProps) {
    const {
      items,
      selectedState,
      onSelectedChange,
      onRemoveSelectedStateItem
    } = this.props;

    prevProps.items.forEach((prevItem) => {
      const {
        id
      } = prevItem;

      const item = _.find(items, { id });

      if (!item) {
        onRemoveSelectedStateItem(id);
        return;
      }

      const selectedSeries = item.selectedSeries;
      const isSelected = selectedState[id];

      const isExistingSeries = !!selectedSeries &&
        _.some(prevProps.allSeries, { tvdbId: selectedSeries.tvdbId });

      // Props doesn't have a selected series or
      // the selected series is an existing series.
      if ((selectedSeries && !prevItem.selectedSeries) || (isExistingSeries && !prevItem.selectedSeries)) {
        onSelectedChange({ id, value: false });

        return;
      }

      // State is selected, but a series isn't selected or
      // the selected series is an existing series.
      if (isSelected && (!selectedSeries || isExistingSeries)) {
        onSelectedChange({ id, value: false });

        return;
      }

      // A series is being selected that wasn't previously selected.
      if (selectedSeries && selectedSeries !== prevItem.selectedSeries) {
        onSelectedChange({ id, value: true });

        return;
      }
    });

    // Forces the table to re-render if the selected state
    // has changed otherwise it will be stale.

    if (prevProps.selectedState !== selectedState && this._table) {
      this._table.forceUpdateGrid();
    }
  }

  //
  // Control

  setTableRef = (ref) => {
    this._table = ref;
  }

  rowRenderer = ({ key, rowIndex, style }) => {
    const {
      rootFolderId,
      items,
      selectedState,
      showLanguageProfile,
      onSelectedChange
    } = this.props;

    const item = items[rowIndex];

    return (
      <ImportSeriesRowConnector
        key={key}
        style={style}
        rootFolderId={rootFolderId}
        showLanguageProfile={showLanguageProfile}
        isSelected={selectedState[item.id]}
        onSelectedChange={onSelectedChange}
        id={item.id}
      />
    );
  }

  //
  // Render

  render() {
    const {
      items,
      allSelected,
      allUnselected,
      isSmallScreen,
      contentBody,
      showLanguageProfile,
      scrollTop,
      onSelectAllChange,
      onScroll
    } = this.props;

    if (!items.length) {
      return null;
    }

    return (
      <VirtualTable
        ref={this.setTableRef}
        items={items}
        contentBody={contentBody}
        isSmallScreen={isSmallScreen}
        rowHeight={52}
        scrollTop={scrollTop}
        overscanRowCount={2}
        rowRenderer={this.rowRenderer}
        header={
          <ImportSeriesHeader
            showLanguageProfile={showLanguageProfile}
            allSelected={allSelected}
            allUnselected={allUnselected}
            onSelectAllChange={onSelectAllChange}
          />
        }
        onScroll={onScroll}
      />
    );
  }
}

ImportSeriesTable.propTypes = {
  rootFolderId: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object),
  unmappedFolders: PropTypes.arrayOf(PropTypes.object),
  defaultMonitor: PropTypes.string.isRequired,
  defaultQualityProfileId: PropTypes.number,
  defaultLanguageProfileId: PropTypes.number,
  defaultSeriesType: PropTypes.string.isRequired,
  defaultSeasonFolder: PropTypes.bool.isRequired,
  allSelected: PropTypes.bool.isRequired,
  allUnselected: PropTypes.bool.isRequired,
  selectedState: PropTypes.object.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  allSeries: PropTypes.arrayOf(PropTypes.object),
  contentBody: PropTypes.object.isRequired,
  showLanguageProfile: PropTypes.bool.isRequired,
  scrollTop: PropTypes.number.isRequired,
  onSelectAllChange: PropTypes.func.isRequired,
  onSelectedChange: PropTypes.func.isRequired,
  onRemoveSelectedStateItem: PropTypes.func.isRequired,
  onSeriesLookup: PropTypes.func.isRequired,
  onSetImportSeriesValue: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired
};

export default ImportSeriesTable;
