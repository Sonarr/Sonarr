import PropTypes from 'prop-types';
import React, { Component } from 'react';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import ImportSeriesTableConnector from './ImportSeriesTableConnector';
import ImportSeriesFooterConnector from './ImportSeriesFooterConnector';

class ImportSeries extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      contentBody: null,
      scrollTop: 0
    };
  }

  //
  // Control

  setContentBodyRef = (ref) => {
    this.setState({ contentBody: ref });
  }

  //
  // Listeners

  getSelectedIds = () => {
    return getSelectedIds(this.state.selectedState, { parseIds: false });
  }

  onSelectAllChange = ({ value }) => {
    // Only select non-dupes
    this.setState(selectAll(this.state.selectedState, value));
  }

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) => {
      return toggleSelected(state, this.props.items, id, value, shiftKey);
    });
  }

  onRemoveSelectedStateItem = (id) => {
    this.setState((state) => {
      const selectedState = Object.assign({}, state.selectedState);
      delete selectedState[id];

      return {
        ...state,
        selectedState
      };
    });
  }

  onInputChange = ({ name, value }) => {
    this.props.onInputChange(this.getSelectedIds(), name, value);
  }

  onImportPress = () => {
    this.props.onImportPress(this.getSelectedIds());
  }

  onScroll = ({ scrollTop }) => {
    this.setState({ scrollTop });
  }

  //
  // Render

  render() {
    const {
      rootFolderId,
      path,
      rootFoldersFetching,
      rootFoldersPopulated,
      rootFoldersError,
      unmappedFolders,
      showLanguageProfile
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState,
      contentBody
    } = this.state;

    return (
      <PageContent title="Import Series">
        <PageContentBodyConnector
          ref={this.setContentBodyRef}
          onScroll={this.onScroll}
        >
          {
            rootFoldersFetching && !rootFoldersPopulated &&
              <LoadingIndicator />
          }

          {
            !rootFoldersFetching && !!rootFoldersError &&
              <div>Unable to load root folders</div>
          }

          {
            !rootFoldersError && rootFoldersPopulated && !unmappedFolders.length &&
              <div>
                All series in {path} have been imported
              </div>
          }

          {
            !rootFoldersError && rootFoldersPopulated && !!unmappedFolders.length && contentBody &&
              <ImportSeriesTableConnector
                rootFolderId={rootFolderId}
                unmappedFolders={unmappedFolders}
                allSelected={allSelected}
                allUnselected={allUnselected}
                selectedState={selectedState}
                contentBody={contentBody}
                showLanguageProfile={showLanguageProfile}
                scrollTop={this.state.scrollTop}
                onSelectAllChange={this.onSelectAllChange}
                onSelectedChange={this.onSelectedChange}
                onRemoveSelectedStateItem={this.onRemoveSelectedStateItem}
                onScroll={this.onScroll}
              />
          }
        </PageContentBodyConnector>

        {
          !rootFoldersError && rootFoldersPopulated && !!unmappedFolders.length &&
            <ImportSeriesFooterConnector
              selectedIds={this.getSelectedIds()}
              showLanguageProfile={showLanguageProfile}
              onInputChange={this.onInputChange}
              onImportPress={this.onImportPress}
            />
        }
      </PageContent>
    );
  }
}

ImportSeries.propTypes = {
  rootFolderId: PropTypes.number.isRequired,
  path: PropTypes.string,
  rootFoldersFetching: PropTypes.bool.isRequired,
  rootFoldersPopulated: PropTypes.bool.isRequired,
  rootFoldersError: PropTypes.object,
  unmappedFolders: PropTypes.arrayOf(PropTypes.object),
  items: PropTypes.arrayOf(PropTypes.object),
  showLanguageProfile: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onImportPress: PropTypes.func.isRequired
};

ImportSeries.defaultProps = {
  unmappedFolders: []
};

export default ImportSeries;
