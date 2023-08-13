import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import { Error } from 'App/State/AppSectionState';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import Quality, { QualityModel } from 'Quality/Quality';
import { fetchQualityProfileSchema } from 'Store/Actions/settingsActions';
import { CheckInputChanged } from 'typings/inputs';
import getQualities from 'Utilities/Quality/getQualities';
import translate from 'Utilities/String/translate';

interface QualitySchemaState {
  isFetching: boolean;
  isPopulated: boolean;
  error: Error;
  items: Quality[];
}

function createQualitySchemaSelector() {
  return createSelector(
    (state: AppState) => state.settings.qualityProfiles,
    (qualityProfiles): QualitySchemaState => {
      const { isSchemaFetching, isSchemaPopulated, schemaError, schema } =
        qualityProfiles;

      const items = getQualities(schema.items) as Quality[];

      return {
        isFetching: isSchemaFetching,
        isPopulated: isSchemaPopulated,
        error: schemaError,
        items,
      };
    }
  );
}

interface SelectQualityModalContentProps {
  qualityId: number;
  proper: boolean;
  real: boolean;
  modalTitle: string;
  onQualitySelect(quality: QualityModel): void;
  onModalClose(): void;
}

function SelectQualityModalContent(props: SelectQualityModalContentProps) {
  const { modalTitle, onQualitySelect, onModalClose } = props;

  const [qualityId, setQualityId] = useState(props.qualityId);
  const [proper, setProper] = useState(props.proper);
  const [real, setReal] = useState(props.real);

  const { isFetching, isPopulated, error, items } = useSelector(
    createQualitySchemaSelector()
  );
  const dispatch = useDispatch();

  useEffect(
    () => {
      dispatch(fetchQualityProfileSchema());
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  const qualityOptions = useMemo(() => {
    return items.map(({ id, name }) => {
      return {
        key: id,
        value: name,
      };
    });
  }, [items]);

  const onQualityChange = useCallback(
    ({ value }: { value: string }) => {
      setQualityId(parseInt(value));
    },
    [setQualityId]
  );

  const onProperChange = useCallback(
    ({ value }: CheckInputChanged) => {
      setProper(value);
    },
    [setProper]
  );

  const onRealChange = useCallback(
    ({ value }: CheckInputChanged) => {
      setReal(value);
    },
    [setReal]
  );

  const onQualitySelectWrapper = useCallback(() => {
    const quality = items.find((item) => item.id === qualityId) as Quality;

    const revision = {
      version: proper ? 2 : 1,
      real: real ? 1 : 0,
      isRepack: false,
    };

    onQualitySelect({
      quality,
      revision,
    });
  }, [items, qualityId, proper, real, onQualitySelect]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{modalTitle} - Select Quality</ModalHeader>

      <ModalBody>
        {isFetching && <LoadingIndicator />}

        {!isFetching && error ? (
          <Alert kind={kinds.DANGER}>{translate('QualitiesLoadError')}</Alert>
        ) : null}

        {isPopulated && !error ? (
          <Form>
            <FormGroup>
              <FormLabel>{translate('Quality')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="quality"
                value={qualityId}
                values={qualityOptions}
                onChange={onQualityChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('Proper')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="proper"
                value={proper}
                onChange={onProperChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('Real')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="real"
                value={real}
                onChange={onRealChange}
              />
            </FormGroup>
          </Form>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>Cancel</Button>

        <Button kind={kinds.SUCCESS} onPress={onQualitySelectWrapper}>
          {translate('SelectQuality')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectQualityModalContent;
