import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setQueueOption } from 'Store/Actions/queueActions';
import QueueOptions from './QueueOptions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.queue.options,
    (options) => {
      return options;
    }
  );
}

const mapDispatchToProps = {
  onOptionChange: setQueueOption
};

export default connect(createMapStateToProps, mapDispatchToProps)(QueueOptions);
