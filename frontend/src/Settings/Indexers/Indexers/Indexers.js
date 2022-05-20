import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import AddIndexerModal from './AddIndexerModal';
import EditIndexerModalConnector from './EditIndexerModalConnector';
import Indexer from './Indexer';
import styles from './Indexers.css';

class Indexers extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddIndexerModalOpen: false,
      isEditIndexerModalOpen: false
    };
  }

  //
  // Listeners

  onAddIndexerPress = () => {
    this.setState({ isAddIndexerModalOpen: true });
  };

  onCloneIndexerPress = (id) => {
    this.props.dispatchCloneIndexer({ id });
    this.setState({ isEditIndexerModalOpen: true });
  };

  onAddIndexerModalClose = ({ indexerSelected = false } = {}) => {
    this.setState({
      isAddIndexerModalOpen: false,
      isEditIndexerModalOpen: indexerSelected
    });
  };

  onEditIndexerModalClose = () => {
    this.setState({ isEditIndexerModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      items,
      tagList,
      dispatchCloneIndexer,
      onConfirmDeleteIndexer,
      ...otherProps
    } = this.props;

    const {
      isAddIndexerModalOpen,
      isEditIndexerModalOpen
    } = this.state;

    const showPriority = items.some((index) => index.priority !== 25);

    return (
      <FieldSet legend="Indexers">
        <PageSectionContent
          errorMessage="Unable to load Indexers"
          {...otherProps}
        >
          <div className={styles.indexers}>
            {
              items.map((item) => {
                return (
                  <Indexer
                    key={item.id}
                    {...item}
                    tagList={tagList}
                    showPriority={showPriority}
                    onCloneIndexerPress={this.onCloneIndexerPress}
                    onConfirmDeleteIndexer={onConfirmDeleteIndexer}
                  />
                );
              })
            }

            <Card
              className={styles.addIndexer}
              onPress={this.onAddIndexerPress}
            >
              <div className={styles.center}>
                <Icon
                  name={icons.ADD}
                  size={45}
                />
              </div>
            </Card>
          </div>

          <AddIndexerModal
            isOpen={isAddIndexerModalOpen}
            onModalClose={this.onAddIndexerModalClose}
          />

          <EditIndexerModalConnector
            isOpen={isEditIndexerModalOpen}
            onModalClose={this.onEditIndexerModalClose}
          />
        </PageSectionContent>
      </FieldSet>
    );
  }
}

Indexers.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  dispatchCloneIndexer: PropTypes.func.isRequired,
  onConfirmDeleteIndexer: PropTypes.func.isRequired
};

export default Indexers;
