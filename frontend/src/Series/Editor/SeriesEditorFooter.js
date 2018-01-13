import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds } from 'Helpers/Props';
import SelectInput from 'Components/Form/SelectInput';
import LanguageProfileSelectInputConnector from 'Components/Form/LanguageProfileSelectInputConnector';
import QualityProfileSelectInputConnector from 'Components/Form/QualityProfileSelectInputConnector';
import RootFolderSelectInputConnector from 'Components/Form/RootFolderSelectInputConnector';
import SeriesTypeSelectInput from 'Components/Form/SeriesTypeSelectInput';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import MoveSeriesModal from 'Series/MoveSeries/MoveSeriesModal';
import TagsModal from './Tags/TagsModal';
import DeleteSeriesModal from './Delete/DeleteSeriesModal';
import SeriesEditorFooterLabel from './SeriesEditorFooterLabel';
import styles from './SeriesEditorFooter.css';

const NO_CHANGE = 'noChange';

class SeriesEditorFooter extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      monitored: NO_CHANGE,
      qualityProfileId: NO_CHANGE,
      languageProfileId: NO_CHANGE,
      seriesType: NO_CHANGE,
      seasonFolder: NO_CHANGE,
      rootFolderPath: NO_CHANGE,
      savingTags: false,
      isDeleteSeriesModalOpen: false,
      isTagsModalOpen: false,
      isConfirmMoveModalOpen: false,
      destinationRootFolder: null
    };
  }

  componentDidUpdate(prevProps) {
    const {
      isSaving,
      saveError
    } = this.props;

    if (prevProps.isSaving && !isSaving && !saveError) {
      this.setState({
        monitored: NO_CHANGE,
        qualityProfileId: NO_CHANGE,
        languageProfileId: NO_CHANGE,
        seriesType: NO_CHANGE,
        seasonFolder: NO_CHANGE,
        rootFolderPath: NO_CHANGE,
        savingTags: false
      });
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.setState({ [name]: value });

    if (value === NO_CHANGE) {
      return;
    }

    switch (name) {
      case 'rootFolderPath':
        this.setState({
          isConfirmMoveModalOpen: true,
          destinationRootFolder: value
        });
        break;
      case 'monitored':
        this.props.onSaveSelected({ [name]: value === 'monitored' });
        break;
      case 'seasonFolder':
        this.props.onSaveSelected({ [name]: value === 'yes' });
        break;
      default:
        this.props.onSaveSelected({ [name]: value });
    }
  }

  onApplyTagsPress = (tags, applyTags) => {
    this.setState({
      savingTags: true,
      isTagsModalOpen: false
    });

    this.props.onSaveSelected({
      tags,
      applyTags
    });
  }

  onDeleteSelectedPress = () => {
    this.setState({ isDeleteSeriesModalOpen: true });
  }

  onDeleteSeriesModalClose = () => {
    this.setState({ isDeleteSeriesModalOpen: false });
  }

  onTagsPress = () => {
    this.setState({ isTagsModalOpen: true });
  }

  onTagsModalClose = () => {
    this.setState({ isTagsModalOpen: false });
  }

  onSaveRootFolderPress = () => {
    this.setState({
      isConfirmMoveModalOpen: false,
      destinationRootFolder: null
    });

    this.props.onSaveSelected({ rootFolderPath: this.state.destinationRootFolder });
  }

  onMoveSeriesPress = () => {
    this.setState({
      isConfirmMoveModalOpen: false,
      destinationRootFolder: null
    });

    this.props.onSaveSelected({
      rootFolderPath: this.state.destinationRootFolder,
      moveFiles: true
    });
  }

  //
  // Render

  render() {
    const {
      seriesIds,
      selectedCount,
      isSaving,
      isDeleting,
      isOrganizingSeries,
      showLanguageProfile,
      onOrganizeSeriesPress
    } = this.props;

    const {
      monitored,
      qualityProfileId,
      languageProfileId,
      seriesType,
      seasonFolder,
      rootFolderPath,
      savingTags,
      isTagsModalOpen,
      isDeleteSeriesModalOpen,
      isConfirmMoveModalOpen,
      destinationRootFolder
    } = this.state;

    const monitoredOptions = [
      { key: NO_CHANGE, value: 'No Change', disabled: true },
      { key: 'monitored', value: 'Monitored' },
      { key: 'unmonitored', value: 'Unmonitored' }
    ];

    const seasonFolderOptions = [
      { key: NO_CHANGE, value: 'No Change', disabled: true },
      { key: 'yes', value: 'Yes' },
      { key: 'no', value: 'No' }
    ];

    return (
      <PageContentFooter>
        <div className={styles.inputContainer}>
          <SeriesEditorFooterLabel
            label="Monitor Series"
            isSaving={isSaving && monitored !== NO_CHANGE}
          />

          <SelectInput
            name="monitored"
            value={monitored}
            values={monitoredOptions}
            isDisabled={!selectedCount}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <SeriesEditorFooterLabel
            label="Quality Profile"
            isSaving={isSaving && qualityProfileId !== NO_CHANGE}
          />

          <QualityProfileSelectInputConnector
            name="qualityProfileId"
            value={qualityProfileId}
            includeNoChange={true}
            isDisabled={!selectedCount}
            onChange={this.onInputChange}
          />
        </div>

        {
          showLanguageProfile &&
            <div className={styles.inputContainer}>
              <SeriesEditorFooterLabel
                label="Language Profile"
                isSaving={isSaving && languageProfileId !== NO_CHANGE}
              />

              <LanguageProfileSelectInputConnector
                name="languageProfileId"
                value={languageProfileId}
                includeNoChange={true}
                isDisabled={!selectedCount}
                onChange={this.onInputChange}
              />
            </div>
        }

        <div className={styles.inputContainer}>
          <SeriesEditorFooterLabel
            label="Series Type"
            isSaving={isSaving && seriesType !== NO_CHANGE}
          />

          <SeriesTypeSelectInput
            name="seriesType"
            value={seriesType}
            includeNoChange={true}
            isDisabled={!selectedCount}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <SeriesEditorFooterLabel
            label="Season Folder"
            isSaving={isSaving && seasonFolder !== NO_CHANGE}
          />

          <SelectInput
            name="seasonFolder"
            value={seasonFolder}
            values={seasonFolderOptions}
            isDisabled={!selectedCount}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <SeriesEditorFooterLabel
            label="Root Folder"
            isSaving={isSaving && rootFolderPath !== NO_CHANGE}
          />

          <RootFolderSelectInputConnector
            name="rootFolderPath"
            value={rootFolderPath}
            includeNoChange={true}
            isDisabled={!selectedCount}
            selectedValueOptions={{ includeFreeSpace: false }}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.buttonContainer}>
          <div className={styles.buttonContainerContent}>
            <SeriesEditorFooterLabel
              label={`${selectedCount} Series Selected`}
              isSaving={false}
            />

            <div className={styles.buttons}>
              <div>
                <SpinnerButton
                  className={styles.organizeSelectedButton}
                  kind={kinds.WARNING}
                  isSpinning={isOrganizingSeries}
                  isDisabled={!selectedCount || isOrganizingSeries}
                  onPress={onOrganizeSeriesPress}
                >
                  Rename Files
                </SpinnerButton>

                <SpinnerButton
                  className={styles.tagsButton}
                  isSpinning={isSaving && savingTags}
                  isDisabled={!selectedCount || isOrganizingSeries}
                  onPress={this.onTagsPress}
                >
                  Set Tags
                </SpinnerButton>
              </div>

              <SpinnerButton
                className={styles.deleteSelectedButton}
                kind={kinds.DANGER}
                isSpinning={isDeleting}
                isDisabled={!selectedCount || isDeleting}
                onPress={this.onDeleteSelectedPress}
              >
                Delete
              </SpinnerButton>
            </div>
          </div>
        </div>

        <TagsModal
          isOpen={isTagsModalOpen}
          seriesIds={seriesIds}
          onApplyTagsPress={this.onApplyTagsPress}
          onModalClose={this.onTagsModalClose}
        />

        <DeleteSeriesModal
          isOpen={isDeleteSeriesModalOpen}
          seriesIds={seriesIds}
          onModalClose={this.onDeleteSeriesModalClose}
        />

        <MoveSeriesModal
          destinationRootFolder={destinationRootFolder}
          isOpen={isConfirmMoveModalOpen}
          onSavePress={this.onSaveRootFolderPress}
          onMoveSeriesPress={this.onMoveSeriesPress}
        />
      </PageContentFooter>
    );
  }
}

SeriesEditorFooter.propTypes = {
  seriesIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  selectedCount: PropTypes.number.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  isDeleting: PropTypes.bool.isRequired,
  deleteError: PropTypes.object,
  isOrganizingSeries: PropTypes.bool.isRequired,
  showLanguageProfile: PropTypes.bool.isRequired,
  onSaveSelected: PropTypes.func.isRequired,
  onOrganizeSeriesPress: PropTypes.func.isRequired
};

export default SeriesEditorFooter;
