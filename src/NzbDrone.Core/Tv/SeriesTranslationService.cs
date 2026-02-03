using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public interface ISeriesTranslationService
    {
        List<SeriesTranslation> GetTranslations();
        List<SeriesTranslation> GetTranslations(int seriesId);
        List<SeriesTranslation> UpdateTranslations(int seriesId, List<SeriesTranslation> translations);
    }

    public class SeriesTranslationService : ISeriesTranslationService, IHandleAsync<SeriesDeletedEvent>
    {
        private readonly ISeriesTranslationRepository _seriesTranslationRepository;
        private readonly Logger _logger;

        public SeriesTranslationService(ISeriesTranslationRepository seriesTranslationRepository, Logger logger)
        {
            _seriesTranslationRepository = seriesTranslationRepository;
            _logger = logger;
        }

        public List<SeriesTranslation> GetTranslations()
        {
            return _seriesTranslationRepository.All().ToList();
        }

        public List<SeriesTranslation> GetTranslations(int seriesId)
        {
            return _seriesTranslationRepository.GetTranslations(seriesId);
        }

        public List<SeriesTranslation> UpdateTranslations(int seriesId, List<SeriesTranslation> translations)
        {
            var toAdd = new List<SeriesTranslation>();
            var toUpdate = new List<SeriesTranslation>();
            var existing = GetTranslations(seriesId);

            translations.ForEach(translation =>
            {
                var existingTranslation = existing.FirstOrDefault(e => e.Language == translation.Language);

                translation.SeriesId = seriesId;

                if (existingTranslation == null)
                {
                    toAdd.Add(translation);
                }
                else
                {
                    translation.Id = existingTranslation.Id;

                    toUpdate.Add(translation);
                    existing.Remove(existingTranslation);
                }
            });

            _seriesTranslationRepository.InsertMany(toAdd);
            _seriesTranslationRepository.UpdateMany(toUpdate);
            _seriesTranslationRepository.DeleteMany(existing);

            return translations;
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            _seriesTranslationRepository.DeleteTranslations(message.Series.Select(s => s.Id).ToList());
        }
    }
}
