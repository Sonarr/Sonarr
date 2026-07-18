import React, { useCallback, useEffect, useState } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import useDebounce from 'Helpers/Hooks/useDebounce';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import { useShowAdvancedSettings } from 'Settings/advancedSettingsStore';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import NamingModal from './NamingModal';
import {
  NamingSettingsModel,
  useManageNamingSettings,
  useNamingExamples,
} from './useNamingSettings';
import styles from './Naming.css';

interface NamingModalOptions {
  name: keyof Pick<
    NamingSettingsModel,
    | 'standardEpisodeFormat'
    | 'dailyEpisodeFormat'
    | 'animeEpisodeFormat'
    | 'seriesFolderFormat'
    | 'seasonFolderFormat'
    | 'specialsFolderFormat'
  >;
  season?: boolean;
  episode?: boolean;
  daily?: boolean;
  anime?: boolean;
  additional?: boolean;
}

interface NamingProps {
  setChildSave: (saveCallback: () => void) => void;
  onChildStateChange: (state: {
    isSaving: boolean;
    hasPendingChanges: boolean;
  }) => void;
}

function Naming({ setChildSave, onChildStateChange }: NamingProps) {
  const advancedSettings = useShowAdvancedSettings();
  const {
    settings,
    updateSetting,
    isFetching,
    error,
    hasSettings,
    hasPendingChanges,
    isSaving,
    saveSettings,
  } = useManageNamingSettings();

  const debouncedSettings = useDebounce(settings, 300);
  const { examples } = useNamingExamples(debouncedSettings);
  const examplesPopulated = !!examples;

  const [isNamingModalOpen, setNamingModalOpen, setNamingModalClosed] =
    useModalOpenState(false);
  const [namingModalOptions, setNamingModalOptions] =
    useState<NamingModalOptions | null>(null);

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      const key = change.name as keyof NamingSettingsModel;

      updateSetting(key, change.value as NamingSettingsModel[typeof key]);
    },
    [updateSetting]
  );

  const handleStandardNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'standardEpisodeFormat',
      season: true,
      episode: true,
      additional: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const handleDailyNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'dailyEpisodeFormat',
      season: true,
      episode: true,
      daily: true,
      additional: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const handleAnimeNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'animeEpisodeFormat',
      season: true,
      episode: true,
      anime: true,
      additional: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const handleSeriesFolderNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'seriesFolderFormat',
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const handleSeasonFolderNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'seasonFolderFormat',
      season: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const handleSpecialsFolderNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'specialsFolderFormat',
      season: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const renameEpisodes = hasSettings && settings.renameEpisodes.value;
  const replaceIllegalCharacters =
    hasSettings && settings.replaceIllegalCharacters.value;

  const multiEpisodeStyleOptions: EnhancedSelectInputValue<number>[] = [
    { key: 0, value: translate('Extend'), hint: 'S01E01-02-03' },
    { key: 1, value: translate('Duplicate'), hint: 'S01E01.S01E02' },
    { key: 2, value: translate('Repeat'), hint: 'S01E01E02E03' },
    { key: 3, value: translate('Scene'), hint: 'S01E01-E02-E03' },
    { key: 4, value: translate('Range'), hint: 'S01E01-03' },
    { key: 5, value: translate('PrefixedRange'), hint: 'S01E01-E03' },
  ];

  const colonReplacementOptions: EnhancedSelectInputValue<number>[] = [
    { key: 0, value: translate('Delete') },
    { key: 1, value: translate('ReplaceWithDash') },
    { key: 2, value: translate('ReplaceWithSpaceDash') },
    { key: 3, value: translate('ReplaceWithSpaceDashSpace') },
    {
      key: 4,
      value: translate('SmartReplace'),
      hint: translate('SmartReplaceHint'),
    },
    {
      key: 5,
      value: translate('Custom'),
      hint: translate('CustomColonReplacementFormatHint'),
    },
  ];

  const standardEpisodeFormatHelpTexts = [];
  const standardEpisodeFormatErrors = [];
  const dailyEpisodeFormatHelpTexts = [];
  const dailyEpisodeFormatErrors = [];
  const animeEpisodeFormatHelpTexts = [];
  const animeEpisodeFormatErrors = [];
  const seriesFolderFormatHelpTexts = [];
  const seriesFolderFormatErrors = [];
  const seasonFolderFormatHelpTexts = [];
  const seasonFolderFormatErrors = [];
  const specialsFolderFormatHelpTexts = [];
  const specialsFolderFormatErrors = [];

  if (examplesPopulated) {
    if (examples.singleEpisodeExample) {
      standardEpisodeFormatHelpTexts.push(
        `${translate('SingleEpisode')}: ${examples.singleEpisodeExample}`
      );
    } else {
      standardEpisodeFormatErrors.push({
        message: translate('SingleEpisodeInvalidFormat'),
      });
    }

    if (examples.multiEpisodeExample) {
      standardEpisodeFormatHelpTexts.push(
        `${translate('MultiEpisode')}: ${examples.multiEpisodeExample}`
      );
    } else {
      standardEpisodeFormatErrors.push({
        message: translate('MultiEpisodeInvalidFormat'),
      });
    }

    if (examples.dailyEpisodeExample) {
      dailyEpisodeFormatHelpTexts.push(
        `${translate('Example')}: ${examples.dailyEpisodeExample}`
      );
    } else {
      dailyEpisodeFormatErrors.push({ message: translate('InvalidFormat') });
    }

    if (examples.animeEpisodeExample) {
      animeEpisodeFormatHelpTexts.push(
        `${translate('SingleEpisode')}: ${examples.animeEpisodeExample}`
      );
    } else {
      animeEpisodeFormatErrors.push({
        message: translate('SingleEpisodeInvalidFormat'),
      });
    }

    if (examples.animeMultiEpisodeExample) {
      animeEpisodeFormatHelpTexts.push(
        `${translate('MultiEpisode')}: ${examples.animeMultiEpisodeExample}`
      );
    } else {
      animeEpisodeFormatErrors.push({
        message: translate('MultiEpisodeInvalidFormat'),
      });
    }

    if (examples.seriesFolderExample) {
      seriesFolderFormatHelpTexts.push(
        `${translate('Example')}: ${examples.seriesFolderExample}`
      );
    } else {
      seriesFolderFormatErrors.push({ message: translate('InvalidFormat') });
    }

    if (examples.seasonFolderExample) {
      seasonFolderFormatHelpTexts.push(
        `${translate('Example')}: ${examples.seasonFolderExample}`
      );
    } else {
      seasonFolderFormatErrors.push({ message: translate('InvalidFormat') });
    }

    if (examples.specialsFolderExample) {
      specialsFolderFormatHelpTexts.push(
        `${translate('Example')}: ${examples.specialsFolderExample}`
      );
    } else {
      specialsFolderFormatErrors.push({ message: translate('InvalidFormat') });
    }
  }

  useEffect(() => {
    onChildStateChange({
      hasPendingChanges,
      isSaving,
    });
  }, [hasPendingChanges, isSaving, onChildStateChange]);

  useEffect(() => {
    setChildSave(saveSettings);
  }, [setChildSave, saveSettings]);

  return (
    <FieldSet
      legend={translate('EpisodeNaming')}
      caption={translate('EpisodeNamingCaption')}
    >
      {isFetching ? <LoadingIndicator /> : null}
      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>
          {translate('NamingSettingsLoadError')}
        </Alert>
      ) : null}
      {hasSettings && !isFetching && !error ? (
        <Form>
          <FormRow size={sizes.MEDIUM}>
            <FormLabel>{translate('RenameEpisodes')}</FormLabel>
            <FormInputHelpText text={translate('RenameEpisodesHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="renameEpisodes"
              onChange={handleInputChange}
              {...settings.renameEpisodes}
            />
          </FormRow>

          <FormRow size={sizes.MEDIUM}>
            <FormLabel>{translate('ReplaceIllegalCharacters')}</FormLabel>
            <FormInputHelpText
              text={translate('ReplaceIllegalCharactersHelpText')}
            />
            <FormInput
              type={inputTypes.CHECK}
              name="replaceIllegalCharacters"
              onChange={handleInputChange}
              {...settings.replaceIllegalCharacters}
            />
          </FormRow>

          {replaceIllegalCharacters ? (
            <FormRow size={sizes.MEDIUM}>
              <FormLabel>{translate('ColonReplacement')}</FormLabel>
              <FormInputHelpText
                text={translate('ColonReplacementFormatHelpText')}
              />
              <FormInput
                type={inputTypes.SELECT}
                name="colonReplacementFormat"
                values={colonReplacementOptions}
                onChange={handleInputChange}
                {...settings.colonReplacementFormat}
              />
            </FormRow>
          ) : null}

          {replaceIllegalCharacters &&
          settings.colonReplacementFormat.value === 5 ? (
            <FormRow size={sizes.MEDIUM}>
              <FormLabel>{translate('CustomColonReplacement')}</FormLabel>
              <FormInputHelpText
                text={translate('CustomColonReplacementFormatHelpText')}
              />
              <FormInput
                type={inputTypes.TEXT}
                name="customColonReplacementFormat"
                onChange={handleInputChange}
                {...settings.customColonReplacementFormat}
              />
            </FormRow>
          ) : null}

          {renameEpisodes ? (
            <>
              <FormRow size={sizes.LARGE}>
                <FormLabel>{translate('StandardEpisodeFormat')}</FormLabel>

                {standardEpisodeFormatHelpTexts.map((text, index) => (
                  <FormInputHelpText key={index} text={text} />
                ))}

                <FormInput
                  inputClassName={styles.namingInput}
                  type={inputTypes.TEXT}
                  name="standardEpisodeFormat"
                  buttons={
                    <FormInputButton
                      onPress={handleStandardNamingModalOpenClick}
                    >
                      ?
                    </FormInputButton>
                  }
                  onChange={handleInputChange}
                  {...settings.standardEpisodeFormat}
                  errors={[
                    ...standardEpisodeFormatErrors,
                    ...settings.standardEpisodeFormat.errors,
                  ]}
                />
              </FormRow>

              <FormRow size={sizes.LARGE}>
                <FormLabel>{translate('DailyEpisodeFormat')}</FormLabel>

                {dailyEpisodeFormatHelpTexts.map((text, index) => (
                  <FormInputHelpText key={index} text={text} />
                ))}

                <FormInput
                  inputClassName={styles.namingInput}
                  type={inputTypes.TEXT}
                  name="dailyEpisodeFormat"
                  buttons={
                    <FormInputButton onPress={handleDailyNamingModalOpenClick}>
                      ?
                    </FormInputButton>
                  }
                  onChange={handleInputChange}
                  {...settings.dailyEpisodeFormat}
                  errors={[
                    ...dailyEpisodeFormatErrors,
                    ...settings.dailyEpisodeFormat.errors,
                  ]}
                />
              </FormRow>

              <FormRow size={sizes.LARGE}>
                <FormLabel>{translate('AnimeEpisodeFormat')}</FormLabel>

                {animeEpisodeFormatHelpTexts.map((text, index) => (
                  <FormInputHelpText key={index} text={text} />
                ))}

                <FormInput
                  inputClassName={styles.namingInput}
                  type={inputTypes.TEXT}
                  name="animeEpisodeFormat"
                  buttons={
                    <FormInputButton onPress={handleAnimeNamingModalOpenClick}>
                      ?
                    </FormInputButton>
                  }
                  onChange={handleInputChange}
                  {...settings.animeEpisodeFormat}
                  errors={[
                    ...animeEpisodeFormatErrors,
                    ...settings.animeEpisodeFormat.errors,
                  ]}
                />
              </FormRow>
            </>
          ) : null}

          <FormRow
            advancedSettings={advancedSettings}
            isAdvanced={true}
            size={sizes.MEDIUM}
          >
            <FormLabel>{translate('SeriesFolderFormat')}</FormLabel>

            <FormInputHelpText text={translate('SeriesFolderFormatHelpText')} />

            {seriesFolderFormatHelpTexts.map((text, index) => (
              <FormInputHelpText key={index} text={text} />
            ))}

            <FormInput
              inputClassName={styles.namingInput}
              type={inputTypes.TEXT}
              name="seriesFolderFormat"
              buttons={
                <FormInputButton
                  onPress={handleSeriesFolderNamingModalOpenClick}
                >
                  ?
                </FormInputButton>
              }
              onChange={handleInputChange}
              {...settings.seriesFolderFormat}
              errors={[
                ...seriesFolderFormatErrors,
                ...settings.seriesFolderFormat.errors,
              ]}
            />
          </FormRow>

          <FormRow size={sizes.MEDIUM}>
            <FormLabel>{translate('SeasonFolderFormat')}</FormLabel>

            {seasonFolderFormatHelpTexts.map((text, index) => (
              <FormInputHelpText key={index} text={text} />
            ))}

            <FormInput
              inputClassName={styles.namingInput}
              type={inputTypes.TEXT}
              name="seasonFolderFormat"
              buttons={
                <FormInputButton
                  onPress={handleSeasonFolderNamingModalOpenClick}
                >
                  ?
                </FormInputButton>
              }
              onChange={handleInputChange}
              {...settings.seasonFolderFormat}
              errors={[
                ...seasonFolderFormatErrors,
                ...settings.seasonFolderFormat.errors,
              ]}
            />
          </FormRow>

          <FormRow
            advancedSettings={advancedSettings}
            isAdvanced={true}
            size={sizes.MEDIUM}
          >
            <FormLabel>{translate('SpecialsFolderFormat')}</FormLabel>

            {specialsFolderFormatHelpTexts.map((text, index) => (
              <FormInputHelpText key={index} text={text} />
            ))}

            <FormInput
              inputClassName={styles.namingInput}
              type={inputTypes.TEXT}
              name="specialsFolderFormat"
              buttons={
                <FormInputButton
                  onPress={handleSpecialsFolderNamingModalOpenClick}
                >
                  ?
                </FormInputButton>
              }
              onChange={handleInputChange}
              {...settings.specialsFolderFormat}
              errors={[
                ...specialsFolderFormatErrors,
                ...settings.specialsFolderFormat.errors,
              ]}
            />
          </FormRow>

          <FormRow size={sizes.MEDIUM}>
            <FormLabel>{translate('MultiEpisodeStyle')}</FormLabel>

            <FormInput
              type={inputTypes.SELECT}
              name="multiEpisodeStyle"
              values={multiEpisodeStyleOptions}
              onChange={handleInputChange}
              {...settings.multiEpisodeStyle}
            />
          </FormRow>

          {namingModalOptions ? (
            <NamingModal
              isOpen={isNamingModalOpen}
              {...namingModalOptions}
              value={settings[namingModalOptions.name].value}
              onInputChange={handleInputChange}
              onModalClose={setNamingModalClosed}
            />
          ) : null}
        </Form>
      ) : null}
    </FieldSet>
  );
}

export default Naming;
