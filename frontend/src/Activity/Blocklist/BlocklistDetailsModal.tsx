import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import translate from 'Utilities/String/translate';

interface BlocklistDetailsModalProps {
  isOpen: boolean;
  sourceTitle: string;
  protocol: DownloadProtocol;
  indexer?: string;
  message?: string;
  source?: string;
  onModalClose: () => void;
}

function BlocklistDetailsModal({
  isOpen,
  sourceTitle,
  protocol,
  indexer,
  message,
  source,
  onModalClose,
}: BlocklistDetailsModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>Details</ModalHeader>

        <ModalBody>
          <DescriptionList>
            <DescriptionListItem title={translate('Name')} data={sourceTitle} />

            <DescriptionListItem
              title={translate('Protocol')}
              data={protocol}
            />

            {message ? (
              <DescriptionListItem
                title={translate('Indexer')}
                data={indexer}
              />
            ) : null}

            {message ? (
              <DescriptionListItem
                title={translate('Message')}
                data={message}
              />
            ) : null}
            {source ? (
              <DescriptionListItem title={translate('Source')} data={source} />
            ) : null}
          </DescriptionList>
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>{translate('Close')}</Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

export default BlocklistDetailsModal;
