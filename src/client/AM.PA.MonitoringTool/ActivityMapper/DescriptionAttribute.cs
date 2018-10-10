// Created at the University of Zurich
// Created: 2018-09-07
// 
// Licensed under the MIT License.

using System;

namespace ActivityMapper
{
    public class DescriptionAttribute : Attribute
    {
        public DescriptionAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; }
    }
}
