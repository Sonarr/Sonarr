import React, { useCallback, useMemo, useState } from 'react';
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
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import { EpisodeOrderType } from 'Series/Series';
import useAvailableOrderings from 'Series/useAvailableOrderings';
import { useToggleSeasonMonitored } from 'Series/useSeries';
import { useGeneralSettings } from 'Settings/General/useGeneralSettings';
import { useNamingSettings } from 'Settings/MediaManagement/Naming/useNamingSettings';
import translate from 'Utilities/String/translate';

export interface EpisodeOrderModalContentProps {
  seriesId: number;
  seasonNumber: number;
  currentOverride?: EpisodeOrderType;
  seriesEpisodeOrder: EpisodeOrderType;
  monitored: boolean;
  onModalClose: () => void;
}

function getOrderTypeLabel(type: string): string {
  const labels: Record<string, string> = {
    default: translate('AiredOrder'),
    dvd: translate('DvdOrder'),
    absolute: translate('AbsoluteOrder'),
    alternate: translate('AlternateOrder'),
    altDvd: translate('AlternateDvdOrder'),
    regional: translate('RegionalOrder'),
  };

  return labels[type] ?? type;
}

function EpisodeOrderModalContent({
  seriesId,
  seasonNumber,
  currentOverride,
  seriesEpisodeOrder,
  monitored,
  onModalClose,
}: EpisodeOrderModalContentProps) {
  const [selectedOrder, setSelectedOrder] = useState<string>(
    currentOverride || ''
  );

  const { data: availableOrderings, isLoading: isLoadingOrderings } =
    useAvailableOrderings(seriesId);

  const { data: generalSettings } = useGeneralSettings();
  const hasTvdbApiKey = !!generalSettings?.tvdbApiKey;

  const { data: namingSettings } = useNamingSettings();
  const renameEnabled = namingSettings?.renameEpisodes ?? false;

  const { toggleSeasonMonitored, isTogglingSeasonMonitored } =
    useToggleSeasonMonitored(seriesId);

  const orderingOptions = useMemo(() => {
    const options = [
      {
        key: '',
        value: translate('SeriesDefaultEpisodeOrder', {
          orderType: getOrderTypeLabel(seriesEpisodeOrder),
        }),
      },
    ];

    if (availableOrderings.length > 0) {
      // Only show orderings that TVDB reports as available
      availableOrderings.forEach((ordering) => {
        options.push({
          key: ordering.type,
          value: getOrderTypeLabel(ordering.type),
        });
      });
    } else if (!isLoadingOrderings) {
      // Fallback: show all options if TVDB fetch failed/empty
      for (const key of [
        'default',
        'dvd',
        'absolute',
        'alternate',
        'altDvd',
        'regional',
      ]) {
        options.push({ key, value: getOrderTypeLabel(key) });
      }
    }

    return options;
  }, [availableOrderings, isLoadingOrderings, seriesEpisodeOrder]);

  const handleOrderChange = useCallback(
    ({ value }: { name: string; value: string }) => {
      setSelectedOrder(value);
    },
    []
  );

  const handleSavePress = useCallback(() => {
    toggleSeasonMonitored({
      seasonNumber,
      monitored,
      episodeOrderOverride: selectedOrder === '' ? undefined : selectedOrder,
    });
    onModalClose();
  }, [
    seasonNumber,
    monitored,
    selectedOrder,
    toggleSeasonMonitored,
    onModalClose,
  ]);

  const isChanging = (currentOverride || '') !== selectedOrder;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('SeasonNumberToken', { seasonNumber })} —{' '}
        {translate('EpisodeOrdering')}
      </ModalHeader>

      <ModalBody>
        {hasTvdbApiKey ? null : (
          <Alert kind={kinds.WARNING}>{translate('TvdbApiKeyRequired')}</Alert>
        )}

        {isLoadingOrderings ? (
          <LoadingIndicator />
        ) : (
          <Form>
            <FormGroup size={sizes.MEDIUM}>
              <FormLabel>{translate('EpisodeOrdering')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="episodeOrderOverride"
                value={selectedOrder}
                values={orderingOptions}
                isDisabled={!hasTvdbApiKey}
                helpText={translate('EpisodeOrderingOverrideHelpText')}
                onChange={handleOrderChange}
              />
            </FormGroup>
          </Form>
        )}

        {isChanging && renameEnabled ? (
          <Alert kind={kinds.INFO}>
            {translate('EpisodeOrderingRenamingEnabledMessage')}
          </Alert>
        ) : null}

        {isChanging && !renameEnabled ? (
          <Alert kind={kinds.INFO}>
            {translate('EpisodeOrderingRenamingNotEnabledMessage')}
          </Alert>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isTogglingSeasonMonitored}
          isDisabled={!isChanging || !hasTvdbApiKey}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EpisodeOrderModalContent;
