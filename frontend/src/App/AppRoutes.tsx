import React from 'react';
import { Navigate, Route } from 'react-router-dom';
import Blocklist from 'Activity/Blocklist/Blocklist';
import History from 'Activity/History/History';
import Queue from 'Activity/Queue/Queue';
import AddNewSeries from 'AddSeries/AddNewSeries/AddNewSeries';
import ImportSeriesPage from 'AddSeries/ImportSeries/ImportSeriesPage';
import CalendarPage from 'Calendar/CalendarPage';
import NotFound from 'Components/NotFound';
import Switch from 'Components/Router/Switch';
import SeriesDetailsPage from 'Series/Details/SeriesDetailsPage';
import SeriesIndex from 'Series/Index/SeriesIndex';
import CustomFormatSettingsPage from 'Settings/CustomFormats/CustomFormatSettingsPage';
import DownloadClientSettings from 'Settings/DownloadClients/DownloadClientSettings';
import GeneralSettings from 'Settings/General/GeneralSettings';
import ImportListSettings from 'Settings/ImportLists/ImportListSettings';
import IndexerSettings from 'Settings/Indexers/IndexerSettings';
import MediaManagement from 'Settings/MediaManagement/MediaManagement';
import MetadataSettings from 'Settings/Metadata/MetadataSettings';
import MetadataSourceSettings from 'Settings/MetadataSource/MetadataSourceSettings';
import NotificationSettings from 'Settings/Notifications/NotificationSettings';
import Profiles from 'Settings/Profiles/Profiles';
import Quality from 'Settings/Quality/Quality';
import Settings from 'Settings/Settings';
import TagSettings from 'Settings/Tags/TagSettings';
import UISettings from 'Settings/UI/UISettings';
import Backups from 'System/Backup/Backups';
import LogsTable from 'System/Events/LogsTable';
import Logs from 'System/Logs/Logs';
import Status from 'System/Status/Status';
import Tasks from 'System/Tasks/Tasks';
import Updates from 'System/Updates/Updates';
import CutoffUnmet from 'Wanted/CutoffUnmet/CutoffUnmet';
import Missing from 'Wanted/Missing/Missing';

function AppRoutes() {
  return (
    <Switch>
      {/*
        Series
      */}

      <Route path="/" element={<SeriesIndex />} />

      <Route path="/add/new" element={<AddNewSeries />} />

      <Route path="/add/import/*" element={<ImportSeriesPage />} />

      <Route path="/serieseditor" element={<Navigate to="/" replace />} />

      <Route path="/seasonpass" element={<Navigate to="/" replace />} />

      <Route path="/series/:titleSlug" element={<SeriesDetailsPage />} />

      {/*
        Calendar
      */}

      <Route path="/calendar" element={<CalendarPage />} />

      {/*
        Activity
      */}

      <Route path="/activity/history" element={<History />} />

      <Route path="/activity/queue" element={<Queue />} />

      <Route path="/activity/blocklist" element={<Blocklist />} />

      {/*
        Wanted
      */}

      <Route path="/wanted/missing" element={<Missing />} />

      <Route path="/wanted/cutoffunmet" element={<CutoffUnmet />} />

      {/*
        Settings
      */}

      <Route path="/settings" element={<Settings />} />

      <Route path="/settings/mediamanagement" element={<MediaManagement />} />

      <Route path="/settings/profiles" element={<Profiles />} />

      <Route path="/settings/quality" element={<Quality />} />

      <Route
        path="/settings/customformats"
        element={<CustomFormatSettingsPage />}
      />

      <Route path="/settings/indexers" element={<IndexerSettings />} />

      <Route
        path="/settings/downloadclients"
        element={<DownloadClientSettings />}
      />

      <Route path="/settings/importlists" element={<ImportListSettings />} />

      <Route path="/settings/connect" element={<NotificationSettings />} />

      <Route path="/settings/metadata" element={<MetadataSettings />} />

      <Route
        path="/settings/metadatasource"
        element={<MetadataSourceSettings />}
      />

      <Route path="/settings/tags" element={<TagSettings />} />

      <Route path="/settings/general" element={<GeneralSettings />} />

      <Route path="/settings/ui" element={<UISettings />} />

      {/*
        System
      */}

      <Route path="/system/status" element={<Status />} />

      <Route path="/system/tasks" element={<Tasks />} />

      <Route path="/system/backup" element={<Backups />} />

      <Route path="/system/updates" element={<Updates />} />

      <Route path="/system/events" element={<LogsTable />} />

      <Route path="/system/logs/files/*" element={<Logs />} />

      {/*
        Not Found
      */}

      <Route path="*" element={<NotFound />} />
    </Switch>
  );
}

export default AppRoutes;
