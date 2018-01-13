import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchDelayProfiles, deleteDelayProfile, reorderDelayProfile } from 'Store/Actions/settingsActions';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import DelayProfiles from './DelayProfiles';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.delayProfiles,
    createTagsSelector(),
    (delayProfiles, tagList) => {
      const defaultProfile = _.find(delayProfiles.items, { id: 1 });
      const items = _.sortBy(_.reject(delayProfiles.items, { id: 1 }), ['order']);

      return {
        defaultProfile,
        ...delayProfiles,
        items,
        tagList
      };
    }
  );
}

const mapDispatchToProps = {
  fetchDelayProfiles,
  deleteDelayProfile,
  reorderDelayProfile
};

class DelayProfilesConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      dragIndex: null,
      dropIndex: null
    };
  }

  componentDidMount() {
    this.props.fetchDelayProfiles();
  }

  //
  // Listeners

  onConfirmDeleteDelayProfile = (id) => {
    this.props.deleteDelayProfile({ id });
  }

  onDelayProfileDragMove = (dragIndex, dropIndex) => {
    if (this.state.dragIndex !== dragIndex || this.state.dropIndex !== dropIndex) {
      this.setState({
        dragIndex,
        dropIndex
      });
    }
  }

  onDelayProfileDragEnd = ({ id }, didDrop) => {
    const {
      dropIndex
    } = this.state;

    if (didDrop && dropIndex !== null) {
      this.props.reorderDelayProfile({ id, moveIndex: dropIndex - 1 });
    }

    this.setState({
      dragIndex: null,
      dropIndex: null
    });
  }

  //
  // Render

  render() {
    return (
      <DelayProfiles
        {...this.state}
        {...this.props}
        onConfirmDeleteDelayProfile={this.onConfirmDeleteDelayProfile}
        onDelayProfileDragMove={this.onDelayProfileDragMove}
        onDelayProfileDragEnd={this.onDelayProfileDragEnd}
      />
    );
  }
}

DelayProfilesConnector.propTypes = {
  fetchDelayProfiles: PropTypes.func.isRequired,
  deleteDelayProfile: PropTypes.func.isRequired,
  reorderDelayProfile: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(DelayProfilesConnector);
