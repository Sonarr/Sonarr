/* eslint max-params: 0 */
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import getQualities from 'Utilities/Quality/getQualities';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import { deleteEpisodeFiles, updateEpisodeFiles } from 'Store/Actions/episodeFileActions';
import { fetchLanguageProfileSchema, fetchQualityProfileSchema } from 'Store/Actions/settingsActions';
import EpisodeFileEditorModalContent from './EpisodeFileEditorModalContent';

function createSchemaSelector() {
  return createSelector(
    (state) => state.settings.languageProfiles,
    (state) => state.settings.qualityProfiles,
    (languageProfiles, qualityProfiles) => {
      const languages = _.map(languageProfiles.schema.languages, 'language');
      const qualities = getQualities(qualityProfiles.schema.items);

      let error = null;

      if (languageProfiles.schemaError) {
        error = 'Unable to load languages';
      } else if (qualityProfiles.schemaError) {
        error = 'Unable to load qualities';
      }

      return {
        isFetching: languageProfiles.isSchemaFetching || qualityProfiles.isSchemaFetching,
        isPopulated: languageProfiles.isSchemaPopulated && qualityProfiles.isSchemaPopulated,
        error,
        languages,
        qualities
      };
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state, { seasonNumber }) => seasonNumber,
    (state) => state.episodes,
    (state) => state.episodeFiles,
    createSchemaSelector(),
    createSeriesSelector(),
    (
      seasonNumber,
      episodes,
      episodeFiles,
      schema,
      series
    ) => {
      const filtered = _.filter(episodes.items, (episode) => {
        if (seasonNumber >= 0 && episode.seasonNumber !== seasonNumber) {
          return false;
        }

        if (!episode.episodeFileId) {
          return false;
        }

        return _.some(episodeFiles.items, { id: episode.episodeFileId });
      });

      const sorted = _.orderBy(filtered, ['seasonNumber', 'episodeNumber'], ['desc', 'desc']);

      const items = _.map(sorted, (episode) => {
        const episodeFile = _.find(episodeFiles.items, { id: episode.episodeFileId });

        return {
          relativePath: episodeFile.relativePath,
          language: episodeFile.language,
          quality: episodeFile.quality,
          languageCutoffNotMet: episodeFile.languageCutoffNotMet,
          qualityCutoffNotMet: episodeFile.qualityCutoffNotMet,
          ...episode
        };
      });

      return {
        ...schema,
        items,
        seriesType: series.seriesType,
        isDeleting: episodeFiles.isDeleting,
        isSaving: episodeFiles.isSaving
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchFetchLanguageProfileSchema(name, path) {
      dispatch(fetchLanguageProfileSchema());
    },

    dispatchFetchQualityProfileSchema(name, path) {
      dispatch(fetchQualityProfileSchema());
    },

    dispatchUpdateEpisodeFiles(updateProps) {
      dispatch(updateEpisodeFiles(updateProps));
    },

    onDeletePress(episodeFileIds) {
      dispatch(deleteEpisodeFiles({ episodeFileIds }));
    }
  };
}

class EpisodeFileEditorModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchLanguageProfileSchema();
    this.props.dispatchFetchQualityProfileSchema();
  }

  //
  // Listeners

  onLanguageChange = (episodeFileIds, languageId) => {
    const language = _.find(this.props.languages, { id: languageId });

    this.props.dispatchUpdateEpisodeFiles({ episodeFileIds, language });
  }

  onQualityChange = (episodeFileIds, qualityId) => {
    const quality = {
      quality: _.find(this.props.qualities, { id: qualityId }),
      revision: {
        version: 1,
        real: 0
      }
    };

    this.props.dispatchUpdateEpisodeFiles({ episodeFileIds, quality });
  }

  //
  // Render

  render() {
    const {
      dispatchFetchLanguageProfileSchema,
      dispatchFetchQualityProfileSchema,
      dispatchUpdateEpisodeFiles,
      ...otherProps
    } = this.props;

    return (
      <EpisodeFileEditorModalContent
        {...otherProps}
        onLanguageChange={this.onLanguageChange}
        onQualityChange={this.onQualityChange}
      />
    );
  }
}

EpisodeFileEditorModalContentConnector.propTypes = {
  seriesId: PropTypes.number.isRequired,
  seasonNumber: PropTypes.number,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  qualities: PropTypes.arrayOf(PropTypes.object).isRequired,
  dispatchFetchLanguageProfileSchema: PropTypes.func.isRequired,
  dispatchFetchQualityProfileSchema: PropTypes.func.isRequired,
  dispatchUpdateEpisodeFiles: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(EpisodeFileEditorModalContentConnector);
