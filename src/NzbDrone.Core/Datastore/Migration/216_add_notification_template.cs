using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(216)]
    public class add_notification_template : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("NotificationTemplates")
                  .WithColumn("Name").AsString().NotNullable().Unique()
                  .WithColumn("Title").AsString().NotNullable()
                  .WithColumn("Body").AsString().NotNullable()
                  .WithColumn("OnGrab").AsBoolean().WithDefaultValue(true)
                  .WithColumn("OnDownload").AsBoolean().WithDefaultValue(true)
                  .WithColumn("OnUpgrade").AsBoolean().WithDefaultValue(true)
                  .WithColumn("OnImportComplete").AsBoolean().WithDefaultValue(true)
                  .WithColumn("OnRename").AsBoolean().WithDefaultValue(false)
                  .WithColumn("OnSeriesAdd").AsBoolean().WithDefaultValue(true)
                  .WithColumn("OnSeriesDelete").AsBoolean().WithDefaultValue(false)
                  .WithColumn("OnEpisodeFileDelete").AsBoolean().WithDefaultValue(false)
                  .WithColumn("OnEpisodeFileDeleteForUpgrade").AsBoolean().WithDefaultValue(false)
                  .WithColumn("OnHealthIssue").AsBoolean().WithDefaultValue(false)
                  .WithColumn("OnHealthRestored").AsBoolean().WithDefaultValue(false)
                  .WithColumn("OnApplicationUpdate").AsBoolean().WithDefaultValue(false)
                  .WithColumn("OnManualInteractionRequired").AsBoolean().WithDefaultValue(false);

            Alter.Table("Notifications").AddColumn("NotificationTemplateId").AsInt32().WithDefaultValue(0);

            Execute.WithConnection(CreateDefaultHtmlTemplate);
            Execute.WithConnection(UpdateEmailConnections);
        }

        private void CreateDefaultHtmlTemplate(IDbConnection conn, IDbTransaction tran)
        {
            var name = "Email template";
            var title = "Sonarr - {{ if grab_message }}Episode Grabbed{{ else if series_add_message }}Series Added{{ else }}{{fallback_title}}{{ end }}";
            var body = @"<!DOCTYPE html>
<html lang=""en"" xmlns:th=""http://www.thymeleaf.org"">
	<head>
		<title>Sonarr Notification</title>
	</head>
	<body>
		{{ if grab_message }}
			{{ series = grab_message.series }}
			<p>{{grab_message.episode.parsed_episode_info.series_title}} - {{grab_message.episode.parsed_episode_info.release_title}} sent to queue.</p>
		{{ else if series_add_message }}
			{{ series = series_add_message.series }}
		{{ else }}
			<p>{{fallback_body}}</p>
		{{ end }}
		{{ if series }}
			<h3>{{series.title}}</h3>
			<p>{{series.overview}}</p>
			{{- for image in series.images }}
				{{ if image.cover_type == ""Banner"" }}
					<img src=""{{image.remote_url}}"" alt=""Series banner"">
				{{ end }}
			{{- end }}
		{{ end }}
		<div id=""footer"">
			<p>Metadata is provided by theTVDB</p>
		</div>
	</body>
</html>";

            using (var updateCmd = conn.CreateCommand())
            {
                updateCmd.Transaction = tran;
                updateCmd.CommandText = "INSERT INTO \"NotificationTemplates\" (\"Name\", \"Title\", \"Body\") VALUES (?, ?, ?)";
                updateCmd.AddParameter(name);
                updateCmd.AddParameter(title);
                updateCmd.AddParameter(body);

                updateCmd.ExecuteNonQuery();
            }
        }

        private void UpdateEmailConnections(IDbConnection conn, IDbTransaction tran)
        {
            using (var selectCmd = conn.CreateCommand())
            {
                selectCmd.Transaction = tran;
                selectCmd.CommandText = "SELECT \"Id\" from \"NotificationTemplates\" DESC LIMIT 1";
                var id = selectCmd.ExecuteReader().Read();

                using (var updateCmd = conn.CreateCommand())
                {
                    updateCmd.Transaction = tran;
                    updateCmd.CommandText = "UPDATE \"Notifications\" SET \"NotificationTemplateId\" = ? WHERE \"Implementation\" = 'Email' and \"NotificationTemplateId\" = 0";
                    updateCmd.AddParameter(id);

                    updateCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
