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
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
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
import { useIsWindows } from 'System/Status/useSystemStatus';
import { InputChanged } from 'typings/inputs';
import selectSettings from 'Utilities/selectSettings';
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
            <SeriesPoster
              className={styles.poster}
              images={images}
              size={250}
              lazy={false}
              title={title}
            />
          )}

          <div className={styles.info}>
            {overview ? <p className={styles.intro}>{overview}</p> : null}

            <Form
              validationErrors={validationErrors}
              validationWarnings={validationWarnings}
            >
              <FormRow>
                <FormLabel>{translate('RootFolder')}</FormLabel>
                <FormInputHelpText
                  text={translate('AddNewSeriesRootFolderHelpText', {
                    folder,
                  })}
                />
                <FormInput
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
                  onChange={handleInputChange}
                  {...rootFolderPath}
                />
              </FormRow>

              <FormRow>
                <FormLabel>{translate('Tags')}</FormLabel>

                <FormInput
                  type={inputTypes.TAG}
                  name="tags"
                  onChange={handleInputChange}
                  {...tags}
                />
              </FormRow>

              <FormRow>
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

                <FormInput
                  type={inputTypes.MONITOR_EPISODES_SELECT}
                  name="monitor"
                  onChange={handleInputChange}
                  {...monitor}
                />
              </FormRow>

              <FormRow>
                <FormLabel>{translate('SeasonFolder')}</FormLabel>

                <FormInput
                  type={inputTypes.CHECK}
                  name="seasonFolder"
                  onChange={handleInputChange}
                  {...seasonFolder}
                />
              </FormRow>

              <FormRow>
                <FormLabel>{translate('QualityProfile')}</FormLabel>

                <FormInput
                  type={inputTypes.QUALITY_PROFILE_SELECT}
                  name="qualityProfileId"
                  onChange={handleQualityProfileIdChange}
                  {...qualityProfileId}
                />
              </FormRow>

              <FormRow>
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
                <FormInputHelpText text={translate('SeriesTypesHelpText')} />
                <FormInput
                  type={inputTypes.SERIES_TYPE_SELECT}
                  name="seriesType"
                  onChange={handleInputChange}
                  {...seriesTypeSetting}
                  value={seriesType}
                />
              </FormRow>
            </Form>
          </div>
        </div>
      </ModalBody>

      <ModalFooter className={styles.modalFooter}>
        <div className={styles.searchOptions}>
          <span className={styles.searchLabel}>{translate('Search')}</span>

          <CheckInput
            containerClassName={styles.searchOption}
            name="searchForMissingEpisodes"
            helpText={translate('Missing')}
            onChange={handleInputChange}
            {...searchForMissingEpisodes}
          />

          <CheckInput
            containerClassName={styles.searchOption}
            name="searchForCutoffUnmetEpisodes"
            helpText={translate('CutoffUnmet')}
            onChange={handleInputChange}
            {...searchForCutoffUnmetEpisodes}
          />
        </div>

        <div className={styles.actions}>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>

          <SpinnerButton
            className={styles.addButton}
            kind={kinds.PRIMARY}
            isSpinning={isAdding}
            onPress={handleAddSeriesPress}
          >
            {translate('AddSeriesWithTitle', { title })}
          </SpinnerButton>
        </div>
      </ModalFooter>
    </ModalContent>
  );
}

export default AddNewSeriesModalContent;
