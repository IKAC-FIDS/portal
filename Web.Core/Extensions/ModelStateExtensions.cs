using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace TES.Web.Core.Extensions
{
    public static class ModelStateExtensions
    {
        public static List<string> ToErrorList(this ModelStateDictionary modelState)
        {
            var errors = modelState.Where(x => x.Value.Errors.Any())
                .SelectMany(y => y.Value.Errors.Select(e => e.ErrorMessage))
                .ToList();

            return errors;
        }
    }
}