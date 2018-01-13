import PropTypes from 'prop-types';
import React from 'react';
import split from 'Utilities/String/split';
import { kinds } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import Button from 'Components/Link/Button';
import Label from 'Components/Label';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import styles from './TagDetailsModalContent.css';

function TagDetailsModalContent(props) {
  const {
    label,
    isTagUsed,
    series,
    delayProfiles,
    notifications,
    restrictions,
    onModalClose,
    onDeleteTagPress
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Tag Details - {label}
      </ModalHeader>

      <ModalBody>
        {
          !isTagUsed &&
            <div>Tag is not used and can be deleted</div>
        }

        {
          !!series.length &&
            <FieldSet legend="Series">
              {
                series.map((item) => {
                  return (
                    <div key={item.id}>
                      {item.title}
                    </div>
                  );
                })
              }
            </FieldSet>
        }

        {
          !!delayProfiles.length &&
            <FieldSet legend="Delay Profiles">
              {
                delayProfiles.map((item) => {
                  return (
                    <div key={item.id}>
                      {item.name}
                    </div>
                  );
                })
              }
            </FieldSet>
        }

        {
          !!notifications.length &&
            <FieldSet legend="Connections">
              {
                notifications.map((item) => {
                  return (
                    <div key={item.id}>
                      {item.name}
                    </div>
                  );
                })
              }
            </FieldSet>
        }

        {
          !!restrictions.length &&
            <FieldSet legend="Restrictions">
              {
                restrictions.map((item) => {
                  return (
                    <div
                      key={item.id}
                      className={styles.restriction}
                    >
                      <div>
                        {
                          split(item.required).map((r) => {
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
                          split(item.ignored).map((i) => {
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
            </FieldSet>
        }
      </ModalBody>

      <ModalFooter>
        {
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            isDisabled={isTagUsed}
            onPress={onDeleteTagPress}
          >
              Delete
          </Button>
        }

        <Button
          onPress={onModalClose}
        >
          Close
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
  notifications: PropTypes.arrayOf(PropTypes.object).isRequired,
  restrictions: PropTypes.arrayOf(PropTypes.object).isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteTagPress: PropTypes.func.isRequired
};

export default TagDetailsModalContent;
