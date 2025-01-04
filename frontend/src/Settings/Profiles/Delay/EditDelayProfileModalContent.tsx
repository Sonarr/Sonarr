import React, { useCallback, useEffect, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
import {
  saveDelayProfile,
  setDelayProfileValue,
} from 'Store/Actions/settingsActions';
import selectSettings from 'Store/Selectors/selectSettings';
import DelayProfile from 'typings/DelayProfile';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './EditDelayProfileModalContent.css';

const newDelayProfile: DelayProfile & { [key: string]: unknown } = {
  id: 0,
  name: '',
  order: 0,
  enableUsenet: true,
  enableTorrent: true,
  preferredProtocol: 'usenet',
  usenetDelay: 0,
  torrentDelay: 0,
  bypassIfHighestQuality: false,
  bypassIfAboveCustomFormatScore: false,
  minimumCustomFormatScore: 0,
  tags: [],
};

const protocolOptions = [
  {
    key: 'preferUsenet',
    get value() {
      return translate('PreferUsenet');
    },
  },
  {
    key: 'preferTorrent',
    get value() {
      return translate('PreferTorrent');
    },
  },
  {
    key: 'onlyUsenet',
    get value() {
      return translate('OnlyUsenet');
    },
  },
  {
    key: 'onlyTorrent',
    get value() {
      return translate('OnlyTorrent');
    },
  },
];

function createDelayProfileSelector(id: number | undefined) {
  return createSelector(
    (state: AppState) => state.settings.delayProfiles,
    (delayProfiles) => {
      const { isFetching, error, isSaving, saveError, pendingChanges, items } =
        delayProfiles;

      const profile = id ? items.find((i) => i.id === id) : newDelayProfile;
      const settings = selectSettings<DelayProfile>(
        profile!,
        pendingChanges,
        saveError
      );

      return {
        isFetching,
        error,
        isSaving,
        saveError,
        item: settings.settings,
        ...settings,
      };
    }
  );
}

export interface EditDelayProfileModalContentProps {
  id?: number;
  onDeleteDelayProfilePress?: () => void;
  onModalClose: () => void;
}

function EditDelayProfileModalContent({
  id,
  onModalClose,
  onDeleteDelayProfilePress,
  ...otherProps
}: EditDelayProfileModalContentProps) {
  const dispatch = useDispatch();

  const { item, isFetching, error, isSaving, saveError } = useSelector(
    createDelayProfileSelector(id)
  );

  const {
    enableUsenet,
    enableTorrent,
    preferredProtocol,
    usenetDelay,
    torrentDelay,
    bypassIfHighestQuality,
    bypassIfAboveCustomFormatScore,
    minimumCustomFormatScore,
    tags,
  } = item;

  const protocol = useMemo(() => {
    if (!enableUsenet.value) {
      return 'onlyTorrent';
    } else if (!enableTorrent.value) {
      return 'onlyUsenet';
    }

    return preferredProtocol.value === 'usenet'
      ? 'preferUsenet'
      : 'preferTorrent';
  }, [enableUsenet, enableTorrent, preferredProtocol]);

  const wasSaving = usePrevious(isSaving);

  const onInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setDelayProfileValue({ name, value }));
    },
    [dispatch]
  );

  const onProtocolChange = useCallback(
    ({ value }: InputChanged) => {
      let enableUsenet = false;
      let enableTorrent = false;
      let preferredProtocol: 'usenet' | 'torrent' = 'usenet';

      switch (value) {
        case 'preferUsenet':
          enableUsenet = true;
          enableTorrent = true;
          preferredProtocol = 'usenet';

          break;
        case 'preferTorrent':
          enableUsenet = true;
          enableTorrent = true;
          preferredProtocol = 'torrent';

          break;
        case 'onlyUsenet':
          enableUsenet = true;
          enableTorrent = false;
          preferredProtocol = 'usenet';

          break;
        case 'onlyTorrent':
          enableUsenet = false;
          enableTorrent = true;
          preferredProtocol = 'torrent';

          break;
        default:
          throw Error(`Unknown protocol option: ${value}`);
      }

      dispatch(
        // @ts-expect-error - actions are not typed
        setDelayProfileValue({ name: 'enableUsenet', value: enableUsenet })
      );
      dispatch(
        // @ts-expect-error - actions are not typed
        setDelayProfileValue({
          name: 'enableTorrent',
          value: enableTorrent,
        })
      );
      dispatch(
        // @ts-expect-error - actions are not typed
        setDelayProfileValue({
          name: 'preferredProtocol',
          value: preferredProtocol,
        })
      );
    },
    [dispatch]
  );

  const handleSavePress = useCallback(() => {
    dispatch(saveDelayProfile({ id }));
  }, [id, dispatch]);

  useEffect(() => {
    if (!id) {
      Object.keys(newDelayProfile).forEach((name) => {
        if (name === 'id') {
          return;
        }

        dispatch(
          // @ts-expect-error - actions are not typed
          setDelayProfileValue({
            name,
            value: newDelayProfile[name],
          })
        );
      });
    }
  }, [id, dispatch]);

  useEffect(() => {
    if (wasSaving && !isSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? translate('EditDelayProfile') : translate('AddDelayProfile')}
      </ModalHeader>

      <ModalBody>
        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && !!error ? (
          <Alert kind={kinds.DANGER}>{translate('AddDelayProfileError')}</Alert>
        ) : null}

        {!isFetching && !error ? (
          <Form {...otherProps}>
            <FormGroup>
              <FormLabel>{translate('PreferredProtocol')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="protocol"
                value={protocol}
                values={protocolOptions}
                helpText={translate('ProtocolHelpText')}
                onChange={onProtocolChange}
              />
            </FormGroup>

            {enableUsenet.value ? (
              <FormGroup>
                <FormLabel>{translate('UsenetDelay')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.NUMBER}
                  name="usenetDelay"
                  unit="minutes"
                  {...usenetDelay}
                  helpText={translate('UsenetDelayHelpText')}
                  onChange={onInputChange}
                />
              </FormGroup>
            ) : null}

            {enableTorrent.value ? (
              <FormGroup>
                <FormLabel>{translate('TorrentDelay')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.NUMBER}
                  name="torrentDelay"
                  unit="minutes"
                  {...torrentDelay}
                  helpText={translate('TorrentDelayHelpText')}
                  onChange={onInputChange}
                />
              </FormGroup>
            ) : null}

            <FormGroup>
              <FormLabel>{translate('BypassDelayIfHighestQuality')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="bypassIfHighestQuality"
                {...bypassIfHighestQuality}
                helpText={translate('BypassDelayIfHighestQualityHelpText')}
                onChange={onInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>
                {translate('BypassDelayIfAboveCustomFormatScore')}
              </FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="bypassIfAboveCustomFormatScore"
                {...bypassIfAboveCustomFormatScore}
                helpText={translate(
                  'BypassDelayIfAboveCustomFormatScoreHelpText'
                )}
                onChange={onInputChange}
              />
            </FormGroup>

            {bypassIfAboveCustomFormatScore.value ? (
              <FormGroup>
                <FormLabel>
                  {translate('BypassDelayIfAboveCustomFormatScoreMinimumScore')}
                </FormLabel>

                <FormInputGroup
                  type={inputTypes.NUMBER}
                  name="minimumCustomFormatScore"
                  {...minimumCustomFormatScore}
                  helpText={translate(
                    'BypassDelayIfAboveCustomFormatScoreMinimumScoreHelpText'
                  )}
                  onChange={onInputChange}
                />
              </FormGroup>
            ) : null}

            {id === 1 ? (
              <Alert>{translate('DefaultDelayProfileSeries')}</Alert>
            ) : (
              <FormGroup>
                <FormLabel>{translate('Tags')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.TAG}
                  name="tags"
                  {...tags}
                  helpText={translate('DelayProfileSeriesTagsHelpText')}
                  onChange={onInputChange}
                />
              </FormGroup>
            )}
          </Form>
        ) : null}
      </ModalBody>

      <ModalFooter>
        {id && id > 1 ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteDelayProfilePress}
          >
            {translate('Delete')}
          </Button>
        ) : null}

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditDelayProfileModalContent;
