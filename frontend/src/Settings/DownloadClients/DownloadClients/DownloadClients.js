import PropTypes from 'prop-types';
import React, { Component } from 'react';
import sortByName from 'Utilities/Array/sortByName';
import { icons } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import Card from 'Components/Card';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import DownloadClient from './DownloadClient';
import AddDownloadClientModal from './AddDownloadClientModal';
import EditDownloadClientModalConnector from './EditDownloadClientModalConnector';
import styles from './DownloadClients.css';

class DownloadClients extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddDownloadClientModalOpen: false,
      isEditDownloadClientModalOpen: false
    };
  }

  //
  // Listeners

  onAddDownloadClientPress = () => {
    this.setState({ isAddDownloadClientModalOpen: true });
  }

  onAddDownloadClientModalClose = ({ downloadClientSelected = false } = {}) => {
    this.setState({
      isAddDownloadClientModalOpen: false,
      isEditDownloadClientModalOpen: downloadClientSelected
    });
  }

  onEditDownloadClientModalClose = () => {
    this.setState({ isEditDownloadClientModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      items,
      onConfirmDeleteDownloadClient,
      ...otherProps
    } = this.props;

    const {
      isAddDownloadClientModalOpen,
      isEditDownloadClientModalOpen
    } = this.state;

    return (
      <FieldSet
        legend="Download Clients"
      >
        <PageSectionContent
          errorMessage="Unable to load download clients"
          {...otherProps}
        >
          <div className={styles.downloadClients}>
            {
              items.sort(sortByName).map((item) => {
                return (
                  <DownloadClient
                    key={item.id}
                    {...item}
                    onConfirmDeleteDownloadClient={onConfirmDeleteDownloadClient}
                  />
                );
              })
            }

            <Card
              className={styles.addDownloadClient}
              onPress={this.onAddDownloadClientPress}
            >
              <div className={styles.center}>
                <Icon
                  name={icons.ADD}
                  size={45}
                />
              </div>
            </Card>
          </div>

          <AddDownloadClientModal
            isOpen={isAddDownloadClientModalOpen}
            onModalClose={this.onAddDownloadClientModalClose}
          />

          <EditDownloadClientModalConnector
            isOpen={isEditDownloadClientModalOpen}
            onModalClose={this.onEditDownloadClientModalClose}
          />
        </PageSectionContent>
      </FieldSet>
    );
  }
}

DownloadClients.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteDownloadClient: PropTypes.func.isRequired
};

export default DownloadClients;
