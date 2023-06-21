// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace BL.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TesTaskLogMetadataKeyAttribute : Attribute
    {
        public string Name { get; private set; }

        public TesTaskLogMetadataKeyAttribute(string name)
            => Name = name;
    }
}
