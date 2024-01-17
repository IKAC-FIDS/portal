using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace TES.Web.Core.Extensions
{
    public static class Extensions
    {
        public static SelectList ToSelectList<TEntity>(this IEnumerable<TEntity> items,
                                           Expression<Func<TEntity, object>> dataValueField,
                                           Expression<Func<TEntity, object>> dataTextField,
                                           string optionText = null,
                                           IEnumerable selectedValue = null)
        {
            var result = items.ToDictionary(dataValueField.Compile(), dataTextField.Compile()).ToList();
            if (!string.IsNullOrEmpty(optionText))
            {
                result.Insert(0, new KeyValuePair<object, object>(string.Empty, optionText));
            }

            return new SelectList(result, "Key", "Value", selectedValue);
        }
    }
}
