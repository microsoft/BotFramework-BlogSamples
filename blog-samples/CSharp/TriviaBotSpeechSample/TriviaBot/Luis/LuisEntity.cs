// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Luis
{
    /// <summary>
    /// Describes an entity that was identified in the query
    /// </summary>
    [DataContract]
    [Serializable]
    public class LuisEntity : IComparable, IComparable<LuisEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuisEntity" /> class.
        /// </summary>
        /// <param name="entity">The entity value</param>
        /// <param name="type">The type of the entity</param>
        /// <param name="startIndex">The start index of the entity value in the text</param>
        /// <param name="endIndex">The end index of the entity value in the text</param>
        /// <param name="score">Confidence that the entity was properly identified</param>
        public LuisEntity(string entity, string type, int startIndex, int endIndex, double score)
        {
            Entity = entity ?? string.Empty;
            Type = type ?? string.Empty;
            StartIndex = startIndex;
            EndIndex = endIndex;
            Score = score;
        }

        /// <summary>
        /// Gets the entity value
        /// </summary>
        [DataMember(Name = "entity")]
        public string Entity { get; private set; }

        /// <summary>
        /// Gets the type, or name, of the entity
        /// </summary>
        [DataMember(Name = "type")]
        public string Type { get; private set; }

        /// <summary>
        /// Gets the index in the query this entity starts at
        /// </summary>
        [DataMember(Name = "startIndex")]
        public int StartIndex { get; private set; }

        /// <summary>
        /// Gets the index in the query this entity ends at
        /// </summary>
        [DataMember(Name = "endIndex")]
        public int EndIndex { get; private set; }

        /// <summary>
        /// Gets the certainty this entity was identified with
        /// </summary>
        [DataMember(Name = "score")]
        public double Score { get; private set; }

        /// <summary>
        /// Gets the type, or name, of the entity
        /// </summary>
        [DataMember(Name = "resolution")]
        public Dictionary<string, string> Resolution { get; private set; }

        /// <summary>
        /// Determines if left is equal to right
        /// </summary>
        /// <param name="left">The object to compare</param>
        /// <param name="right">The object to compare against</param>
        /// <returns>The result of the comparison</returns>
        public static bool operator ==(LuisEntity left, LuisEntity right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Determines if left is not equal to right
        /// </summary>
        /// <param name="left">The object to compare</param>
        /// <param name="right">The object to compare against</param>
        /// <returns>The result of the comparison</returns>
        public static bool operator !=(LuisEntity left, LuisEntity right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determines if left is less than right
        /// </summary>
        /// <param name="left">The object to compare</param>
        /// <param name="right">The object to compare against</param>
        /// <returns>The result of the comparison</returns>
        public static bool operator <(LuisEntity left, LuisEntity right)
        {
            return Compare(left, right) < 0;
        }

        /// <summary>
        /// Determines if left is greater than right
        /// </summary>
        /// <param name="left">The object to compare</param>
        /// <param name="right">The object to compare against</param>
        /// <returns>The result of the comparison</returns>
        public static bool operator >(LuisEntity left, LuisEntity right)
        {
            return Compare(left, right) > 0;
        }

        /// <summary>
        /// Compare left with right
        /// </summary>
        /// <param name="left">The object to compare</param>
        /// <param name="right">The object to compare against</param>
        /// <returns>The result of the comparison</returns>
        public static int Compare(LuisEntity left, LuisEntity right)
        {
            if (object.ReferenceEquals(left, right))
            {
                return 0;
            }

            if (object.ReferenceEquals(left, null))
            {
                return -1;
            }

            return left.CompareTo(right);
        }

        /// <summary>
        /// Gets the string representation of this object
        /// </summary>
        /// <returns>The string representation of this object</returns>
        public override string ToString()
        {
            return "{ Entity = " + Entity + ", Type = " + Type + ", StartIndex = " +
                StartIndex + ", EndIndex = " + EndIndex + ", Score = " + Score + " }";
        }

        /// <summary>
        /// Compares this EntityDefinition to the passed in object
        /// </summary>
        /// <param name="obj">The object to compare this EntityDefinition with</param>
        /// <returns>The result of the comparison</returns>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            LuisEntity other = obj as LuisEntity;

            if (other == null)
            {
                throw new ArgumentException("A EntityDefinition object is required for comparison.", "obj");
            }

            return CompareTo(other);
        }

        /// <summary>
        /// Compares this EntityDefinition to the passed in EntityDefinition
        /// The ordering follows the following precedence (stopping when one value differs)
        /// Higher score
        /// Case insensitive lexicographic ordering Type
        /// Case sensitive lexicographic Entity (value)
        /// Earlier in the string first
        /// Shorter first
        /// </summary>
        /// <param name="other">The EntityDefinition to compare against</param>
        /// <returns>The result of the comparison</returns>
        public int CompareTo(LuisEntity other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return 1;
            }

            // Compare score (higher comes first)
            var comparison = other.Score.CompareTo(Score);

            // Compare Type
            if (comparison == 0)
            {
                comparison = string.Compare(Type, other.Type, StringComparison.Ordinal);
            }

            // Compare Entity
            if (comparison == 0)
            {
                comparison = string.Compare(Entity, other.Entity, StringComparison.Ordinal);
            }

            // Compare StartIndex
            if (comparison == 0)
            {
                comparison = StartIndex.CompareTo(other.StartIndex);
            }

            // Compare EndIndex
            if (comparison == 0)
            {
                comparison = EndIndex.CompareTo(other.EndIndex);
            }

            return comparison;
        }

        /// <summary>
        /// Determines if this EntityDefinition is equal to another
        /// </summary>
        /// <param name="obj">The object to compare this EntityDefinition with</param>
        /// <returns>Whether the objects are equal</returns>
        public override bool Equals(object obj)
        {
            var other = obj as LuisEntity;

            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            return CompareTo(other) == 0;
        }

        /// <summary>
        /// A hash code for this object that will always be the same for EntityDefinition objects with the
        /// same values
        /// </summary>
        /// <returns>The generated hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = Entity.GetHashCode();
            hashCode = (hashCode * 251) + Type.GetHashCode();
            hashCode = (hashCode * 251) + StartIndex.GetHashCode();
            hashCode = (hashCode * 251) + EndIndex.GetHashCode();
            hashCode = (hashCode * 251) + Score.GetHashCode();

            return hashCode;
        }
    }
}
