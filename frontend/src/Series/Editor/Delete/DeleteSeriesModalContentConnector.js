import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import { bulkDeleteSeries } from 'Store/Actions/seriesEditorActions';
import DeleteSeriesModalContent from './DeleteSeriesModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { seriesIds }) => seriesIds,
    createAllSeriesSelector(),
    (seriesIds, allSeries) => {
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
        series
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onDeleteSelectedPress(deleteFiles) {
      dispatch(bulkDeleteSeries({
        seriesIds: props.seriesIds,
        deleteFiles
      }));

      props.onModalClose();
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(DeleteSeriesModalContent);
