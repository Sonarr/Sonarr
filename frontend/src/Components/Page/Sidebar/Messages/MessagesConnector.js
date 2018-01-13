import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import Messages from './Messages';

function createMapStateToProps() {
  return createSelector(
    (state) => state.app.messages.items,
    (messages) => {
      return {
        messages: messages.slice().reverse()
      };
    }
  );
}

export default connect(createMapStateToProps)(Messages);
