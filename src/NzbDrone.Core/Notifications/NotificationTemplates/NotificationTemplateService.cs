using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Messaging.Events;
using Scriban;

namespace NzbDrone.Core.Notifications.NotificationTemplates
{
    public interface INotificationTemplateService
    {
        void Update(NotificationTemplate notificationTemplate);
        NotificationTemplate Insert(NotificationTemplate notificationTemplate);
        List<NotificationTemplate> All();
        NotificationTemplate GetById(int id);
        void Delete(int id);
        NotificationTemplate processNotificationTemplate(int notificationTemplateId, GrabMessage grabMessage, string fallbackTitle, string fallbackBody);
        NotificationTemplate processNotificationTemplate(int notificationTemplateId, SeriesAddMessage message, string fallbackTitle, string fallbackBody);
        NotificationTemplate processNotificationTemplate(int notificationTemplateId, EpisodeDeleteMessage deleteMessage, string fallbackTitle, string fallbackBody);
        NotificationTemplate processNotificationTemplate(int notificationTemplateId, SeriesDeleteMessage deleteMessage, string fallbackTitle, string fallbackBody);
        NotificationTemplate processNotificationTemplate(int notificationTemplateId, ImportCompleteMessage message, string fallbackTitle, string fallbackBody);
        NotificationTemplate processNotificationTemplate(int notificationTemplateId, DownloadMessage message, string fallbackTitle, string fallbackBody);
        NotificationTemplate processNotificationTemplate(int notificationTemplateId, HealthCheck.HealthCheck message, string fallbackTitle, string fallbackBody);
        NotificationTemplate processNotificationTemplate(int notificationTemplateId, ApplicationUpdateMessage updateMessage, string fallbackTitle, string fallbackBody);
        NotificationTemplate processNotificationTemplate(int notificationTemplateId, ManualInteractionRequiredMessage message, string fallbackTitle, string fallbackBody);
    }

    public class NotificationTemplateService : INotificationTemplateService
    {
        private readonly INotificationTemplateRepository _templateRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICached<Dictionary<int, NotificationTemplate>> _cache;
        private readonly INotificationRepository _notificationRepository;

        public NotificationTemplateService(INotificationTemplateRepository templateRepository,
                                   ICacheManager cacheManager,
                                   IEventAggregator eventAggregator,
                                   INotificationRepository notificationRepository)
        {
            _templateRepository = templateRepository;
            _eventAggregator = eventAggregator;
            _cache = cacheManager.GetCache<Dictionary<int, NotificationTemplate>>(typeof(NotificationTemplate), "templates");
            _notificationRepository = notificationRepository;
        }

        private Dictionary<int, NotificationTemplate> AllDictionary()
        {
            return _cache.Get("all", () => _templateRepository.All().ToDictionary(m => m.Id));
        }

        public List<NotificationTemplate> All()
        {
            return AllDictionary().Values.ToList();
        }

        public NotificationTemplate GetById(int id)
        {
            return AllDictionary()[id];
        }

        public void Update(NotificationTemplate notificationTemplate)
        {
            _templateRepository.Update(notificationTemplate);
            _cache.Clear();
        }

        public void Update(List<NotificationTemplate> notificationTemplate)
        {
            _templateRepository.UpdateMany(notificationTemplate);
            _cache.Clear();
        }

        public NotificationTemplate Insert(NotificationTemplate notificationTemplate)
        {
            var result = _templateRepository.Insert(notificationTemplate);
            _cache.Clear();

            return result;
        }

        public void Delete(int id)
        {
            _notificationRepository.removeNotificationTemplate(id);
            _templateRepository.Delete(id);
            _cache.Clear();
        }

        public void Delete(List<int> ids)
        {
            foreach (var id in ids)
            {
                _notificationRepository.removeNotificationTemplate(id);
                _templateRepository.Delete(id);
            }

            _cache.Clear();
        }

        NotificationTemplate INotificationTemplateService.processNotificationTemplate(int notificationTemplateId, GrabMessage grabMessage, string fallbackTitle, string fallbackBody)
        {
            var templateParams = new NotificationTemplateParameters
            {
                FallbackTitle = fallbackTitle,
                FallbackBody = fallbackBody,
                GrabMessage = grabMessage
            };
            return this.ProcessNotificationTemplate(notificationTemplateId, templateParams);
        }

        NotificationTemplate INotificationTemplateService.processNotificationTemplate(int notificationTemplateId, SeriesAddMessage message, string fallbackTitle, string fallbackBody)
        {
            var templateParams = new NotificationTemplateParameters
            {
                FallbackTitle = fallbackTitle,
                FallbackBody = fallbackBody,
                SeriesAddMessage = message
            };
            return this.ProcessNotificationTemplate(notificationTemplateId, templateParams);
        }

        NotificationTemplate INotificationTemplateService.processNotificationTemplate(int notificationTemplateId, EpisodeDeleteMessage deleteMessage, string fallbackTitle, string fallbackBody)
        {
            var templateParams = new NotificationTemplateParameters
            {
                FallbackTitle = fallbackTitle,
                FallbackBody = fallbackBody,
                EpisodeDeleteMessage = deleteMessage
            };
            return this.ProcessNotificationTemplate(notificationTemplateId, templateParams);
        }

