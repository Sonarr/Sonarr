import { Action, Location, UnregisterCallback } from 'history';
import React, {
  ReactElement,
  useCallback,
  useEffect,
  useRef,
  useState,
} from 'react';
import { useHistory } from 'react-router';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import useKeyboardShortcuts from 'Helpers/Hooks/useKeyboardShortcuts';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import AdvancedSettingsButton from './AdvancedSettingsButton';
import PendingChangesModal from './PendingChangesModal';

interface SettingsToolbarProps {
  showSave?: boolean;
  isSaving?: boolean;
  hasPendingChanges?: boolean;
  // TODO: This should do type checking like PageToolbarSectionProps,
  // but this works for the time being.
  additionalButtons?: ReactElement | null;
  onSavePress?: () => void;
}

function SettingsToolbar({
  showSave = true,
  isSaving,
  hasPendingChanges,
  additionalButtons = null,
  onSavePress,
}: SettingsToolbarProps) {
  const { bindShortcut, unbindShortcut } = useKeyboardShortcuts();
  const history = useHistory();
  const [nextLocation, setNextLocation] = useState<Location | null>(null);
  const [nextLocationAction, setNextLocationAction] = useState<Action | null>(
    null
  );
  const hasConfirmed = useRef(false);
  const unblocker = useRef<UnregisterCallback>();

  const handleConfirmNavigation = useCallback(() => {
    if (!nextLocation) {
      return;
    }

    const path = `${nextLocation.pathname}${nextLocation.search}`;

    hasConfirmed.current = true;

    if (nextLocationAction === 'PUSH') {
      history.push(path);
    } else {
      // Unfortunately back and forward both use POP,
      // which means we don't actually know which direction
      // the user wanted to go, assuming back.

      history.goBack();
    }
  }, [nextLocation, nextLocationAction, history]);

  const handleCancelNavigation = useCallback(() => {
    setNextLocation(null);
    setNextLocationAction(null);
    hasConfirmed.current = false;
  }, []);

  const handleRouterLeaving = useCallback(
    (routerLocation: Location, routerAction: Action) => {
      if (hasConfirmed.current) {
        setNextLocation(null);
        setNextLocationAction(null);
        hasConfirmed.current = false;

        return;
      }

      if (hasPendingChanges) {
        setNextLocation(routerLocation);
        setNextLocationAction(routerAction);

        return false;
      }

      return;
    },
    [hasPendingChanges]
  );

  useEffect(() => {
    unblocker.current = history.block(handleRouterLeaving);

    return () => {
      unblocker.current?.();
    };
  }, [history, handleRouterLeaving]);

  useEffect(() => {
    bindShortcut(
      'saveSettings',
      () => {
        if (hasPendingChanges) {
          onSavePress?.();
        }
      },
      {
        isGlobal: true,
      }
    );

    return () => {
      unbindShortcut('saveSettings');
    };
  }, [hasPendingChanges, bindShortcut, unbindShortcut, onSavePress]);

  return (
    <PageToolbar>
      <PageToolbarSection>
        <AdvancedSettingsButton showLabel={true} />
        {showSave ? (
          <PageToolbarButton
            label={
              hasPendingChanges
                ? translate('SaveChanges')
                : translate('NoChanges')
            }
            iconName={icons.SAVE}
            isSpinning={isSaving}
            isDisabled={!hasPendingChanges}
            onPress={onSavePress}
          />
        ) : null}

        {additionalButtons}
      </PageToolbarSection>

      <PendingChangesModal
        isOpen={nextLocation !== null}
        onConfirm={handleConfirmNavigation}
        onCancel={handleCancelNavigation}
      />
    </PageToolbar>
  );
}

export default SettingsToolbar;
