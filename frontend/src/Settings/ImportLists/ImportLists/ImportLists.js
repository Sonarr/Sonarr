import PropTypes from 'prop-types';
import React, { Component } from 'react';
import sortByName from 'Utilities/Array/sortByName';
import { icons } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import Card from 'Components/Card';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import ImportList from './ImportList';
import AddImportListModal from './AddImportListModal';
import EditImportListModalConnector from './EditImportListModalConnector';
import styles from './ImportLists.css';

class ImportLists extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddImportListModalOpen: false,
      isEditImportListModalOpen: false
    };
  }

  //
  // Listeners

  onAddImportListPress = () => {
    this.setState({ isAddImportListModalOpen: true });
  }

  onAddImportListModalClose = ({ listSelected = false } = {}) => {
    this.setState({
      isAddImportListModalOpen: false,
      isEditImportListModalOpen: listSelected
    });
  }

  onEditImportListModalClose = () => {
    this.setState({ isEditImportListModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      items,
      onConfirmDeleteImportList,
      ...otherProps
    } = this.props;

    const {
      isAddImportListModalOpen,
      isEditImportListModalOpen
    } = this.state;

    return (
      <FieldSet
        legend="Import Lists"
      >
        <PageSectionContent
          errorMessage="Unable to load Lists"
          {...otherProps}
        >
          <div className={styles.lists}>
            {
              items.sort(sortByName).map((item) => {
                return (
                  <ImportList
                    key={item.id}
                    {...item}
                    onConfirmDeleteImportList={onConfirmDeleteImportList}
                  />
                );
              })
            }

            <Card
              className={styles.addList}
              onPress={this.onAddImportListPress}
            >
              <div className={styles.center}>
                <Icon
                  name={icons.ADD}
                  size={45}
                />
              </div>
            </Card>
          </div>

          <AddImportListModal
            isOpen={isAddImportListModalOpen}
            onModalClose={this.onAddImportListModalClose}
          />

          <EditImportListModalConnector
            isOpen={isEditImportListModalOpen}
            onModalClose={this.onEditImportListModalClose}
          />
        </PageSectionContent>
      </FieldSet>
    );
  }
}

ImportLists.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteImportList: PropTypes.func.isRequired
};

export default ImportLists;
