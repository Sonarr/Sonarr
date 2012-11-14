using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Web.Helpers.Binders
{
    public class QualityTypesBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue("quality");
            var quality = QualityTypes.FindById(Convert.ToInt32(value.AttemptedValue));

            return quality;
        }
    }
}