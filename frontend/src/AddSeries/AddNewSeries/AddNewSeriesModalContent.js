import PropTypes from 'prop-types';
import React, { Component } from 'react';
import SeriesMonitoringOptionsPopoverContent from 'AddSeries/SeriesMonitoringOptionsPopoverContent';
import SeriesTypePopoverContent from 'AddSeries/SeriesTypePopoverContent';
import CheckInput from 'Components/Form/CheckInput';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Popover from 'Components/Tooltip/Popover';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import SeriesPoster from 'Series/SeriesPoster';
import * as seriesTypes from 'Utilities/Series/seriesTypes';
import styles from './AddNewSeriesModalContent.css';

class AddNewSeriesModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      seriesType: props.initialSeriesType === seriesTypes.STANDARD ?
        props.seriesType.value :
        props.initialSeriesType
    };
  }

  componentDidUpdate(prevProps) {
    if (this.props.seriesType.value !== prevProps.seriesType.value) {
      this.setState({ seriesType: this.props.seriesType.value });
    }
  }

  //
  // Listeners

  onQualityProfileIdChange = ({ value }) => {
    this.props.onInputChange({ name: 'qualityProfileId', value: parseInt(value) });
  };

  onLanguageProfileIdChange = ({ value }) => {
    this.props.onInputChange({ name: 'languageProfileId', value: parseInt(value) });
  };

  onAddSeriesPress = () => {
    const {
      seriesType
    } = this.state;

    this.props.onAddSeriesPress(
      seriesType
    );
  };

  //
  // Render

  render() {
    const {
      title,
      year,
      overview,
      images,
      isAdding,
      rootFolderPath,
      monitor,
      qualityProfileId,
      languageProfileId,
      seriesType,
      seasonFolder,
      searchForMissingEpisodes,
      searchForCutoffUnmetEpisodes,
      folder,
      tags,
      showLanguageProfile,
      isSmallScreen,
      isWindows,
      onModalClose,
      onInputChange,
      ...otherProps
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {title}

          {
            !title.contains(year) && !!year &&
              <span className={styles.year}>({year})</span>
          }
        </ModalHeader>

        <ModalBody>
          <div className={styles.container}>
            {
              isSmallScreen ?
                null :
                <div className={styles.poster}>
                  <SeriesPoster
                    className={styles.poster}
                    images={images}
                    size={250}
                  />
                </div>
            }

            <div className={styles.info}>
              {
                overview ?
                  <div className={styles.overview}>
                    {overview}
                  </div> :
                  null
              }

              <Form {...otherProps}>
                <FormGroup>
                  <FormLabel>Root Folder</FormLabel>

                  <FormInputGroup
                    type={inputTypes.ROOT_FOLDER_SELECT}
                    name="rootFolderPath"
                    valueOptions={{
                      seriesFolder: folder,
                      isWindows
                    }}
                    selectedValueOptions={{
                      seriesFolder: folder,
                      isWindows
                    }}
                    helpText={`'${folder}' subfolder will be created automatically`}
                    onChange={onInputChange}
                    {...rootFolderPath}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>
                    Monitor

                    <Popover
                      anchor={
                        <Icon
                          className={styles.labelIcon}
                          name={icons.INFO}
                        />
                      }
                      title="Monitoring Options"
                      body={<SeriesMonitoringOptionsPopoverContent />}
                      position={tooltipPositions.RIGHT}
                    />
                  </FormLabel>

                  <FormInputGroup
                    type={inputTypes.MONITOR_EPISODES_SELECT}
                    name="monitor"
                    onChange={onInputChange}
                    {...monitor}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>Quality Profile</FormLabel>

                  <FormInputGroup
                    type={inputTypes.QUALITY_PROFILE_SELECT}
                    name="qualityProfileId"
                    onChange={this.onQualityProfileIdChange}
                    {...qualityProfileId}
                  />
                </FormGroup>

                <FormGroup className={showLanguageProfile ? undefined : styles.hideLanguageProfile}>
                  <FormLabel>Language Profile</FormLabel>

                  <FormInputGroup
                    type={inputTypes.LANGUAGE_PROFILE_SELECT}
                    name="languageProfileId"
                    onChange={this.onLanguageProfileIdChange}
                    {...languageProfileId}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>
                    Series Type

                    <Popover
                      anchor={
                        <Icon
                          className={styles.labelIcon}
                          name={icons.INFO}
                        />
                      }
                      title="Series Types"
                      body={<SeriesTypePopoverContent />}
                      position={tooltipPositions.RIGHT}
                    />
                  </FormLabel>

                  <FormInputGroup
                    type={inputTypes.SERIES_TYPE_SELECT}
                    name="seriesType"
                    onChange={onInputChange}
                    {...seriesType}
                    value={this.state.seriesType}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>Season Folder</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="seasonFolder"
                    onChange={onInputChange}
                    {...seasonFolder}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>Tags</FormLabel>

                  <FormInputGroup
                    type={inputTypes.TAG}
                    name="tags"
                    onChange={onInputChange}
                    {...tags}
                  />
                </FormGroup>
              </Form>
            </div>
          </div>
        </ModalBody>

        <ModalFooter className={styles.modalFooter}>
          <div>
            <label className={styles.searchLabelContainer}>
              <span className={styles.searchLabel}>
                Start search for missing episodes
              </span>

              <CheckInput
                containerClassName={styles.searchInputContainer}
                className={styles.searchInput}
                name="searchForMissingEpisodes"
                onChange={onInputChange}
                {...searchForMissingEpisodes}
              />
            </label>

            <label className={styles.searchLabelContainer}>
              <span className={styles.searchLabel}>
                Start search for cutoff unmet episodes
              </span>

              <CheckInput
                containerClassName={styles.searchInputContainer}
                className={styles.searchInput}
                name="searchForCutoffUnmetEpisodes"
                onChange={onInputChange}
                {...searchForCutoffUnmetEpisodes}
              />
            </label>
          </div>

          <SpinnerButton
            className={styles.addButton}
            kind={kinds.SUCCESS}
            isSpinning={isAdding}
            onPress={this.onAddSeriesPress}
          >
            Add {title}
          </SpinnerButton>
        </ModalFooter>
      </ModalContent>
    );
  }
}

AddNewSeriesModalContent.propTypes = {
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  overview: PropTypes.string,
  initialSeriesType: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  isAdding: PropTypes.bool.isRequired,
  addError: PropTypes.object,
  rootFolderPath: PropTypes.object,
  monitor: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.object,
  languageProfileId: PropTypes.object,
  seriesType: PropTypes.object.isRequired,
  seasonFolder: PropTypes.object.isRequired,
  searchForMissingEpisodes: PropTypes.object.isRequired,
  searchForCutoffUnmetEpisodes: PropTypes.object.isRequired,
  folder: PropTypes.string.isRequired,
  tags: PropTypes.object.isRequired,
  showLanguageProfile: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isWindows: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onAddSeriesPress: PropTypes.func.isRequired
};

export default AddNewSeriesModalContent;
