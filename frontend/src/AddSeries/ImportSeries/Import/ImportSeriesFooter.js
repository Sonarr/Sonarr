import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes, kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import CheckInput from 'Components/Form/CheckInput';
import FormInputGroup from 'Components/Form/FormInputGroup';
import PageContentFooter from 'Components/Page/PageContentFooter';
import styles from './ImportSeriesFooter.css';

const MIXED = 'mixed';

class ImportSeriesFooter extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const {
      defaultMonitor,
      defaultQualityProfileId,
      defaultLanguageProfileId,
      defaultSeasonFolder,
      defaultSeriesType
    } = props;

    this.state = {
      monitor: defaultMonitor,
      qualityProfileId: defaultQualityProfileId,
      languageProfileId: defaultLanguageProfileId,
      seriesType: defaultSeriesType,
      seasonFolder: defaultSeasonFolder
    };
  }

  componentDidUpdate(prevProps, prevState) {
    const {
      defaultMonitor,
      defaultQualityProfileId,
      defaultLanguageProfileId,
      defaultSeriesType,
      defaultSeasonFolder,
      isMonitorMixed,
      isQualityProfileIdMixed,
      isLanguageProfileIdMixed,
      isSeriesTypeMixed,
      isSeasonFolderMixed
    } = this.props;

    const {
      monitor,
      qualityProfileId,
      languageProfileId,
      seriesType,
      seasonFolder
    } = this.state;

    const newState = {};

    if (isMonitorMixed && monitor !== MIXED) {
      newState.monitor = MIXED;
    } else if (!isMonitorMixed && monitor !== defaultMonitor) {
      newState.monitor = defaultMonitor;
    }

    if (isQualityProfileIdMixed && qualityProfileId !== MIXED) {
      newState.qualityProfileId = MIXED;
    } else if (!isQualityProfileIdMixed && qualityProfileId !== defaultQualityProfileId) {
      newState.qualityProfileId = defaultQualityProfileId;
    }

    if (isLanguageProfileIdMixed && languageProfileId !== MIXED) {
      newState.languageProfileId = MIXED;
    } else if (!isLanguageProfileIdMixed && languageProfileId !== defaultLanguageProfileId) {
      newState.languageProfileId = defaultLanguageProfileId;
    }

    if (isSeriesTypeMixed && seriesType !== MIXED) {
      newState.seriesType = MIXED;
    } else if (!isSeriesTypeMixed && seriesType !== defaultSeriesType) {
      newState.seriesType = defaultSeriesType;
    }

    if (isSeasonFolderMixed && seasonFolder != null) {
      newState.seasonFolder = null;
    } else if (!isSeasonFolderMixed && seasonFolder !== defaultSeasonFolder) {
      newState.seasonFolder = defaultSeasonFolder;
    }

    if (!_.isEmpty(newState)) {
      this.setState(newState);
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.setState({ [name]: value });
    this.props.onInputChange({ name, value });
  }

  //
  // Render

  render() {
    const {
      selectedCount,
      isImporting,
      isLookingUpSeries,
      isMonitorMixed,
      isQualityProfileIdMixed,
      isLanguageProfileIdMixed,
      isSeriesTypeMixed,
      showLanguageProfile,
      onImportPress,
      onCancelLookupPress
    } = this.props;

    const {
      monitor,
      qualityProfileId,
      languageProfileId,
      seriesType,
      seasonFolder
    } = this.state;

    return (
      <PageContentFooter>
        <div className={styles.inputContainer}>
          <div className={styles.label}>
            Monitor
          </div>

          <FormInputGroup
            type={inputTypes.MONITOR_EPISODES_SELECT}
            name="monitor"
            value={monitor}
            isDisabled={!selectedCount}
            includeMixed={isMonitorMixed}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <div className={styles.label}>
            Quality Profile
          </div>

          <FormInputGroup
            type={inputTypes.QUALITY_PROFILE_SELECT}
            name="qualityProfileId"
            value={qualityProfileId}
            isDisabled={!selectedCount}
            includeMixed={isQualityProfileIdMixed}
            onChange={this.onInputChange}
          />
        </div>

        {
          showLanguageProfile &&
            <div className={styles.inputContainer}>
              <div className={styles.label}>
                Language Profile
              </div>

              <FormInputGroup
                type={inputTypes.LANGUAGE_PROFILE_SELECT}
                name="languageProfileId"
                value={languageProfileId}
                isDisabled={!selectedCount}
                includeMixed={isLanguageProfileIdMixed}
                onChange={this.onInputChange}
              />
            </div>
        }

        <div className={styles.inputContainer}>
          <div className={styles.label}>
            Series Type
          </div>

          <FormInputGroup
            type={inputTypes.SERIES_TYPE_SELECT}
            name="seriesType"
            value={seriesType}
            isDisabled={!selectedCount}
            includeMixed={isSeriesTypeMixed}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <div className={styles.label}>
            Season Folder
          </div>

          <CheckInput
            name="seasonFolder"
            value={seasonFolder}
            isDisabled={!selectedCount}
            onChange={this.onInputChange}
          />
        </div>

        <div>
          <div className={styles.label}>
            &nbsp;
          </div>

          <div className={styles.importButtonContainer}>
            <SpinnerButton
              className={styles.importButton}
              kind={kinds.PRIMARY}
              isSpinning={isImporting}
              isDisabled={!selectedCount || isLookingUpSeries}
              onPress={onImportPress}
            >
              Import {selectedCount} Series
            </SpinnerButton>

            {
              isLookingUpSeries &&
                <Button
                  className={styles.loadingButton}
                  kind={kinds.WARNING}
                  onPress={onCancelLookupPress}
                >
                  Cancel Processing
                </Button>
            }

            {
              isLookingUpSeries &&
                <LoadingIndicator
                  className={styles.loading}
                  size={24}
                />
            }

            {
              isLookingUpSeries &&
                'Processing Folders'
            }
          </div>
        </div>
      </PageContentFooter>
    );
  }
}

ImportSeriesFooter.propTypes = {
  selectedCount: PropTypes.number.isRequired,
  isImporting: PropTypes.bool.isRequired,
  isLookingUpSeries: PropTypes.bool.isRequired,
  defaultMonitor: PropTypes.string.isRequired,
  defaultQualityProfileId: PropTypes.number,
  defaultLanguageProfileId: PropTypes.number,
  defaultSeriesType: PropTypes.string.isRequired,
  defaultSeasonFolder: PropTypes.bool.isRequired,
  isMonitorMixed: PropTypes.bool.isRequired,
  isQualityProfileIdMixed: PropTypes.bool.isRequired,
  isLanguageProfileIdMixed: PropTypes.bool.isRequired,
  isSeriesTypeMixed: PropTypes.bool.isRequired,
  isSeasonFolderMixed: PropTypes.bool.isRequired,
  showLanguageProfile: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onImportPress: PropTypes.func.isRequired,
  onCancelLookupPress: PropTypes.func.isRequired
};

export default ImportSeriesFooter;
