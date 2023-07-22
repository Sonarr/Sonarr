import PropTypes from 'prop-types';
import React from 'react';
import FieldSet from 'Components/FieldSet';
import Label from 'Components/Label';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import TagDetailsDelayProfile from './TagDetailsDelayProfile';
import styles from './TagDetailsModalContent.css';

function TagDetailsModalContent(props) {
  const {
    label,
    isTagUsed,
    series,
    delayProfiles,
    importLists,
    notifications,
    releaseProfiles,
    indexers,
    downloadClients,
    autoTags,
    onModalClose,
    onDeleteTagPress
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('TagDetails', { label })}
      </ModalHeader>

      <ModalBody>
        {
          !isTagUsed &&
            <div>
              {translate('TagIsNotUsedAndCanBeDeleted')}
            </div>
        }

        {
          series.length ?
            <FieldSet legend={translate('Series')}>
              {
                series.map((item) => {
                  return (
                    <div key={item.id}>
                      {item.title}
                    </div>
                  );
                })
              }
            </FieldSet> :
            null
        }

        {
          delayProfiles.length ?
            <FieldSet legend={translate('DelayProfile')}>
              {
                delayProfiles.map((item) => {
                  const {
                    id,
                    preferredProtocol,
                    enableUsenet,
                    enableTorrent,
                    usenetDelay,
                    torrentDelay
                  } = item;

                  return (
                    <TagDetailsDelayProfile
                      key={id}
                      preferredProtocol={preferredProtocol}
                      enableUsenet={enableUsenet}
                      enableTorrent={enableTorrent}
                      usenetDelay={usenetDelay}
                      torrentDelay={torrentDelay}
                    />
                  );
                })
              }
            </FieldSet> :
            null
        }

        {
          notifications.length ?
            <FieldSet legend={translate('Connections')}>
              {
                notifications.map((item) => {
                  return (
                    <div key={item.id}>
                      {item.name}
                    </div>
                  );
                })
              }
            </FieldSet> :
            null
        }

        {
          importLists.length ?
            <FieldSet legend={translate('ImportLists')}>
              {
                importLists.map((item) => {
                  return (
                    <div key={item.id}>
                      {item.name}
                    </div>
                  );
                })
              }
            </FieldSet> :
            null
        }

        {
          releaseProfiles.length ?
            <FieldSet legend={translate('ReleaseProfiles')}>
              {
                releaseProfiles.map((item) => {
                  return (
                    <div
                      key={item.id}
                      className={styles.restriction}
                    >
                      <div>
                        {
                          item.required.map((r) => {
                            return (
                              <Label
                                key={r}
                                kind={kinds.SUCCESS}
                              >
                                {r}
                              </Label>
                            );
                          })
                        }
                      </div>

                      <div>
                        {
                          item.ignored.map((i) => {
                            return (
                              <Label
                                key={i}
                                kind={kinds.DANGER}
                              >
                                {i}
                              </Label>
                            );
                          })
                        }
                      </div>
                    </div>
                  );
                })
              }
            </FieldSet> :
            null
        }

        {
          indexers.length ?
            <FieldSet legend={translate('Indexers')}>
              {
                indexers.map((item) => {
                  return (
                    <div key={item.id}>
                      {item.name}
                    </div>
                  );
                })
              }
            </FieldSet> :
            null
        }

        {
          downloadClients.length ?
            <FieldSet legend={translate('DownloadClients')}>
              {
                downloadClients.map((item) => {
                  return (
                    <div key={item.id}>
                      {item.name}
                    </div>
                  );
                })
              }
            </FieldSet> :
            null
        }

        {
          autoTags.length ?
            <FieldSet legend={translate('AutoTagging')}>
              {
                autoTags.map((item) => {
                  return (
                    <div key={item.id}>
                      {item.name}
                    </div>
                  );
                })
              }
            </FieldSet> :
            null
        }
      </ModalBody>

      <ModalFooter>
        {
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            title={isTagUsed ? translate('TagCannotBeDeletedWhileInUse') : undefined}
            isDisabled={isTagUsed}
            onPress={onDeleteTagPress}
          >
            {translate('Delete')}
          </Button>
        }

        <Button
          onPress={onModalClose}
        >
          {translate('Close')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

TagDetailsModalContent.propTypes = {
  label: PropTypes.string.isRequired,
  isTagUsed: PropTypes.bool.isRequired,
  series: PropTypes.arrayOf(PropTypes.object).isRequired,
  delayProfiles: PropTypes.arrayOf(PropTypes.object).isRequired,
  importLists: PropTypes.arrayOf(PropTypes.object).isRequired,
  notifications: PropTypes.arrayOf(PropTypes.object).isRequired,
  releaseProfiles: PropTypes.arrayOf(PropTypes.object).isRequired,
  indexers: PropTypes.arrayOf(PropTypes.object).isRequired,
  downloadClients: PropTypes.arrayOf(PropTypes.object).isRequired,
  autoTags: PropTypes.arrayOf(PropTypes.object).isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteTagPress: PropTypes.func.isRequired
};

export default TagDetailsModalContent;
