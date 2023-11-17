import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditRemotePathMappingModalConnector from './EditRemotePathMappingModalConnector';
import RemotePathMapping from './RemotePathMapping';
import styles from './RemotePathMappings.css';

class RemotePathMappings extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddRemotePathMappingModalOpen: false
    };
  }

  //
  // Listeners

  onAddRemotePathMappingPress = () => {
    this.setState({ isAddRemotePathMappingModalOpen: true });
  };

  onModalClose = () => {
    this.setState({ isAddRemotePathMappingModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      items,
      onConfirmDeleteRemotePathMapping,
      ...otherProps
    } = this.props;

    return (
      <FieldSet legend={translate('RemotePathMappings')} >
        <PageSectionContent
          errorMessage={translate('RemotePathMappingsLoadError')}
          {...otherProps}
        >

          <Alert kind={kinds.INFO}>
            <InlineMarkdown data={translate('RemotePathMappingsInfo', { wikiLink: 'https://wiki.servarr.com/sonarr/settings#remote-path-mappings' })} />
          </Alert>

          <div className={styles.remotePathMappingsHeader}>
            <div className={styles.host}>
              {translate('Host')}
            </div>
            <div className={styles.path}>
              {translate('RemotePath')}
            </div>
            <div className={styles.path}>
              {translate('LocalPath')}
            </div>
          </div>

          <div>
            {
              items.map((item, index) => {
                return (
                  <RemotePathMapping
                    key={item.id}
                    {...item}
                    {...otherProps}
                    index={index}
                    onConfirmDeleteRemotePathMapping={onConfirmDeleteRemotePathMapping}
                  />
                );
              })
            }
          </div>

          <div className={styles.addRemotePathMapping}>
            <Link
              className={styles.addButton}
              onPress={this.onAddRemotePathMappingPress}
            >
              <Icon name={icons.ADD} />
            </Link>
          </div>

          <EditRemotePathMappingModalConnector
            isOpen={this.state.isAddRemotePathMappingModalOpen}
            onModalClose={this.onModalClose}
          />
        </PageSectionContent>
      </FieldSet>
    );
  }
}

RemotePathMappings.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteRemotePathMapping: PropTypes.func.isRequired
};

export default RemotePathMappings;
