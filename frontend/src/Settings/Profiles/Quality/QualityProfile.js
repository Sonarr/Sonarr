import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import Tooltip from 'Components/Tooltip/Tooltip';
import EditQualityProfileModalConnector from './EditQualityProfileModalConnector';
import styles from './QualityProfile.css';

class QualityProfile extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditQualityProfileModalOpen: false,
      isDeleteQualityProfileModalOpen: false
    };
  }

  //
  // Listeners

  onEditQualityProfilePress = () => {
    this.setState({ isEditQualityProfileModalOpen: true });
  }

  onEditQualityProfileModalClose = () => {
    this.setState({ isEditQualityProfileModalOpen: false });
  }

  onDeleteQualityProfilePress = () => {
    this.setState({
      isEditQualityProfileModalOpen: false,
      isDeleteQualityProfileModalOpen: true
    });
  }

  onDeleteQualityProfileModalClose = () => {
    this.setState({ isDeleteQualityProfileModalOpen: false });
  }

  onConfirmDeleteQualityProfile = () => {
    this.props.onConfirmDeleteQualityProfile(this.props.id);
  }

  onCloneQualityProfilePress = () => {
    const {
      id,
      onCloneQualityProfilePress
    } = this.props;

    onCloneQualityProfilePress(id);
  }

  //
  // Render

  render() {
    const {
      id,
      name,
      cutoff,
      items,
      isDeleting
    } = this.props;

    return (
      <Card
        className={styles.qualityProfile}
        overlayContent={true}
        onPress={this.onEditQualityProfilePress}
      >
        <div className={styles.nameContainer}>
          <div className={styles.name}>
            {name}
          </div>

          <IconButton
            className={styles.cloneButton}
            title="Clone Profile"
            name={icons.CLONE}
            onPress={this.onCloneQualityProfilePress}
          />
        </div>

        <div className={styles.qualities}>
          {
            items.map((item) => {
              if (!item.allowed) {
                return null;
              }

              if (item.quality) {
                const isCutoff = item.quality.id === cutoff;

                return (
                  <Label
                    key={item.quality.id}
                    kind={isCutoff ? kinds.INFO : kinds.default}
                    title={isCutoff ? 'Cutoff' : null}
                  >
                    {item.quality.name}
                  </Label>
                );
              }

              const isCutoff = item.id === cutoff;

              return (
                <Tooltip
                  key={item.id}
                  className={styles.tooltipLabel}
                  anchor={
                    <Label
                      kind={isCutoff ? kinds.INFO : kinds.default}
                      title={isCutoff ? 'Cutoff' : null}
                    >
                      {item.name}
                    </Label>
                  }
                  tooltip={
                    <div>
                      {
                        item.items.map((groupItem) => {
                          return (
                            <Label
                              key={groupItem.quality.id}
                              kind={isCutoff ? kinds.INFO : kinds.default}
                              title={isCutoff ? 'Cutoff' : null}
                            >
                              {groupItem.quality.name}
                            </Label>
                          );
                        })
                      }
                    </div>
                  }
                  kind={kinds.INVERSE}
                  position={tooltipPositions.TOP}
                />
              );
            })
          }
        </div>

        <EditQualityProfileModalConnector
          id={id}
          isOpen={this.state.isEditQualityProfileModalOpen}
          onModalClose={this.onEditQualityProfileModalClose}
          onDeleteQualityProfilePress={this.onDeleteQualityProfilePress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteQualityProfileModalOpen}
          kind={kinds.DANGER}
          title="Delete Quality Profile"
          message={`Are you sure you want to delete the quality profile '${name}'?`}
          confirmLabel="Delete"
          isSpinning={isDeleting}
          onConfirm={this.onConfirmDeleteQualityProfile}
          onCancel={this.onDeleteQualityProfileModalClose}
        />
      </Card>
    );
  }
}

QualityProfile.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  cutoff: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDeleting: PropTypes.bool.isRequired,
  onConfirmDeleteQualityProfile: PropTypes.func.isRequired,
  onCloneQualityProfilePress: PropTypes.func.isRequired
};

export default QualityProfile;
