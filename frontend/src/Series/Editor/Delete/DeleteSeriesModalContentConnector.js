import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setDeleteOption } from 'Store/Actions/seriesActions';
import { bulkDeleteSeries } from 'Store/Actions/seriesEditorActions';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import DeleteSeriesModalContent from './DeleteSeriesModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { seriesIds }) => seriesIds,
    (state) => state.series.deleteOptions,
    createAllSeriesSelector(),
    (seriesIds, deleteOptions, allSeries) => {
      const selectedSeries = _.intersectionWith(allSeries, seriesIds, (s, id) => {
        return s.id === id;
      });

      const sortedSeries = _.orderBy(selectedSeries, 'sortTitle');
      const series = _.map(sortedSeries, (s) => {
        return {
          title: s.title,
          path: s.path
        };
      });

      return {
        series,
        deleteOptions
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    setDeleteOption(option) {
      dispatch(
        setDeleteOption({
          [option.name]: option.value
        })
      );
    },

    onDeleteSelectedPress(deleteFiles, addImportListExclusion) {
      dispatch(
        bulkDeleteSeries({
          seriesIds: props.seriesIds,
          deleteFiles,
          addImportListExclusion
        })
      );

      props.onModalClose();
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(DeleteSeriesModalContent);
