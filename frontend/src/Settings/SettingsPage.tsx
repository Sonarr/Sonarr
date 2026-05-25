import React, { type ReactNode, useMemo } from 'react';
import PageContent from 'Components/Page/PageContent';
import PageToolbar, {
  type MoreMenuItem,
} from 'Components/Page/Toolbar/PageToolbar';
import PendingChangesModal from './PendingChangesModal';
import SettingsSaveItems from './SettingsSaveItems';
import useSettingsSave from './useSettingsSave';

interface SettingsPageProps {
  title: string;
  showSave?: boolean;
  isSaving?: boolean;
  hasPendingChanges?: boolean;
  onSavePress?: () => void;
  moreMenuItems?: MoreMenuItem[];
  toolbarChildren?: ReactNode;
  children?: ReactNode;
}

function SettingsPage({
  title,
  showSave = true,
  isSaving = false,
  hasPendingChanges = false,
  onSavePress,
  moreMenuItems,
  toolbarChildren,
  children,
}: SettingsPageProps) {
  const { saveMoreMenuItem, pendingChangesModalProps } = useSettingsSave({
    showSave,
    isSaving,
    hasPendingChanges,
    onSavePress,
  });

  const allMoreMenuItems = useMemo<MoreMenuItem[]>(() => {
    const extras = moreMenuItems ?? [];
    return saveMoreMenuItem ? [saveMoreMenuItem, ...extras] : extras;
  }, [saveMoreMenuItem, moreMenuItems]);

  return (
    <PageContent title={title}>
      <PageToolbar moreMenuItems={allMoreMenuItems}>
        <SettingsSaveItems
          showSave={showSave}
          isSaving={isSaving}
          hasPendingChanges={hasPendingChanges}
          onSavePress={onSavePress}
        />
        {toolbarChildren}
      </PageToolbar>

      <PendingChangesModal {...pendingChangesModalProps} />

      {children}
    </PageContent>
  );
}

export default SettingsPage;
