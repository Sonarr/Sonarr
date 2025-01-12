import React, { useCallback, useState } from 'react';
import { useSelector } from 'react-redux';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { inputTypes } from 'Helpers/Props';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';

export interface RootFolderUpdated {
  path: string;
  rootFolderPath: string;
}

export interface RootFolderModalContentProps {
  seriesId: number;
  rootFolderPath: string;
  onSavePress(change: RootFolderUpdated): void;
  onModalClose(): void;
}

interface SeriesFolder {
  folder: string;
}

function RootFolderModalContent(props: RootFolderModalContentProps) {
  const { seriesId, onSavePress, onModalClose } = props;
  const { isWindows } = useSelector(createSystemStatusSelector());

  const [rootFolderPath, setRootFolderPath] = useState(props.rootFolderPath);

  const { isLoading, data } = useApiQuery<SeriesFolder>({
    path: `/series/${seriesId}/folder`,
  });

  const onInputChange = useCallback(({ value }: InputChanged<string>) => {
    setRootFolderPath(value);
  }, []);

  const handleSavePress = useCallback(() => {
    const separator = isWindows ? '\\' : '/';

    onSavePress({
      path: `${rootFolderPath}${separator}${data?.folder}`,
      rootFolderPath,
    });
  }, [rootFolderPath, isWindows, data, onSavePress]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('UpdateSeriesPath')}</ModalHeader>

      <ModalBody>
        <FormGroup>
          <FormLabel>{translate('RootFolder')}</FormLabel>

          <FormInputGroup
            type={inputTypes.ROOT_FOLDER_SELECT}
            name="rootFolderPath"
            value={rootFolderPath}
            valueOptions={{
              seriesFolder: data?.folder,
              isWindows,
            }}
            selectedValueOptions={{
              seriesFolder: data?.folder,
              isWindows,
            }}
            helpText={translate('SeriesEditRootFolderHelpText')}
            onChange={onInputChange}
          />
        </FormGroup>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button disabled={isLoading || !data?.folder} onPress={handleSavePress}>
          {translate('UpdatePath')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default RootFolderModalContent;
