using System;
using System.Collections.Generic;
using NzbDrone.Core.Repository.Search;

namespace NzbDrone.Web.Models
{
    public class SearchDetailsModel
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public List<SearchItemModel> SearchResultItems { get; set; }
    }
}