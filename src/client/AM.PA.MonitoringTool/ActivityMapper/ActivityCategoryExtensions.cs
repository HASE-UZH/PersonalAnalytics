// Created at the University of Zurich
// Created: 2018-09-07
// 
// Licensed under the MIT License.

using System.Linq;
using System.Reflection;

namespace ActivityMapper
{
    public static class ActivityCategoryExtensions
    {
        public static string GetDescription(this ActivityCategory activityCategory)
        {
            var type = typeof(ActivityCategory);
            var field = type.GetRuntimeField(activityCategory.ToString());
            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            var description = ((DescriptionAttribute)attributes.First())?.Description;
            return string.IsNullOrWhiteSpace(description) ? "??" : description;
        }
    }
}