        NotificationTemplate INotificationTemplateService.processNotificationTemplate(int notificationTemplateId, SeriesDeleteMessage deleteMessage, string fallbackTitle, string fallbackBody)
        {
            var templateParams = new NotificationTemplateParameters
            {
                FallbackTitle = fallbackTitle,
                FallbackBody = fallbackBody,
                SeriesDeleteMessage = deleteMessage
            };
            return this.ProcessNotificationTemplate(notificationTemplateId, templateParams);
        }

        NotificationTemplate INotificationTemplateService.processNotificationTemplate(int notificationTemplateId, ImportCompleteMessage message, string fallbackTitle, string fallbackBody)
        {
            var templateParams = new NotificationTemplateParameters
            {
                FallbackTitle = fallbackTitle,
                FallbackBody = fallbackBody,
                ImportCompleteMessage = message
            };
            return this.ProcessNotificationTemplate(notificationTemplateId, templateParams);
        }

        NotificationTemplate INotificationTemplateService.processNotificationTemplate(int notificationTemplateId, DownloadMessage message, string fallbackTitle, string fallbackBody)
        {
            var templateParams = new NotificationTemplateParameters
            {
                FallbackTitle = fallbackTitle,
                FallbackBody = fallbackBody,
                DownloadMessage = message
            };
            return this.ProcessNotificationTemplate(notificationTemplateId, templateParams);
        }

        NotificationTemplate INotificationTemplateService.processNotificationTemplate(int notificationTemplateId, HealthCheck.HealthCheck message, string fallbackTitle, string fallbackBody)
        {
            var templateParams = new NotificationTemplateParameters
            {
                FallbackTitle = fallbackTitle,
                FallbackBody = fallbackBody,
                HealthCheck = message
            };
            return this.ProcessNotificationTemplate(notificationTemplateId, templateParams);
        }

        NotificationTemplate INotificationTemplateService.processNotificationTemplate(int notificationTemplateId, ApplicationUpdateMessage updateMessage, string fallbackTitle, string fallbackBody)
        {
            var templateParams = new NotificationTemplateParameters
            {
                FallbackTitle = fallbackTitle,
                FallbackBody = fallbackBody,
                ApplicationUpdateMessage = updateMessage
            };
            return this.ProcessNotificationTemplate(notificationTemplateId, templateParams);
        }

        NotificationTemplate INotificationTemplateService.processNotificationTemplate(int notificationTemplateId, ManualInteractionRequiredMessage message, string fallbackTitle, string fallbackBody)
        {
            var templateParams = new NotificationTemplateParameters
            {
                FallbackTitle = fallbackTitle,
                FallbackBody = fallbackBody,
                ManualInteractionRequiredMessage = message
            };
            return this.ProcessNotificationTemplate(notificationTemplateId, templateParams);
        }

        private NotificationTemplate ProcessNotificationTemplate(int notificationTemplateId, NotificationTemplateParameters templateParams)
        {
            var processedNotificationTemplate = new NotificationTemplate();
            processedNotificationTemplate.Title = templateParams.FallbackTitle;
            processedNotificationTemplate.Body = templateParams.FallbackBody;

            if (notificationTemplateId > 0)
            {
                var notificationTemplate = _templateRepository.Find(notificationTemplateId);
                if (notificationTemplate != null && (
                    (templateParams.GrabMessage != null && notificationTemplate.OnGrab)
                    || (templateParams.SeriesAddMessage != null && notificationTemplate.OnSeriesAdd)
                    || (templateParams.EpisodeDeleteMessage != null && notificationTemplate.OnEpisodeFileDelete)
                    || (templateParams.SeriesDeleteMessage != null && notificationTemplate.OnSeriesDelete)
                    || (templateParams.ImportCompleteMessage != null && notificationTemplate.OnImportComplete)
                    || (templateParams.DownloadMessage != null && notificationTemplate.OnDownload)
                    || (templateParams.HealthCheck != null && (notificationTemplate.OnHealthIssue || notificationTemplate.OnHealthRestored))
                    || (templateParams.ApplicationUpdateMessage != null && notificationTemplate.OnApplicationUpdate)
                    || (templateParams.ManualInteractionRequiredMessage != null && notificationTemplate.OnManualInteractionRequired)))
                {
                    if (!string.IsNullOrEmpty(notificationTemplate.Title))
                    {
                        var tpl = Template.Parse(notificationTemplate.Title);
                        processedNotificationTemplate.Title = tpl.Render(templateParams);
                    }

                    if (!string.IsNullOrEmpty(notificationTemplate.Body))
                    {
                        var tpl = Template.Parse(notificationTemplate.Body);
                        processedNotificationTemplate.Body = tpl.Render(templateParams);
                    }

                    return processedNotificationTemplate;
                }
            }

            return processedNotificationTemplate;
        }
    }
}
