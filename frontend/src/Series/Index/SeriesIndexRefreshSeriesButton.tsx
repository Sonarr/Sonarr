import React, { useCallback, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useSelect } from 'App/SelectContext';
import ClientSideCollectionAppState from 'App/State/ClientSideCollectionAppState';
import SeriesAppState, { SeriesIndexAppState } from 'App/State/SeriesAppState';
import { REFRESH_SERIES } from 'Commands/commandNames';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import { icons } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createSeriesClientSideCollectionItemsSelector from 'Store/Selectors/createSeriesClientSideCollectionItemsSelector';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';

interface SeriesIndexRefreshSeriesButtonProps {
  isSelectMode: boolean;
  selectedFilterKey: string;
}

function SeriesIndexRefreshSeriesButton(
  props: SeriesIndexRefreshSeriesButtonProps
) {
  const isRefreshing = useSelector(
    createCommandExecutingSelector(REFRESH_SERIES)
  );
  const {
    items,
    totalItems,
  }: SeriesAppState & SeriesIndexAppState & ClientSideCollectionAppState =
    useSelector(createSeriesClientSideCollectionItemsSelector('seriesIndex'));

  const dispatch = useDispatch();
  const { isSelectMode, selectedFilterKey } = props;
  const [selectState] = useSelect();
  const { selectedState } = selectState;

  const selectedSeriesIds = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

  const seriesToRefresh =
    isSelectMode && selectedSeriesIds.length > 0
      ? selectedSeriesIds
      : items.map((m) => m.id);

  let refreshLabel = translate('UpdateAll');

  if (selectedSeriesIds.length > 0) {
    refreshLabel = translate('UpdateSelected');
  } else if (selectedFilterKey !== 'all') {
    refreshLabel = translate('UpdateFiltered');
  }

  const onPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: REFRESH_SERIES,
        seriesIds: seriesToRefresh,
      })
    );
  }, [dispatch, seriesToRefresh]);

  return (
    <PageToolbarButton
      label={refreshLabel}
      isSpinning={isRefreshing}
      isDisabled={!totalItems}
      iconName={icons.REFRESH}
      onPress={onPress}
    />
  );
}

export default SeriesIndexRefreshSeriesButton;
