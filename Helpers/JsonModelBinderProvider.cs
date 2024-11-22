using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;

namespace Agri_Smart.Helpers
{
    public class JsonModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.IsComplexType && context.BindingInfo.BindingSource == BindingSource.Form)
            {
                return new BinderTypeModelBinder(typeof(JsonModelBinder));
            }

            return null;
        }
    }
}
