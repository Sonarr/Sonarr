import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import EditMetadataModalConnector from './EditMetadataModalConnector';
import styles from './Metadata.css';

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
  };

  onEditMetadataModalClose = () => {
    this.setState({ isEditMetadataModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      id,
      name,
      enable,
      fields
    } = this.props;

    const metadataFields = [];
    const imageFields = [];

    fields.forEach((field) => {
      if (field.section === 'metadata') {
        metadataFields.push(field);
      } else {
        imageFields.push(field);
      }
    });

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
          {
            enable ?
              <Label kind={kinds.SUCCESS}>
                Enabled
              </Label> :
              <Label
                kind={kinds.DISABLED}
                outline={true}
              >
                Disabled
              </Label>
          }
        </div>

        {
          enable && !!metadataFields.length &&
            <div>
              <div className={styles.section}>
                Metadata
              </div>

              {
                metadataFields.map((field) => {
                  if (!field.value) {
                    return null;
                  }

                  return (
                    <Label
                      key={field.label}
                      kind={kinds.SUCCESS}
                    >
                      {field.label}
                    </Label>
                  );
                })
              }
            </div>
        }

        {
          enable && !!imageFields.length &&
            <div>
              <div className={styles.section}>
                Images
              </div>

              {
                imageFields.map((field) => {
                  if (!field.value) {
                    return null;
                  }

                  return (
                    <Label
                      key={field.label}
                      kind={kinds.SUCCESS}
                    >
                      {field.label}
                    </Label>
                  );
                })
              }
            </div>
        }

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
