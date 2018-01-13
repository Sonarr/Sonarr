import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds } from 'Helpers/Props';
import Card from 'Components/Card';
import Label from 'Components/Label';
import EditMetadataModalConnector from './EditMetadataModalConnector';
import styles from './Metadata.css';

function getKind(enable) {
  if (enable) {
    return kinds.SUCCESS;
  }

  return kinds.DANGER;
}

class Metadata extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditMetadataModalOpen: false
    };
  }

  //
  // Listeners

  onEditMetadataPress = () => {
    this.setState({ isEditMetadataModalOpen: true });
  }

  onEditMetadataModalClose = () => {
    this.setState({ isEditMetadataModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      id,
      name,
      enable,
      fields
    } = this.props;

    return (
      <Card
        className={styles.metadata}
        overlayContent={true}
        onPress={this.onEditMetadataPress}
      >
        <div className={styles.name}>
          {name}
        </div>

        <div>
          <Label
            kind={getKind(enable)}
            outline={!enable}
          >
            Enable
          </Label>
        </div>

        <div>
          {
            fields.map((field) => {
              return (
                <Label
                  key={field.label}
                  kind={enable ? getKind(field.value) : kinds.DEFAULT}
                  outline={enable && !field.value}
                >
                  {field.label}
                </Label>
              );
            })
          }
        </div>

        <EditMetadataModalConnector
          id={id}
          isOpen={this.state.isEditMetadataModalOpen}
          onModalClose={this.onEditMetadataModalClose}
        />
      </Card>
    );
  }
}

Metadata.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  enable: PropTypes.bool.isRequired,
  fields: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default Metadata;
