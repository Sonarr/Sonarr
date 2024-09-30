import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  fetchNamingExamples,
  fetchNamingSettings,
  setNamingSettingsValue,
} from 'Store/Actions/settingsActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import NamingConfig from 'typings/Settings/NamingConfig';
import translate from 'Utilities/String/translate';
import NamingModal from './NamingModal';
import styles from './Naming.css';

const SECTION = 'naming';

function createNamingSelector() {
  return createSelector(
    (state: AppState) => state.settings.advancedSettings,
    (state: AppState) => state.settings.namingExamples,
    createSettingsSectionSelector(SECTION),
    (advancedSettings, namingExamples, sectionSettings) => {
      return {
        advancedSettings,
        examples: namingExamples.item,
        examplesPopulated: namingExamples.isPopulated,
        ...sectionSettings,
      };
    }
  );
}

interface NamingModalOptions {
  name: keyof Pick<
    NamingConfig,
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

function Naming() {
  const {
    advancedSettings,
    isFetching,
    error,
    settings,
    hasSettings,
    examples,
    examplesPopulated,
  } = useSelector(createNamingSelector());

  const dispatch = useDispatch();

  const [isNamingModalOpen, setNamingModalOpen, setNamingModalClosed] =
    useModalOpenState(false);
  const [namingModalOptions, setNamingModalOptions] =
    useState<NamingModalOptions | null>(null);
  const namingExampleTimeout = useRef<ReturnType<typeof setTimeout>>();

  useEffect(() => {
    dispatch(fetchNamingSettings());
    dispatch(fetchNamingExamples());

    return () => {
      dispatch(clearPendingChanges({ section: SECTION }));
    };
  }, [dispatch]);

  const handleInputChange = useCallback(
    ({ name, value }: { name: string; value: string }) => {
      // @ts-expect-error 'setNamingSettingsValue' isn't typed yet
      dispatch(setNamingSettingsValue({ name, value }));

      if (namingExampleTimeout.current) {
        clearTimeout(namingExampleTimeout.current);
      }

      namingExampleTimeout.current = setTimeout(() => {
        dispatch(fetchNamingExamples());
      }, 1000);
    },
    [dispatch]
  );

  const onStandardNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'standardEpisodeFormat',
      season: true,
      episode: true,
      additional: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const onDailyNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'dailyEpisodeFormat',
      season: true,
      episode: true,
      daily: true,
      additional: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const onAnimeNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'animeEpisodeFormat',
      season: true,
      episode: true,
      anime: true,
      additional: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const onSeriesFolderNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'seriesFolderFormat',
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const onSeasonFolderNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'seasonFolderFormat',
      season: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const onSpecialsFolderNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'specialsFolderFormat',
      season: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const renameEpisodes = hasSettings && settings.renameEpisodes.value;
  const replaceIllegalCharacters =
    hasSettings && settings.replaceIllegalCharacters.value;

  const multiEpisodeStyleOptions = [
    { key: 0, value: translate('Extend'), hint: 'S01E01-02-03' },
    { key: 1, value: translate('Duplicate'), hint: 'S01E01.S01E02' },
    { key: 2, value: translate('Repeat'), hint: 'S01E01E02E03' },
    { key: 3, value: translate('Scene'), hint: 'S01E01-E02-E03' },
    { key: 4, value: translate('Range'), hint: 'S01E01-03' },
    { key: 5, value: translate('PrefixedRange'), hint: 'S01E01-E03' },
  ];

  const colonReplacementOptions = [
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

  return (
    <FieldSet legend={translate('EpisodeNaming')}>
      {isFetching ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>
          {translate('NamingSettingsLoadError')}
        </Alert>
      ) : null}

      {hasSettings && !isFetching && !error ? (
        <Form>
          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('RenameEpisodes')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="renameEpisodes"
              helpText={translate('RenameEpisodesHelpText')}
              onChange={handleInputChange}
              {...settings.renameEpisodes}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('ReplaceIllegalCharacters')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="replaceIllegalCharacters"
              helpText={translate('ReplaceIllegalCharactersHelpText')}
              onChange={handleInputChange}
              {...settings.replaceIllegalCharacters}
            />
          </FormGroup>

          {replaceIllegalCharacters ? (
            <FormGroup size={sizes.MEDIUM}>
              <FormLabel>{translate('ColonReplacement')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="colonReplacementFormat"
                values={colonReplacementOptions}
                helpText={translate('ColonReplacementFormatHelpText')}
                onChange={handleInputChange}
                {...settings.colonReplacementFormat}
              />
            </FormGroup>
          ) : null}

          {replaceIllegalCharacters &&
          settings.colonReplacementFormat.value === 5 ? (
            <FormGroup size={sizes.MEDIUM}>
              <FormLabel>{translate('CustomColonReplacement')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="customColonReplacementFormat"
                helpText={translate('CustomColonReplacementFormatHelpText')}
                onChange={handleInputChange}
                {...settings.customColonReplacementFormat}
              />
            </FormGroup>
          ) : null}

          {renameEpisodes ? (
            <>
              <FormGroup size={sizes.LARGE}>
                <FormLabel>{translate('StandardEpisodeFormat')}</FormLabel>

                <FormInputGroup
                  inputClassName={styles.namingInput}
                  type={inputTypes.TEXT}
                  name="standardEpisodeFormat"
                  buttons={
                    <FormInputButton onPress={onStandardNamingModalOpenClick}>
                      ?
                    </FormInputButton>
                  }
                  onChange={handleInputChange}
                  {...settings.standardEpisodeFormat}
                  helpTexts={standardEpisodeFormatHelpTexts}
                  errors={[
                    ...standardEpisodeFormatErrors,
                    ...settings.standardEpisodeFormat.errors,
                  ]}
                />
              </FormGroup>

              <FormGroup size={sizes.LARGE}>
                <FormLabel>{translate('DailyEpisodeFormat')}</FormLabel>

                <FormInputGroup
                  inputClassName={styles.namingInput}
                  type={inputTypes.TEXT}
                  name="dailyEpisodeFormat"
                  buttons={
                    <FormInputButton onPress={onDailyNamingModalOpenClick}>
                      ?
                    </FormInputButton>
                  }
                  onChange={handleInputChange}
                  {...settings.dailyEpisodeFormat}
                  helpTexts={dailyEpisodeFormatHelpTexts}
                  errors={[
                    ...dailyEpisodeFormatErrors,
                    ...settings.dailyEpisodeFormat.errors,
                  ]}
                />
              </FormGroup>

              <FormGroup size={sizes.LARGE}>
                <FormLabel>{translate('AnimeEpisodeFormat')}</FormLabel>

                <FormInputGroup
                  inputClassName={styles.namingInput}
                  type={inputTypes.TEXT}
                  name="animeEpisodeFormat"
                  buttons={
                    <FormInputButton onPress={onAnimeNamingModalOpenClick}>
                      ?
                    </FormInputButton>
                  }
                  onChange={handleInputChange}
                  {...settings.animeEpisodeFormat}
                  helpTexts={animeEpisodeFormatHelpTexts}
                  errors={[
                    ...animeEpisodeFormatErrors,
                    ...settings.animeEpisodeFormat.errors,
                  ]}
                />
              </FormGroup>
            </>
          ) : null}

          <FormGroup
            advancedSettings={advancedSettings}
            isAdvanced={true}
            size={sizes.MEDIUM}
          >
            <FormLabel>{translate('SeriesFolderFormat')}</FormLabel>

            <FormInputGroup
              inputClassName={styles.namingInput}
              type={inputTypes.TEXT}
              name="seriesFolderFormat"
              buttons={
                <FormInputButton onPress={onSeriesFolderNamingModalOpenClick}>
                  ?
                </FormInputButton>
              }
              onChange={handleInputChange}
              {...settings.seriesFolderFormat}
              helpTexts={[
                translate('SeriesFolderFormatHelpText'),
                ...seriesFolderFormatHelpTexts,
              ]}
              errors={[
                ...seriesFolderFormatErrors,
                ...settings.seriesFolderFormat.errors,
              ]}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('SeasonFolderFormat')}</FormLabel>

            <FormInputGroup
              inputClassName={styles.namingInput}
              type={inputTypes.TEXT}
              name="seasonFolderFormat"
              buttons={
                <FormInputButton onPress={onSeasonFolderNamingModalOpenClick}>
                  ?
                </FormInputButton>
              }
              onChange={handleInputChange}
              {...settings.seasonFolderFormat}
              helpTexts={seasonFolderFormatHelpTexts}
              errors={[
                ...seasonFolderFormatErrors,
                ...settings.seasonFolderFormat.errors,
              ]}
            />
          </FormGroup>

          <FormGroup
            advancedSettings={advancedSettings}
            isAdvanced={true}
            size={sizes.MEDIUM}
          >
            <FormLabel>{translate('SpecialsFolderFormat')}</FormLabel>

            <FormInputGroup
              inputClassName={styles.namingInput}
              type={inputTypes.TEXT}
              name="specialsFolderFormat"
              buttons={
                <FormInputButton onPress={onSpecialsFolderNamingModalOpenClick}>
                  ?
                </FormInputButton>
              }
              onChange={handleInputChange}
              {...settings.specialsFolderFormat}
              helpTexts={specialsFolderFormatHelpTexts}
              errors={[
                ...specialsFolderFormatErrors,
                ...settings.specialsFolderFormat.errors,
              ]}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('MultiEpisodeStyle')}</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="multiEpisodeStyle"
              values={multiEpisodeStyleOptions}
              onChange={handleInputChange}
              {...settings.multiEpisodeStyle}
            />
          </FormGroup>

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
