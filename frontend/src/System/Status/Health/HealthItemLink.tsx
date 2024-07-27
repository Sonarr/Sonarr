import React from 'react';
import IconButton from 'Components/Link/IconButton';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

interface HealthItemLinkProps {
  source: string;
}

function HealthItemLink(props: HealthItemLinkProps) {
  const { source } = props;

  switch (source) {
    case 'IndexerRssCheck':
    case 'IndexerSearchCheck':
    case 'IndexerStatusCheck':
    case 'IndexerJackettAllCheck':
    case 'IndexerLongTermStatusCheck':
      return (
        <IconButton
          name={icons.SETTINGS}
          title={translate('Settings')}
          to="/settings/indexers"
        />
      );
    case 'DownloadClientCheck':
    case 'DownloadClientStatusCheck':
    case 'ImportMechanismCheck':
      return (
        <IconButton
          name={icons.SETTINGS}
          title={translate('Settings')}
          to="/settings/downloadclients"
        />
      );
    case 'NotificationStatusCheck':
      return (
        <IconButton
          name={icons.SETTINGS}
          title={translate('Settings')}
          to="/settings/connect"
        />
      );
    case 'RootFolderCheck':
      return (
        <IconButton
          name={icons.SERIES_CONTINUING}
          title={translate('SeriesEditor')}
          to="/serieseditor"
        />
      );
    case 'UpdateCheck':
      return (
        <IconButton
          name={icons.UPDATE}
          title={translate('Updates')}
          to="/system/updates"
        />
      );
    default:
      return null;
  }
}

export default HealthItemLink;
