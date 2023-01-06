import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import ImportSeriesFooterConnector from './ImportSeriesFooterConnector';
import ImportSeriesTableConnector from './ImportSeriesTableConnector';

class ImportSeries extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.scrollerRef = React.createRef();

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {}
    };
  }

  //
  // Listeners

  getSelectedIds = () => {
    return getSelectedIds(this.state.selectedState, { parseIds: false });
  };

  onSelectAllChange = ({ value }) => {
    // Only select non-dupes
    this.setState(selectAll(this.state.selectedState, value));
  };

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) => {
      return toggleSelected(state, this.props.items, id, value, shiftKey);
    });
  };

  onRemoveSelectedStateItem = (id) => {
    this.setState((state) => {
      const selectedState = Object.assign({}, state.selectedState);
      delete selectedState[id];

      return {
        ...state,
        selectedState
      };
    });
  };

  onInputChange = ({ name, value }) => {
    this.props.onInputChange(this.getSelectedIds(), name, value);
  };

  onImportPress = () => {
    this.props.onImportPress(this.getSelectedIds());
  };

  //
  // Render

  render() {
    const {
      rootFolderId,
      path,
      rootFoldersFetching,
      rootFoldersPopulated,
      rootFoldersError,
      unmappedFolders
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState
    } = this.state;

    return (
      <PageContent title="Import Series">
        <PageContentBody ref={this.scrollerRef} >
          {
            rootFoldersFetching ? <LoadingIndicator /> : null
          }

          {
            !rootFoldersFetching && !!rootFoldersError ?
              <div>Unable to load root folders</div> :
              null
          }

          {
            !rootFoldersError &&
            !rootFoldersFetching &&
            rootFoldersPopulated &&
            !unmappedFolders.length ?
              <div>
                All series in {path} have been imported
              </div> :
              null
          }

          {
            !rootFoldersError &&
            !rootFoldersFetching &&
            rootFoldersPopulated &&
            !!unmappedFolders.length &&
            this.scrollerRef.current ?
              <ImportSeriesTableConnector
                rootFolderId={rootFolderId}
                unmappedFolders={unmappedFolders}
                allSelected={allSelected}
                allUnselected={allUnselected}
                selectedState={selectedState}
                scroller={this.scrollerRef.current}
                onSelectAllChange={this.onSelectAllChange}
                onSelectedChange={this.onSelectedChange}
                onRemoveSelectedStateItem={this.onRemoveSelectedStateItem}
              /> :
              null
          }
        </PageContentBody>

        {
          !rootFoldersError &&
          !rootFoldersFetching &&
          !!unmappedFolders.length ?
            <ImportSeriesFooterConnector
              selectedIds={this.getSelectedIds()}
              onInputChange={this.onInputChange}
              onImportPress={this.onImportPress}
            /> :
            null
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
  onInputChange: PropTypes.func.isRequired,
  onImportPress: PropTypes.func.isRequired
};

ImportSeries.defaultProps = {
  unmappedFolders: []
};

export default ImportSeries;
