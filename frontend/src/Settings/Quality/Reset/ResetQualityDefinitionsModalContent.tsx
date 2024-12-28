import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import * as commandNames from 'Commands/commandNames';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './ResetQualityDefinitionsModalContent.css';

interface ResetQualityDefinitionsModalContentProps {
  onModalClose: () => void;
}

function ResetQualityDefinitionsModalContent({
  onModalClose,
}: ResetQualityDefinitionsModalContentProps) {
  const dispatch = useDispatch();

  const isResettingQualityDefinitions = useSelector(
    createCommandExecutingSelector(commandNames.RESET_QUALITY_DEFINITIONS)
  );

  const [resetDefinitionTitles, setResetDefinitionTitles] = useState(false);

  const handleResetDefinitionTitlesChange = useCallback(
    ({ value }: InputChanged<boolean>) => {
      setResetDefinitionTitles(value);
    },
    []
  );

  const handleResetQualityDefinitionsConfirmed = useCallback(() => {
    const resetTitles = resetDefinitionTitles;

    setResetDefinitionTitles(false);

    dispatch(
      executeCommand({
        name: commandNames.RESET_QUALITY_DEFINITIONS,
        resetTitles,
      })
    );
    onModalClose();
  }, [resetDefinitionTitles, dispatch, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('ResetQualityDefinitions')}</ModalHeader>

      <ModalBody>
        <div className={styles.messageContainer}>
          {translate('ResetQualityDefinitionsMessageText')}
        </div>

        <FormGroup>
          <FormLabel>{translate('ResetTitles')}</FormLabel>

          <FormInputGroup
            type={inputTypes.CHECK}
            name="resetDefinitionTitles"
            value={resetDefinitionTitles}
            helpText={translate('ResetDefinitionTitlesHelpText')}
            onChange={handleResetDefinitionTitlesChange}
          />
        </FormGroup>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button
          kind={kinds.DANGER}
          isDisabled={isResettingQualityDefinitions}
          onPress={handleResetQualityDefinitionsConfirmed}
        >
          {translate('Reset')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default ResetQualityDefinitionsModalContent;
