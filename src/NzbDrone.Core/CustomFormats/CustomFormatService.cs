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
        CustomFormat Insert(CustomFormat customFormat);
        List<CustomFormat> All();
        CustomFormat GetById(int id);
        void Delete(int id);
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

        public CustomFormat Insert(CustomFormat customFormat)
        {
            // Add to DB then insert into profiles
            var result = _formatRepository.Insert(customFormat);
            _cache.Clear();

            _eventAggregator.PublishEvent(new CustomFormatAddedEvent(result));

            return result;
        }

        public void Delete(int id)
        {
            var format = _formatRepository.Get(id);

            // Remove from profiles before removing from DB
            _eventAggregator.PublishEvent(new CustomFormatDeletedEvent(format));

            _formatRepository.Delete(id);
            _cache.Clear();
        }
    }
}
