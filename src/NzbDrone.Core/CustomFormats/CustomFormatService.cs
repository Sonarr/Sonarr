using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Cache;
using NzbDrone.Core.CustomFormats.Events;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.CustomFormats
{
    public interface ICustomFormatService
    {
        void Update(CustomFormat customFormat);
        void Update(List<CustomFormat> customFormat);
        CustomFormat Insert(CustomFormat customFormat);
        List<CustomFormat> All();
        CustomFormat GetById(int id);
        void Delete(int id);
        void Delete(List<int> ids);
    }

    public class CustomFormatService : ICustomFormatService
    {
        private readonly ICustomFormatRepository _formatRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICached<Dictionary<int, CustomFormat>> _cache;

        public CustomFormatService(ICustomFormatRepository formatRepository,
                                   ICacheManager cacheManager,
                                   IEventAggregator eventAggregator)
        {
            _formatRepository = formatRepository;
            _eventAggregator = eventAggregator;
            _cache = cacheManager.GetCache<Dictionary<int, CustomFormat>>(typeof(CustomFormat), "formats");
        }

        private Dictionary<int, CustomFormat> AllDictionary()
        {
            return _cache.Get("all", () => _formatRepository.All().ToDictionary(m => m.Id));
        }

        public List<CustomFormat> All()
        {
            return AllDictionary().Values.ToList();
        }

        public CustomFormat GetById(int id)
        {
            return AllDictionary()[id];
        }

        public void Update(CustomFormat customFormat)
        {
            _formatRepository.Update(customFormat);
            _cache.Clear();
        }

        public void Update(List<CustomFormat> customFormat)
        {
            _formatRepository.UpdateMany(customFormat);
            _cache.Clear();
        }

        public CustomFormat Insert(CustomFormat customFormat)
        {
            var result = _formatRepository.Insert(customFormat);
            _cache.Clear();

            _eventAggregator.PublishEventAsync(new CustomFormatAddedEvent(result)).GetAwaiter().GetResult();

            return result;
        }

        public void Delete(int id)
        {
            var format = _formatRepository.Get(id);

            _eventAggregator.PublishEventAsync(new CustomFormatDeletedEvent(format)).GetAwaiter().GetResult();

            _formatRepository.Delete(id);
            _cache.Clear();
        }

        public void Delete(List<int> ids)
        {
            foreach (var id in ids)
            {
                var format = _formatRepository.Get(id);

                _eventAggregator.PublishEventAsync(new CustomFormatDeletedEvent(format)).GetAwaiter().GetResult();

                _formatRepository.Delete(id);
            }

            _cache.Clear();
        }
    }
}
