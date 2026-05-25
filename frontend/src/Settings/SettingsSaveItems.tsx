import React from 'react';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import AdvancedSettingsButton from './AdvancedSettingsButton';

interface SettingsSaveItemsProps {
  showSave?: boolean;
  isSaving?: boolean;
  hasPendingChanges?: boolean;
  onSavePress?: () => void;
}

function SettingsSaveItems({
  showSave = true,
  isSaving = false,
  hasPendingChanges = false,
  onSavePress,
}: SettingsSaveItemsProps) {
  return (
    <>
      <ToolbarItem id="advanced-settings" pinned={true}>
        <AdvancedSettingsButton showLabel={true} />
      </ToolbarItem>

      {showSave ? (
        <ToolbarItem id="save" priority={1} groupId="save">
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
        </ToolbarItem>
      ) : null}
    </>
  );
}

export default SettingsSaveItems;
