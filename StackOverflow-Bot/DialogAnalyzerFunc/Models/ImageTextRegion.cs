using System;
using System.Runtime.Serialization;

namespace DialogAnalyzerFunc.Models
{
    [DataContract]
    public class ImageTextRegion
    {
        /// <summary>
        /// The height of this Region
        /// </summary>
        [DataMember]
        public int Height { get; set; }

        /// <summary>
        /// The text value of this Text Region
        /// </summary>
        [DataMember]
        public string Text { get; set; }

        /// <summary>
        /// The width of this Region
        /// </summary>
        [DataMember]
        public int Width { get; set; }

        /// <summary>
        /// The x-coordinate of the upper-left corner of this Region
        /// </summary>
        [DataMember]
        public int X { get; set; }

        /// <summary>
        /// The y-coordinate of the upper-left corner of this Region
        /// </summary>
        [DataMember]
        public int Y { get; set; }

        public int CenterX
        {
            get
            {
                return (this.Width / 2) + this.X;
            }
        }

        public int CenterY
        {
            get
            {
                return (this.Height / 2) + this.Y;
            }
        }

        public int Bottom
        {
            get
            {
                return this.Y + this.Height;
            }
        }
    }
}
