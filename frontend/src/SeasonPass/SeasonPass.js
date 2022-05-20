import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { align, sortDirections } from 'Helpers/Props';
import NoSeries from 'Series/NoSeries';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import SeasonPassFilterModalConnector from './SeasonPassFilterModalConnector';
import SeasonPassFooter from './SeasonPassFooter';
import SeasonPassRowConnector from './SeasonPassRowConnector';

const columns = [
  {
    name: 'status',
    isVisible: true
  },
  {
    name: 'sortTitle',
    label: 'Title',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'monitored',
    isVisible: true
  },
  {
    name: 'seasonCount',
    label: 'Seasons',
    isSortable: true,
    isVisible: true
  }
];

class SeasonPass extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {}
    };
  }

  componentDidUpdate(prevProps) {
    const {
      isSaving,
      saveError
    } = this.props;

    if (prevProps.isSaving && !isSaving && !saveError) {
      this.onSelectAllChange({ value: false });
    }
  }

  //
  // Control

  getSelectedIds = () => {
    return getSelectedIds(this.state.selectedState);
  };

  //
  // Listeners

  onSelectAllChange = ({ value }) => {
    this.setState(selectAll(this.state.selectedState, value));
  };

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) => {
      return toggleSelected(state, this.props.items, id, value, shiftKey);
    });
  };

  onUpdateSelectedPress = (changes) => {
    this.props.onUpdateSelectedPress({
      seriesIds: this.getSelectedIds(),
      ...changes
    });
  };

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      totalItems,
      items,
      selectedFilterKey,
      filters,
      customFilters,
      sortKey,
      sortDirection,
      isSaving,
      saveError,
      onSortPress,
      onFilterSelect
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState
    } = this.state;

    return (
      <PageContent title="Season Pass">
        <PageToolbar>
          <PageToolbarSection />
          <PageToolbarSection alignContent={align.RIGHT}>
            <FilterMenu
              alignMenu={align.RIGHT}
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={customFilters}
              filterModalConnectorComponent={SeasonPassFilterModalConnector}
              onFilterSelect={onFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody>
          {
            isFetching && !isPopulated &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error &&
              <div>Unable to load the calendar</div>
          }

          {
            !error && isPopulated && !!items.length &&
              <div>
                <Table
                  columns={columns}
                  sortKey={sortKey}
                  sortDirection={sortDirection}
                  selectAll={true}
                  allSelected={allSelected}
                  allUnselected={allUnselected}
                  onSortPress={onSortPress}
                  onSelectAllChange={this.onSelectAllChange}
                >
                  <TableBody>
                    {
                      items.map((item) => {
                        return (
                          <SeasonPassRowConnector
                            key={item.id}
                            seriesId={item.id}
                            isSelected={selectedState[item.id]}
                            onSelectedChange={this.onSelectedChange}
                          />
                        );
                      })
                    }
                  </TableBody>
                </Table>
              </div>
          }

          {
            !error && isPopulated && !items.length &&
              <NoSeries totalItems={totalItems} />
          }
        </PageContentBody>

        <SeasonPassFooter
          selectedCount={this.getSelectedIds().length}
          isSaving={isSaving}
          saveError={saveError}
          onUpdateSelectedPress={this.onUpdateSelectedPress}
        />
      </PageContent>
    );
  }
}

SeasonPass.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  totalItems: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  onSortPress: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onUpdateSelectedPress: PropTypes.func.isRequired
};

export default SeasonPass;
