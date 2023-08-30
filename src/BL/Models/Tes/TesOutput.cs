﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/*
 * Task Execution Service
 *
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * OpenAPI spec version: 0.3.0
 *
 * Generated by: https://openapi-generator.tech
 */

using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using BL.Utilities;

namespace BL.Models.Tes
{
    /// <summary>
    /// Output describes Task output files.
    /// </summary>
    [DataContract]
    public partial class TesOutput : IEquatable<TesOutput>
    {
        public TesOutput()
            => NewtonsoftJsonSafeInit.SetDefaultSettings();

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets Description
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// URL in long term storage, for example: s3://my-object-store/file1 gs://my-bucket/file2 file:///path/to/my/file /path/to/my/file etc...
        /// </summary>
        /// <value>URL in long term storage, for example: s3://my-object-store/file1 gs://my-bucket/file2 file:///path/to/my/file /path/to/my/file etc...</value>
        [DataMember(Name = "url")]
        public string Url { get; set; }

        /// <summary>
        /// Path of the file inside the container. Must be an absolute path.
        /// </summary>
        /// <value>Path of the file inside the container. Must be an absolute path.</value>
        [DataMember(Name = "path")]
        public string Path { get; set; }

        /// <summary>
        /// Gets or Sets Type
        /// </summary>
        [DataMember(Name = "type")]
        public TesFileType Type { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
            => new StringBuilder()
                .Append("class TesOutput {\n")
                .Append("  Name: ").Append(Name).Append('\n')
                .Append("  Description: ").Append(Description).Append('\n')
                .Append("  Url: ").Append(Url).Append('\n')
                .Append("  Path: ").Append(Path).Append('\n')
                .Append("  Type: ").Append(Type).Append('\n')
                .Append("}\n")
                .ToString();

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
            => obj switch
            {
                var x when x is null => false,
                var x when ReferenceEquals(this, x) => true,
                _ => obj.GetType() == GetType() && Equals((TesOutput)obj),
            };

        /// <summary>
        /// Returns true if TesOutput instances are equal
        /// </summary>
        /// <param name="other">Instance of TesOutput to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TesOutput other)
            => other switch
            {
                var x when x is null => false,
                var x when ReferenceEquals(this, x) => true,
                _ =>
                (
                    Name == other.Name ||
                    Name is not null &&
                    Name.Equals(other.Name)
                ) &&
                (
                    Description == other.Description ||
                    Description is not null &&
                    Description.Equals(other.Description)
                ) &&
                (
                    Url == other.Url ||
                    Url is not null &&
                    Url.Equals(other.Url)
                ) &&
                (
                    Path == other.Path ||
                    Path is not null &&
                    Path.Equals(other.Path)
                ) &&
                (
                    Type == other.Type ||
                    Type.Equals(other.Type)
                ),
            };

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                // Suitable nullity checks etc, of course :)
                if (Name is not null)
                {
                    hashCode = hashCode * 59 + Name.GetHashCode();
                }

                if (Description is not null)
                {
                    hashCode = hashCode * 59 + Description.GetHashCode();
                }

                if (Url is not null)
                {
                    hashCode = hashCode * 59 + Url.GetHashCode();
                }

                if (Path is not null)
                {
                    hashCode = hashCode * 59 + Path.GetHashCode();
                }

                hashCode = hashCode * 59 + Type.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
#pragma warning disable 1591

        public static bool operator ==(TesOutput left, TesOutput right)
            => Equals(left, right);

        public static bool operator !=(TesOutput left, TesOutput right)
            => !Equals(left, right);

#pragma warning restore 1591
        #endregion Operators
    }
}
