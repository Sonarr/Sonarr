import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { removeBlocklistItem } from 'Store/Actions/blocklistActions';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import BlocklistRow from './BlocklistRow';

function createMapStateToProps() {
  return createSelector(
    createSeriesSelector(),
    (series) => {
      return {
        series
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onRemovePress() {
      dispatch(removeBlocklistItem({ id: props.id }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(BlocklistRow);
