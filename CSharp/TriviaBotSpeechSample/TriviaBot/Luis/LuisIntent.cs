// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.Serialization;

namespace Luis
{
    /// <summary>
    /// Describes an intent
    /// </summary>
    [DataContract]
    public class LuisIntent : IComparable, IComparable<LuisIntent>
    {
        /// <summary>
        /// Gets or sets the intent name
        /// </summary>
        [DataMember(Name = "intent")]
        public string Intent { get; set; }

        /// <summary>
        /// Gets or sets the certainty for this intent
        /// </summary>
        [DataMember(Name = "score")]
        public double Score { get; set; }

        /// <summary>
        /// Determines if left is equal to right
        /// </summary>
        /// <param name="left">The object to compare</param>
        /// <param name="right">The object to compare against</param>
        /// <returns>The result of the comparison</returns>
        public static bool operator ==(LuisIntent left, LuisIntent right)
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
        public static bool operator !=(LuisIntent left, LuisIntent right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determines if left is less than right
        /// </summary>
        /// <param name="left">The object to compare</param>
        /// <param name="right">The object to compare against</param>
        /// <returns>The result of the comparison</returns>
        public static bool operator <(LuisIntent left, LuisIntent right)
        {
            return Compare(left, right) < 0;
        }

        /// <summary>
        /// Determines if left is greater than right
        /// </summary>
        /// <param name="left">The object to compare</param>
        /// <param name="right">The object to compare against</param>
        /// <returns>The result of the comparison</returns>
        public static bool operator >(LuisIntent left, LuisIntent right)
        {
            return Compare(left, right) > 0;
        }

        /// <summary>
        /// Compare left with right
        /// </summary>
        /// <param name="left">The object to compare</param>
        /// <param name="right">The object to compare against</param>
        /// <returns>The result of the comparison</returns>
        public static int Compare(LuisIntent left, LuisIntent right)
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
            return "{ Intent = " + Intent.ToString() + ", Score = " + Score.ToString(System.Globalization.CultureInfo.InvariantCulture) + " }";
        }

        /// <summary>
        /// Compares this IntentDefinition to the passed in object
        /// </summary>
        /// <param name="obj">The object to compare this IntentDefinition with</param>
        /// <returns>The result of the comparison</returns>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            LuisIntent other = obj as LuisIntent;

            if (other == null)
            {
                throw new ArgumentException("A IntentDefinition object is required for comparison.", "obj");
            }

            return CompareTo(other);
        }

        /// <summary>
        /// Compares this IntentDefinition to the passed in IntentDefinition
        /// </summary>
        /// <param name="other">The IntentDefinition to compare against</param>
        /// <returns>The result of the comparison</returns>
        public int CompareTo(LuisIntent other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return 1;
            }

            var comparison = Score.CompareTo(other.Score);

            if (comparison == 0)
            {
                comparison = string.Compare(Intent, other.Intent, StringComparison.OrdinalIgnoreCase);
            }

            return comparison;
        }

        /// <summary>
        /// Determines if this IntentDefinition is equal to another
        /// </summary>
        /// <param name="obj">The object to compare this IntentDefinition with</param>
        /// <returns>Whether the objects are equal</returns>
        public override bool Equals(object obj)
        {
            var other = obj as LuisIntent;

            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            return CompareTo(other) == 0;
        }

        /// <summary>
        /// A hash code for this object that will always be the same for IntentDefinition objects with the
        /// same values
        /// </summary>
        /// <returns>The generated hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = Intent.GetHashCode();
            hashCode = (hashCode * 251) + Score.GetHashCode();

            return hashCode;
        }
    }
}
