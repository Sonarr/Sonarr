import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { setQualityDefinitionValue } from 'Store/Actions/settingsActions';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import QualityDefinition from './QualityDefinition';

function mapStateToProps(state) {
  return {
    advancedSettings: state.settings.advancedSettings
  };
}

const mapDispatchToProps = {
  setQualityDefinitionValue,
  clearPendingChanges
};

class QualityDefinitionConnector extends Component {

  componentWillUnmount() {
    this.props.clearPendingChanges({ section: 'settings.qualityDefinitions' });
  }

  //
  // Listeners

  onTitleChange = ({ value }) => {
    this.props.setQualityDefinitionValue({ id: this.props.id, name: 'title', value });
  }

  onSizeChange = ({ minSize, maxSize }) => {
    const {
      id,
      minSize: currentMinSize,
      maxSize: currentMaxSize
    } = this.props;

    if (minSize !== currentMinSize) {
      this.props.setQualityDefinitionValue({ id, name: 'minSize', value: minSize });
    }

    if (minSize !== currentMaxSize) {
      this.props.setQualityDefinitionValue({ id, name: 'maxSize', value: maxSize });
    }
  }

  //
  // Render

  render() {
    return (
      <QualityDefinition
        {...this.props}
        onTitleChange={this.onTitleChange}
        onSizeChange={this.onSizeChange}
      />
    );
  }
}

QualityDefinitionConnector.propTypes = {
  id: PropTypes.number.isRequired,
  minSize: PropTypes.number,
  maxSize: PropTypes.number,
  setQualityDefinitionValue: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(mapStateToProps, mapDispatchToProps)(QualityDefinitionConnector);
