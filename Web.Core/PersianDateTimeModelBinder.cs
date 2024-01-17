using StackExchange.Exceptional;
using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace TES.Web.Core
{
    public class PersianDateTimeModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            var attemptedValue = result?.AttemptedValue;

            if (string.IsNullOrEmpty(attemptedValue))
            {
                return null;
            }    

            try
            {
                var dateParts = attemptedValue.Split('/').Select(x => Convert.ToInt32(x)).ToArray();

                return new PersianCalendar().ToDateTime(dateParts[0], dateParts[1], dateParts[2], 0, 0, 0, 0);
            }
            catch (Exception exception)
            {
                exception.AddLogData("AttemptedValue", attemptedValue).LogNoContext();
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "تاریخ صحیح نمیباشد");
            }

            return null;
        }
    }
}