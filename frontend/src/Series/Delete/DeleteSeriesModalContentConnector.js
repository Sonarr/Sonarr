import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteSeries, setDeleteOption } from 'Store/Actions/seriesActions';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import DeleteSeriesModalContent from './DeleteSeriesModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.series.deleteOptions,
    createSeriesSelector(),
    (deleteOptions, series) => {
      return {
        ...series,
        deleteOptions
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onDeleteOptionChange(option) {
      dispatch(
        setDeleteOption({
          [option.name]: option.value
        })
      );
    },

    onDeletePress(deleteFiles, addImportListExclusion) {
      dispatch(
        deleteSeries({
          id: props.seriesId,
          deleteFiles,
          addImportListExclusion
        })
      );

      props.onModalClose(true);
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(DeleteSeriesModalContent);
