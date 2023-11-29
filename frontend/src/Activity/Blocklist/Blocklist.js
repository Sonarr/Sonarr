import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import TablePager from 'Components/Table/TablePager';
import { align, icons, kinds } from 'Helpers/Props';
import getRemovedItems from 'Utilities/Object/getRemovedItems';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import removeOldSelectedState from 'Utilities/Table/removeOldSelectedState';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import BlocklistRowConnector from './BlocklistRowConnector';

class Blocklist extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      isConfirmRemoveModalOpen: false,
      isConfirmClearModalOpen: false,
      items: props.items
    };
  }

  componentDidUpdate(prevProps) {
    const {
      items
    } = this.props;

    if (hasDifferentItems(prevProps.items, items)) {
      this.setState((state) => {
        return {
          ...removeOldSelectedState(state, getRemovedItems(prevProps.items, items)),
          items
        };
      });

      return;
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

  onRemoveSelectedPress = () => {
    this.setState({ isConfirmRemoveModalOpen: true });
  };

  onRemoveSelectedConfirmed = () => {
    this.props.onRemoveSelected(this.getSelectedIds());
    this.setState({ isConfirmRemoveModalOpen: false });
  };

  onConfirmRemoveModalClose = () => {
    this.setState({ isConfirmRemoveModalOpen: false });
  };

  onClearBlocklistPress = () => {
    this.setState({ isConfirmClearModalOpen: true });
  };

  onClearBlocklistConfirmed = () => {
    this.props.onClearBlocklistPress();
    this.setState({ isConfirmClearModalOpen: false });
  };

  onConfirmClearModalClose = () => {
    this.setState({ isConfirmClearModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      columns,
      totalRecords,
      isRemoving,
      isClearingBlocklistExecuting,
      ...otherProps
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState,
      isConfirmRemoveModalOpen,
      isConfirmClearModalOpen
    } = this.state;

    const selectedIds = this.getSelectedIds();

    return (
      <PageContent title={translate('Blocklist')}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label={translate('RemoveSelected')}
              iconName={icons.REMOVE}
              isDisabled={!selectedIds.length}
              isSpinning={isRemoving}
              onPress={this.onRemoveSelectedPress}
            />

            <PageToolbarButton
              label={translate('Clear')}
              iconName={icons.CLEAR}
              isDisabled={!items.length}
              isSpinning={isClearingBlocklistExecuting}
              onPress={this.onClearBlocklistPress}
            />
          </PageToolbarSection>

          <PageToolbarSection alignContent={align.RIGHT}>
            <TableOptionsModalWrapper
              {...otherProps}
              columns={columns}
            >
              <PageToolbarButton
                label={translate('Options')}
                iconName={icons.TABLE}
              />
            </TableOptionsModalWrapper>
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody>
          {
            isFetching && !isPopulated &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error &&
              <Alert kind={kinds.DANGER}>
                {translate('BlocklistLoadError')}
              </Alert>
          }

          {
            isPopulated && !error && !items.length &&
              <Alert kind={kinds.INFO}>
                {translate('NoHistoryBlocklist')}
              </Alert>
          }

          {
            isPopulated && !error && !!items.length &&
              <div>
                <Table
                  selectAll={true}
                  allSelected={allSelected}
                  allUnselected={allUnselected}
                  columns={columns}
                  {...otherProps}
                  onSelectAllChange={this.onSelectAllChange}
                >
                  <TableBody>
                    {
                      items.map((item) => {
                        return (
                          <BlocklistRowConnector
                            key={item.id}
                            isSelected={selectedState[item.id] || false}
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
                  isFetching={isFetching}
                  {...otherProps}
                />
              </div>
          }
        </PageContentBody>

        <ConfirmModal
          isOpen={isConfirmRemoveModalOpen}
          kind={kinds.DANGER}
          title={translate('RemoveSelected')}
          message={translate('RemoveSelectedBlocklistMessageText')}
          confirmLabel={translate('RemoveSelected')}
          onConfirm={this.onRemoveSelectedConfirmed}
          onCancel={this.onConfirmRemoveModalClose}
        />

        <ConfirmModal
          isOpen={isConfirmClearModalOpen}
          kind={kinds.DANGER}
          title={translate('ClearBlocklist')}
          message={translate('ClearBlocklistMessageText')}
          confirmLabel={translate('Clear')}
          onConfirm={this.onClearBlocklistConfirmed}
          onCancel={this.onConfirmClearModalClose}
        />
      </PageContent>
    );
  }
}

Blocklist.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  totalRecords: PropTypes.number,
  isRemoving: PropTypes.bool.isRequired,
  isClearingBlocklistExecuting: PropTypes.bool.isRequired,
  onRemoveSelected: PropTypes.func.isRequired,
  onClearBlocklistPress: PropTypes.func.isRequired
};

export default Blocklist;
