import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import CheckInput from 'Components/Form/CheckInput';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import formatSeason from 'Season/formatSeason';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import OrganizePreviewRow from './OrganizePreviewRow';
import styles from './OrganizePreviewModalContent.css';

function getValue(allSelected, allUnselected) {
  if (allSelected) {
    return true;
  } else if (allUnselected) {
    return false;
  }

  return null;
}

class OrganizePreviewModalContent extends Component {

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

  onOrganizePress = () => {
    this.props.onOrganizePress(this.getSelectedIds());
  };

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      seasonNumber,
      renameEpisodes,
      episodeFormat,
      path,
      onModalClose
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState
    } = this.state;

    const selectAllValue = getValue(allSelected, allUnselected);

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          { seasonNumber == null ?
            translate('OrganizeModalHeader') :
            translate('OrganizeModalHeaderSeason', { season: formatSeason(seasonNumber) })
          }
        </ModalHeader>

        <ModalBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && error &&
              <div>{translate('OrganizeLoadError')}</div>
          }

          {
            !isFetching && isPopulated && !items.length &&
              <div>
                {
                  renameEpisodes ?
                    <div>{translate('OrganizeNothingToRename')}</div> :
                    <div>{translate('OrganizeRenamingDisabled')}</div>
                }
              </div>
          }

          {
            !isFetching && isPopulated && !!items.length &&
              <div>
                <Alert>
                  <div>
                    <InlineMarkdown data={translate('OrganizeRelativePaths', { path })} blockClassName={styles.path} />
                  </div>

                  <div>
                    <InlineMarkdown data={translate('OrganizeNamingPattern', { episodeFormat })} blockClassName={styles.episodeFormat} />
                  </div>
                </Alert>

                <div className={styles.previews}>
                  {
                    items.map((item) => {
                      return (
                        <OrganizePreviewRow
                          key={item.episodeFileId}
                          id={item.episodeFileId}
                          existingPath={item.existingPath}
                          newPath={item.newPath}
                          isSelected={selectedState[item.episodeFileId]}
                          onSelectedChange={this.onSelectedChange}
                        />
                      );
                    })
                  }
                </div>
              </div>
          }
        </ModalBody>

        <ModalFooter>
          {
            isPopulated && !!items.length &&
              <CheckInput
                className={styles.selectAllInput}
                containerClassName={styles.selectAllInputContainer}
                name="selectAll"
                value={selectAllValue}
                onChange={this.onSelectAllChange}
              />
          }

          <Button
            onPress={onModalClose}
          >
            {translate('Cancel')}
          </Button>

          <Button
            kind={kinds.PRIMARY}
            onPress={this.onOrganizePress}
          >
            {translate('Organize')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

OrganizePreviewModalContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  seasonNumber: PropTypes.number,
  path: PropTypes.string.isRequired,
  renameEpisodes: PropTypes.bool,
  episodeFormat: PropTypes.string,
  onOrganizePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default OrganizePreviewModalContent;
