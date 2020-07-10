import PropTypes from 'prop-types';
import React, { Component } from 'react';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import { kinds, scrollDirections } from 'Helpers/Props';
import TextInput from 'Components/Form/TextInput';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Scroller from 'Components/Scroller/Scroller';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import SelectEpisodeRow from './SelectEpisodeRow';
import styles from './SelectEpisodeModalContent.css';

const columns = [
  {
    name: 'episodeNumber',
    label: '#',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'title',
    label: 'Title',
    isVisible: true
  },
  {
    name: 'airDate',
    label: 'Air Date',
    isVisible: true
  }
];

class SelectEpisodeModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      allSelected: false,
      allUnselected: false,
      filter: '',
      lastToggled: null,
      selectedState: {}
    };
  }

  //
  // Control

  getSelectedIds = () => {
    return getSelectedIds(this.state.selectedState);
  }

  //
  // Listeners

  onFilterChange = ({ value }) => {
    this.setState({ filter: value.toLowerCase() });
  }

  onSelectAllChange = ({ value }) => {
    this.setState(selectAll(this.state.selectedState, value));
  }

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) => {
      return toggleSelected(state, this.props.items, id, value, shiftKey);
    });
  }

  onEpisodesSelect = () => {
    this.props.onEpisodesSelect(this.getSelectedIds());
  }

  //
  // Render

  render() {
    const {
      ids,
      isFetching,
      isPopulated,
      error,
      items,
      relativePath,
      sortKey,
      sortDirection,
      onSortPress,
      onModalClose
    } = this.props;

    const {
      allSelected,
      allUnselected,
      filter,
      selectedState
    } = this.state;
    const filterEpisodeNumber = parseInt(filter);

    const errorMessage = getErrorMessage(error, 'Unable to load episodes');

    const selectedFilesCount = ids.length;
    const selectedCount = this.getSelectedIds().length;
    const selectionIsValid = (
      selectedCount > 0 &&
      selectedCount % selectedFilesCount === 0
    );

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          <div className={styles.header}>
            Manual Import - Select Episode(s)
          </div>

        </ModalHeader>

        <ModalBody
          className={styles.modalBody}
          scrollDirection={scrollDirections.NONE}
        >
          <TextInput
            className={styles.filterInput}
            placeholder="Filter episodes by title or number"
            name="filter"
            value={filter}
            autoFocus={true}
            onChange={this.onFilterChange}
          />

          <Scroller
            className={styles.scroller}
            autoFocus={false}
          >
            {
              isFetching ? <LoadingIndicator /> : null
            }

            {
              error ? <div>{errorMessage}</div> : null
            }

            {
              isPopulated && !!items.length ?
                <Table
                  columns={columns}
                  selectAll={true}
                  allSelected={allSelected}
                  allUnselected={allUnselected}
                  sortKey={sortKey}
                  sortDirection={sortDirection}
                  onSortPress={onSortPress}
                  onSelectAllChange={this.onSelectAllChange}
                >
                  <TableBody>
                    {
                      items.map((item) => {
                        return item.title.toLowerCase().includes(filter) ||
                          item.episodeNumber === filterEpisodeNumber ?
                          (
                            <SelectEpisodeRow
                              key={item.id}
                              id={item.id}
                              episodeNumber={item.episodeNumber}
                              title={item.title}
                              airDate={item.airDate}
                              isSelected={selectedState[item.id]}
                              onSelectedChange={this.onSelectedChange}
                            />
                          ) :
                          null;
                      })
                    }
                  </TableBody>
                </Table> :
                null
            }

            {
              isPopulated && !items.length ?
                'No episodes were found for the selected season' :
                null
            }
          </Scroller>
        </ModalBody>

        <ModalFooter className={styles.footer}>
          <div className={styles.path}>
            {
              relativePath ?
                relativePath :
                `${selectedFilesCount} selected files`
            }
          </div>

          <div className={styles.buttons}>
            <Button onPress={onModalClose}>
              Cancel
            </Button>

            <Button
              kind={kinds.SUCCESS}
              isDisabled={!selectionIsValid}
              onPress={this.onEpisodesSelect}
            >
              Select Episodes
            </Button>
          </div>
        </ModalFooter>
      </ModalContent>
    );
  }
}

SelectEpisodeModalContent.propTypes = {
  ids: PropTypes.arrayOf(PropTypes.number).isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  relativePath: PropTypes.string,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.string,
  onSortPress: PropTypes.func.isRequired,
  onEpisodesSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectEpisodeModalContent;
