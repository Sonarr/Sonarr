import React from 'react';
import { useSelector } from 'react-redux';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import { kinds } from 'Helpers/Props';
import createEnabledDownloadClientsSelector from 'Store/Selectors/createEnabledDownloadClientsSelector';
import translate from 'Utilities/String/translate';
import SelectDownloadClientRow from './SelectDownloadClientRow';

interface SelectDownloadClientModalContentProps {
  protocol: DownloadProtocol;
  modalTitle: string;
  onDownloadClientSelect(downloadClientId: number): void;
  onModalClose(): void;
}

function SelectDownloadClientModalContent(
  props: SelectDownloadClientModalContentProps
) {
  const { modalTitle, protocol, onDownloadClientSelect, onModalClose } = props;

  const { isFetching, isPopulated, error, items } = useSelector(
    createEnabledDownloadClientsSelector(protocol)
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('SelectDownloadClientModalTitle', { modalTitle })}
      </ModalHeader>

      <ModalBody>
        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && error ? (
          <Alert kind={kinds.DANGER}>
            {translate('DownloadClientsLoadError')}
          </Alert>
        ) : null}

        {isPopulated && !error ? (
          <Form>
            {items.map((downloadClient) => {
              const { id, name, priority } = downloadClient;

              return (
                <SelectDownloadClientRow
                  key={id}
                  id={id}
                  name={name}
                  priority={priority}
                  onDownloadClientSelect={onDownloadClientSelect}
                />
              );
            })}
          </Form>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectDownloadClientModalContent;
