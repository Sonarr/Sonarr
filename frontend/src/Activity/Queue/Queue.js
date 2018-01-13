import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import { icons } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TablePager from 'Components/Table/TablePager';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import RemoveQueueItemsModal from './RemoveQueueItemsModal';
import QueueRowConnector from './QueueRowConnector';

class Queue extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      isPendingSelected: false,
      isConfirmRemoveModalOpen: false
    };
  }

  shouldComponentUpdate(nextProps) {
    // Don't update when fetching has completed if items have changed,
    // before episodes start fetching or when episodes start fetching.

    if (
      (
        this.props.isFetching &&
        nextProps.isPopulated &&
        hasDifferentItems(this.props.items, nextProps.items)
      ) ||
      (!this.props.isEpisodesFetching && nextProps.isEpisodesFetching)
    ) {
      return false;
    }

    return true;
  }

  componentDidUpdate(prevProps) {
    if (hasDifferentItems(prevProps.items, this.props.items)) {
      this.setState({ selectedState: {} });
      return;
    }

    const selectedIds = this.getSelectedIds();
    const isPendingSelected = _.some(this.props.items, (item) => {
      return selectedIds.indexOf(item.id) > -1 && item.status === 'Delay';
    });

    if (isPendingSelected !== this.state.isPendingSelected) {
      this.setState({ isPendingSelected });
    }
  }

  //
  // Control

  getSelectedIds = () => {
    return getSelectedIds(this.state.selectedState);
  }

  //
  // Listeners

  onSelectAllChange = ({ value }) => {
    this.setState(selectAll(this.state.selectedState, value));
  }

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) => {
      return toggleSelected(state, this.props.items, id, value, shiftKey);
    });
  }

  onGrabSelectedPress = () => {
    this.props.onGrabSelectedPress(this.getSelectedIds());
  }

  onRemoveSelectedPress = () => {
    this.setState({ isConfirmRemoveModalOpen: true });
  }

  onRemoveSelectedConfirmed = (blacklist) => {
    this.props.onRemoveSelectedPress(this.getSelectedIds(), blacklist);
    this.setState({ isConfirmRemoveModalOpen: false });
  }

  onConfirmRemoveModalClose = () => {
    this.setState({ isConfirmRemoveModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      isEpisodesFetching,
      isEpisodesPopulated,
      episodesError,
      columns,
      totalRecords,
      isGrabbing,
      isRemoving,
      isCheckForFinishedDownloadExecuting,
      onRefreshPress,
      ...otherProps
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState,
      isConfirmRemoveModalOpen,
      isPendingSelected
    } = this.state;

    const isRefreshing = isFetching || isEpisodesFetching || isCheckForFinishedDownloadExecuting;
    const isAllPopulated = isPopulated && (isEpisodesPopulated || !items.length);
    const hasError = error || episodesError;
    const selectedCount = this.getSelectedIds().length;
    const disableSelectedActions = selectedCount === 0;

    return (
      <PageContent title="Queue">
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="Refresh"
              iconName={icons.REFRESH}
              isSpinning={isRefreshing}
              onPress={onRefreshPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label="Grab Selected"
              iconName={icons.DOWNLOAD}
              isDisabled={disableSelectedActions || !isPendingSelected}
              isSpinning={isGrabbing}
              onPress={this.onGrabSelectedPress}
            />

            <PageToolbarButton
              label="Remove Selected"
              iconName={icons.REMOVE}
              isDisabled={disableSelectedActions}
              isSpinning={isRemoving}
              onPress={this.onRemoveSelectedPress}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBodyConnector>
          {
            isRefreshing && !isAllPopulated &&
              <LoadingIndicator />
          }

          {
            !isRefreshing && hasError &&
              <div>
                Failed to load Queue
              </div>
          }

          {
            isPopulated && !hasError && !items.length &&
              <div>
                Queue is empty
              </div>
          }

          {
            isAllPopulated && !hasError && !!items.length &&
              <div>
                <Table
                  columns={columns}
                  selectAll={true}
                  allSelected={allSelected}
                  allUnselected={allUnselected}
                  {...otherProps}
                  onSelectAllChange={this.onSelectAllChange}
                >
                  <TableBody>
                    {
                      items.map((item) => {
                        return (
                          <QueueRowConnector
                            key={item.id}
                            episodeId={item.episodeId}
                            isSelected={selectedState[item.id]}
                            columns={columns}
                            {...item}
                            onSelectedChange={this.onSelectedChange}
                          />
                        );
                      })
                    }
                  </TableBody>
                </Table>

                <TablePager
                  totalRecords={totalRecords}
                  isFetching={isRefreshing}
                  {...otherProps}
                />
              </div>
          }
        </PageContentBodyConnector>

        <RemoveQueueItemsModal
          isOpen={isConfirmRemoveModalOpen}
          selectedCount={selectedCount}
          onRemovePress={this.onRemoveSelectedConfirmed}
          onModalClose={this.onConfirmRemoveModalClose}
        />
      </PageContent>
    );
  }
}

Queue.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  isEpisodesFetching: PropTypes.bool.isRequired,
  isEpisodesPopulated: PropTypes.bool.isRequired,
  episodesError: PropTypes.object,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  totalRecords: PropTypes.number,
  isGrabbing: PropTypes.bool.isRequired,
  isRemoving: PropTypes.bool.isRequired,
  isCheckForFinishedDownloadExecuting: PropTypes.bool.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onGrabSelectedPress: PropTypes.func.isRequired,
  onRemoveSelectedPress: PropTypes.func.isRequired
};

export default Queue;
