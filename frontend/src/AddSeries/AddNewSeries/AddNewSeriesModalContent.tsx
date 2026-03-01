import React, { useCallback, useEffect, useMemo, useState } from 'react';
import AddSeries from 'AddSeries/AddSeries';
import {
  AddSeriesOptions,
  setAddSeriesOption,
  useAddSeriesOptions,
} from 'AddSeries/addSeriesOptionsStore';
import SeriesMonitoringOptionsPopoverContent from 'AddSeries/SeriesMonitoringOptionsPopoverContent';
import SeriesTypePopoverContent from 'AddSeries/SeriesTypePopoverContent';
import { useAppDimension } from 'App/appStore';
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
import { getValidationFailures } from 'Helpers/Hooks/useApiMutation';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import { SeriesType } from 'Series/Series';
import SeriesPoster from 'Series/SeriesPoster';
import selectSettings from 'Store/Selectors/selectSettings';
import { useIsWindows } from 'System/Status/useSystemStatus';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { useAddSeries } from './useAddSeries';
import styles from './AddNewSeriesModalContent.css';

export interface AddNewSeriesModalContentProps {
  series: AddSeries;
  initialSeriesType: SeriesType;
  onModalClose: () => void;
}

function AddNewSeriesModalContent({
  series,
  initialSeriesType,
  onModalClose,
}: AddNewSeriesModalContentProps) {
  const { title, year, overview, images, folder } = series;
  const options = useAddSeriesOptions();
  const isSmallScreen = useAppDimension('isSmallScreen');
  const isWindows = useIsWindows();

  const { isAdding, addError, addSeries } = useAddSeries();

  const { settings, validationErrors, validationWarnings } = useMemo(() => {
    return {
      ...selectSettings(options, {}),
      ...getValidationFailures(addError),
    };
  }, [options, addError]);

  const [seriesType, setSeriesType] = useState<SeriesType>(
    initialSeriesType === 'standard'
      ? settings.seriesType.value
      : initialSeriesType
  );

  const {
    monitor,
    qualityProfileId,
    rootFolderPath,
    searchForCutoffUnmetEpisodes,
    searchForMissingEpisodes,
    seasonFolder,
    seriesType: seriesTypeSetting,
    tags,
  } = settings;

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged<string | number | boolean | number[]>) => {
      setAddSeriesOption(name as keyof AddSeriesOptions, value);
    },
    []
  );

  const handleQualityProfileIdChange = useCallback(
    ({ value }: InputChanged<string | number>) => {
      setAddSeriesOption('qualityProfileId', value as number);
    },
    []
  );

  const handleAddSeriesPress = useCallback(() => {
    addSeries({
      ...series,
      rootFolderPath: rootFolderPath.value,
      addOptions: {
        monitor: monitor.value,
        searchForMissingEpisodes: searchForMissingEpisodes.value,
        searchForCutoffUnmetEpisodes: searchForCutoffUnmetEpisodes.value,
      },
      qualityProfileId: qualityProfileId.value,
      seriesType,
      seasonFolder: seasonFolder.value,
      tags: tags.value,
    });
  }, [
    series,
    seriesType,
    rootFolderPath,
    monitor,
    qualityProfileId,
    seasonFolder,
    searchForMissingEpisodes,
    searchForCutoffUnmetEpisodes,
    tags,
    addSeries,
  ]);

  useEffect(() => {
    setSeriesType(seriesTypeSetting.value);
  }, [seriesTypeSetting]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {title}

        {!title.includes(String(year)) && year ? (
          <span className={styles.year}>({year})</span>
        ) : null}
      </ModalHeader>

      <ModalBody>
        <div className={styles.container}>
          {isSmallScreen ? null : (
            <div className={styles.poster}>
              <SeriesPoster
                className={styles.poster}
                images={images}
                size={250}
                title={title}
              />
            </div>
          )}

          <div className={styles.info}>
            {overview ? (
              <div className={styles.overview}>{overview}</div>
            ) : null}

            <Form
              validationErrors={validationErrors}
              validationWarnings={validationWarnings}
            >
              <FormGroup>
                <FormLabel>{translate('RootFolder')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.ROOT_FOLDER_SELECT}
                  name="rootFolderPath"
                  valueOptions={{
                    seriesFolder: folder,
                    isWindows,
                  }}
                  selectedValueOptions={{
                    seriesFolder: folder,
                    isWindows,
                  }}
                  helpText={translate('AddNewSeriesRootFolderHelpText', {
                    folder,
                  })}
                  onChange={handleInputChange}
                  {...rootFolderPath}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>
                  {translate('Monitor')}

                  <Popover
                    anchor={
                      <Icon className={styles.labelIcon} name={icons.INFO} />
                    }
                    title={translate('MonitoringOptions')}
                    body={<SeriesMonitoringOptionsPopoverContent />}
                    position={tooltipPositions.RIGHT}
                  />
                </FormLabel>

                <FormInputGroup
                  type={inputTypes.MONITOR_EPISODES_SELECT}
                  name="monitor"
                  onChange={handleInputChange}
                  {...monitor}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('QualityProfile')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.QUALITY_PROFILE_SELECT}
                  name="qualityProfileId"
                  onChange={handleQualityProfileIdChange}
                  {...qualityProfileId}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>
                  {translate('SeriesType')}

                  <Popover
                    anchor={
                      <Icon className={styles.labelIcon} name={icons.INFO} />
                    }
                    title={translate('SeriesTypes')}
                    body={<SeriesTypePopoverContent />}
                    position={tooltipPositions.RIGHT}
                  />
                </FormLabel>

                <FormInputGroup
                  type={inputTypes.SERIES_TYPE_SELECT}
                  name="seriesType"
                  onChange={handleInputChange}
                  {...seriesTypeSetting}
                  value={seriesType}
                  helpText={translate('SeriesTypesHelpText')}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('SeasonFolder')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="seasonFolder"
                  onChange={handleInputChange}
                  {...seasonFolder}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('Tags')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.TAG}
                  name="tags"
                  onChange={handleInputChange}
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
              {translate('AddNewSeriesSearchForMissingEpisodes')}
            </span>

            <CheckInput
              containerClassName={styles.searchInputContainer}
              className={styles.searchInput}
              name="searchForMissingEpisodes"
              onChange={handleInputChange}
              {...searchForMissingEpisodes}
            />
          </label>

          <label className={styles.searchLabelContainer}>
            <span className={styles.searchLabel}>
              {translate('AddNewSeriesSearchForCutoffUnmetEpisodes')}
            </span>

            <CheckInput
              containerClassName={styles.searchInputContainer}
              className={styles.searchInput}
              name="searchForCutoffUnmetEpisodes"
              onChange={handleInputChange}
              {...searchForCutoffUnmetEpisodes}
            />
          </label>
        </div>

        <SpinnerButton
          className={styles.addButton}
          kind={kinds.SUCCESS}
          isSpinning={isAdding}
          onPress={handleAddSeriesPress}
        >
          {translate('AddSeriesWithTitle', { title })}
        </SpinnerButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default AddNewSeriesModalContent;
