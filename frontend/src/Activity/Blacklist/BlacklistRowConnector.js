import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { removeFromBlacklist } from 'Store/Actions/blacklistActions';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import BlacklistRow from './BlacklistRow';

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
      dispatch(removeFromBlacklist({ id: props.id }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(BlacklistRow);
