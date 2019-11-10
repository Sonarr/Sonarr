import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import PageSectionContent from 'Components/Page/PageSectionContent';
import ImportListExclusion from './ImportListExclusion';
import EditImportListExclusionModalConnector from './EditImportListExclusionModalConnector';
import styles from './ImportListExclusions.css';

class ImportListExclusions extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddImportListExclusionModalOpen: false
    };
  }

  //
  // Listeners

  onAddImportListExclusionPress = () => {
    this.setState({ isAddImportListExclusionModalOpen: true });
  }

  onModalClose = () => {
    this.setState({ isAddImportListExclusionModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      items,
      onConfirmDeleteImportListExclusion,
      ...otherProps
    } = this.props;

    return (
      <FieldSet legend="Import List Exclusions">
        <PageSectionContent
          errorMessage="Unable to load Import List Exclusions"
          {...otherProps}
        >
          <div className={styles.importListExclusionsHeader}>
            <div className={styles.host}>Title</div>
            <div className={styles.path}>TVDB ID</div>
          </div>

          <div>
            {
              items.map((item, index) => {
                return (
                  <ImportListExclusion
                    key={item.id}
                    {...item}
                    {...otherProps}
                    index={index}
                    onConfirmDeleteImportListExclusion={onConfirmDeleteImportListExclusion}
                  />
                );
              })
            }
          </div>

          <div className={styles.addImportListExclusion}>
            <Link
              className={styles.addButton}
              onPress={this.onAddImportListExclusionPress}
            >
              <Icon name={icons.ADD} />
            </Link>
          </div>

          <EditImportListExclusionModalConnector
            isOpen={this.state.isAddImportListExclusionModalOpen}
            onModalClose={this.onModalClose}
          />

        </PageSectionContent>
      </FieldSet>
    );
  }
}

ImportListExclusions.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteImportListExclusion: PropTypes.func.isRequired
};

export default ImportListExclusions;
