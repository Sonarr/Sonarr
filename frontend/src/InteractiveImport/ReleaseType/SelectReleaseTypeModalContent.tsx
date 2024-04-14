import React, { useCallback, useState } from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import ReleaseType from 'InteractiveImport/ReleaseType';
import translate from 'Utilities/String/translate';

const options = [
  {
    key: 'unknown',
    get value() {
      return translate('Unknown');
    },
  },
  {
    key: 'singleEpisode',
    get value() {
      return translate('SingleEpisode');
    },
  },
  {
    key: 'multiEpisode',
    get value() {
      return translate('MultiEpisode');
    },
  },
  {
    key: 'seasonPack',
    get value() {
      return translate('SeasonPack');
    },
  },
];

interface SelectReleaseTypeModalContentProps {
  releaseType: ReleaseType;
  modalTitle: string;
  onReleaseTypeSelect(releaseType: ReleaseType): void;
  onModalClose(): void;
}

function SelectReleaseTypeModalContent(
  props: SelectReleaseTypeModalContentProps
) {
  const { modalTitle, onReleaseTypeSelect, onModalClose } = props;
  const [releaseType, setReleaseType] = useState(props.releaseType);

  const handleReleaseTypeChange = useCallback(
    ({ value }: { value: string }) => {
      setReleaseType(value as ReleaseType);
    },
    [setReleaseType]
  );

  const handleReleaseTypeSelect = useCallback(() => {
    onReleaseTypeSelect(releaseType);
  }, [releaseType, onReleaseTypeSelect]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {modalTitle} - {translate('SelectReleaseType')}
      </ModalHeader>

      <ModalBody>
        <Form>
          <FormGroup>
            <FormLabel>{translate('ReleaseType')}</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="releaseType"
              value={releaseType}
              values={options}
              onChange={handleReleaseTypeChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button kind={kinds.SUCCESS} onPress={handleReleaseTypeSelect}>
          {translate('SelectReleaseType')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectReleaseTypeModalContent;
