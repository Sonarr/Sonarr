import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes, sizes } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FormInputButton from 'Components/Form/FormInputButton';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import NamingModal from './NamingModal';
import styles from './Naming.css';

class Naming extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isNamingModalOpen: false,
      namingModalOptions: null
    };
  }

  //
  // Listeners

  onStandardNamingModalOpenClick = () => {
    this.setState({
      isNamingModalOpen: true,
      namingModalOptions: {
        name: 'standardEpisodeFormat',
        season: true,
        episode: true,
        additional: true
      }
    });
  }

  onDailyNamingModalOpenClick = () => {
    this.setState({
      isNamingModalOpen: true,
      namingModalOptions: {
        name: 'dailyEpisodeFormat',
        season: true,
        episode: true,
        daily: true,
        additional: true
      }
    });
  }

  onAnimeNamingModalOpenClick = () => {
    this.setState({
      isNamingModalOpen: true,
      namingModalOptions: {
        name: 'animeEpisodeFormat',
        season: true,
        episode: true,
        anime: true,
        additional: true
      }
    });
  }

  onSeriesFolderNamingModalOpenClick = () => {
    this.setState({
      isNamingModalOpen: true,
      namingModalOptions: {
        name: 'seriesFolderFormat'
      }
    });
  }

  onSeasonFolderNamingModalOpenClick = () => {
    this.setState({
      isNamingModalOpen: true,
      namingModalOptions: {
        name: 'seasonFolderFormat',
        season: true
      }
    });
  }

  onNamingModalClose = () => {
    this.setState({ isNamingModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      advancedSettings,
      isFetching,
      error,
      settings,
      hasSettings,
      examples,
      examplesPopulated,
      onInputChange
    } = this.props;

    const {
      isNamingModalOpen,
      namingModalOptions
    } = this.state;

    const renameEpisodes = hasSettings && settings.renameEpisodes.value;

    const multiEpisodeStyleOptions = [
      { key: 0, value: 'Extend' },
      { key: 1, value: 'Duplicate' },
      { key: 2, value: 'Repeat' },
      { key: 3, value: 'Scene' },
      { key: 4, value: 'Range' },
      { key: 5, value: 'Prefixed Range' }
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

    if (examplesPopulated) {
      if (examples.singleEpisodeExample) {
        standardEpisodeFormatHelpTexts.push(`Single Episode: ${examples.singleEpisodeExample}`);
      } else {
        standardEpisodeFormatErrors.push('Single Episode: Invalid Format');
      }

      if (examples.multiEpisodeExample) {
        standardEpisodeFormatHelpTexts.push(`Multi Episode: ${examples.multiEpisodeExample}`);
      } else {
        standardEpisodeFormatErrors.push('Multi Episode: Invalid Format');
      }

      if (examples.dailyEpisodeExample) {
        dailyEpisodeFormatHelpTexts.push(`Example: ${examples.dailyEpisodeExample}`);
      } else {
        dailyEpisodeFormatErrors.push('Invalid Format');
      }

      if (examples.animeEpisodeExample) {
        animeEpisodeFormatHelpTexts.push(`Single Episode: ${examples.animeEpisodeExample}`);
      } else {
        animeEpisodeFormatErrors.push('Single Episode: Invalid Format');
      }

      if (examples.animeMultiEpisodeExample) {
        animeEpisodeFormatHelpTexts.push(`Multi Episode: ${examples.animeMultiEpisodeExample}`);
      } else {
        animeEpisodeFormatErrors.push('Multi Episode: Invalid Format');
      }

      if (examples.seriesFolderExample) {
        seriesFolderFormatHelpTexts.push(`Example: ${examples.seriesFolderExample}`);
      } else {
        seriesFolderFormatErrors.push('Invalid Format');
      }

      if (examples.seasonFolderExample) {
        seasonFolderFormatHelpTexts.push(`Example: ${examples.seasonFolderExample}`);
      } else {
        seasonFolderFormatErrors.push('Invalid Format');
      }
    }

    return (
      <FieldSet
        legend="Episode Naming"
      >
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && error &&
            <div>Unable to load Naming settings</div>
        }

        {
          hasSettings && !isFetching && !error &&
            <Form>
              <FormGroup size={sizes.MEDIUM}>
                <FormLabel>Rename Episodes</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="renameEpisodes"
                  helpText="Sonarr will use the existing file name if renaming is disabled"
                  onChange={onInputChange}
                  {...settings.renameEpisodes}
                />
              </FormGroup>

              <FormGroup size={sizes.MEDIUM}>
                <FormLabel>Replace Illegal Characters</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="replaceIllegalCharacters"
                  helpText="Replace or Remove illegal characters"
                  onChange={onInputChange}
                  {...settings.replaceIllegalCharacters}
                />
              </FormGroup>

              {
                renameEpisodes &&
                  <div>
                    <FormGroup size={sizes.LARGE}>
                      <FormLabel>Standard Episode Format</FormLabel>

                      <FormInputGroup
                        inputClassName={styles.namingInput}
                        type={inputTypes.TEXT}
                        name="standardEpisodeFormat"
                        buttons={<FormInputButton onPress={this.onStandardNamingModalOpenClick}>?</FormInputButton>}
                        onChange={onInputChange}
                        {...settings.standardEpisodeFormat}
                        helpTexts={standardEpisodeFormatHelpTexts}
                        errors={[...standardEpisodeFormatErrors, ...settings.standardEpisodeFormat.errors]}
                      />
                    </FormGroup>

                    <FormGroup size={sizes.LARGE}>
                      <FormLabel>Daily Episode Format</FormLabel>

                      <FormInputGroup
                        inputClassName={styles.namingInput}
                        type={inputTypes.TEXT}
                        name="dailyEpisodeFormat"
                        buttons={<FormInputButton onPress={this.onDailyNamingModalOpenClick}>?</FormInputButton>}
                        onChange={onInputChange}
                        {...settings.dailyEpisodeFormat}
                        helpTexts={dailyEpisodeFormatHelpTexts}
                        errors={[...dailyEpisodeFormatErrors, ...settings.dailyEpisodeFormat.errors]}
                      />
                    </FormGroup>

                    <FormGroup size={sizes.LARGE}>
                      <FormLabel>Anime Episode Format</FormLabel>

                      <FormInputGroup
                        inputClassName={styles.namingInput}
                        type={inputTypes.TEXT}
                        name="animeEpisodeFormat"
                        buttons={<FormInputButton onPress={this.onAnimeNamingModalOpenClick}>?</FormInputButton>}
                        onChange={onInputChange}
                        {...settings.animeEpisodeFormat}
                        helpTexts={animeEpisodeFormatHelpTexts}
                        errors={[...animeEpisodeFormatErrors, ...settings.animeEpisodeFormat.errors]}
                      />
                    </FormGroup>
                  </div>
              }

              <FormGroup
                advancedSettings={advancedSettings}
                isAdvanced={true}
              >
                <FormLabel>Series Folder Format</FormLabel>

                <FormInputGroup
                  inputClassName={styles.namingInput}
                  type={inputTypes.TEXT}
                  name="seriesFolderFormat"
                  buttons={<FormInputButton onPress={this.onSeriesFolderNamingModalOpenClick}>?</FormInputButton>}
                  onChange={onInputChange}
                  {...settings.seriesFolderFormat}
                  helpTexts={['Only used when adding a new series', ...seriesFolderFormatHelpTexts]}
                  errors={[...seriesFolderFormatErrors, ...settings.seriesFolderFormat.errors]}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Season Folder Format</FormLabel>

                <FormInputGroup
                  inputClassName={styles.namingInput}
                  type={inputTypes.TEXT}
                  name="seasonFolderFormat"
                  buttons={<FormInputButton onPress={this.onSeasonFolderNamingModalOpenClick}>?</FormInputButton>}
                  onChange={onInputChange}
                  {...settings.seasonFolderFormat}
                  helpTexts={seasonFolderFormatHelpTexts}
                  errors={[...seasonFolderFormatErrors, ...settings.seasonFolderFormat.errors]}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Multi-Episode Style</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="multiEpisodeStyle"
                  helpText="Change file date on import/rescan"
                  values={multiEpisodeStyleOptions}
                  onChange={onInputChange}
                  {...settings.multiEpisodeStyle}
                />
              </FormGroup>

              {
                namingModalOptions &&
                  <NamingModal
                    isOpen={isNamingModalOpen}
                    advancedSettings={advancedSettings}
                    {...namingModalOptions}
                    value={settings[namingModalOptions.name].value}
                    onInputChange={onInputChange}
                    onModalClose={this.onNamingModalClose}
                  />
              }
            </Form>
        }
      </FieldSet>
    );
  }

}

Naming.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  settings: PropTypes.object.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  examples: PropTypes.object.isRequired,
  examplesPopulated: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default Naming;
