using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using DialogAnalyzerFunc.Models;

namespace DialogAnalyzerFunc.Clients
{
    public class DialogDataInterpreter
    {
        private IEnumerable<DialogLabel> DialogLabels;

        public DialogDataInterpreter(int height, int width, IEnumerable<ImageTextRegion> labels)
        {
            if (labels?.Count() > 0 == false)
            {
                throw new ArgumentNullException("Labels list is not initialized.");
            }

            // 1. Convert text labels
            this.DialogLabels =
                labels.Where(label => string.IsNullOrEmpty(label?.Text) == false).Select(label => new DialogLabel() { Id = Guid.NewGuid(), TextLabel = label }).ToList();

            // 2. Set title label
            this.SetTitleLabel();

            // 3. Set button labels
            this.SetButtonLabels();

            // 4. Set content labels
            this.SetContentLabels();

            // 5. Set results
            this.Result = new DialogAnalysisResult()
            {
                Labels = this.DialogLabels.ToArray()
            };
        }

        /// <summary>
        /// Image data grid for buttons
        /// </summary>
        private ImageDataGrid ButtonsGrid => new ImageDataGrid(0, this.Height - this.DefaultGridHeight, this.DefaultGridHeight, this.Width);

        /// <summary>
        /// Get the height of the default grid
        /// </summary>
        protected int DefaultGridHeight { get; private set; }

        /// <summary>
        /// Height of the image
        /// </summary>
        protected int Height { get; private set; }

        /// <summary>
        /// Determine if the value is valid text
        /// </summary>
        private bool IsValidText(string value)
        {
            return string.IsNullOrEmpty(value) == false && Regex.Match(value, "[a-z]+", RegexOptions.IgnoreCase).Success == true;
        }

        /// <summary>
        /// Determine if the value is with the target and buffer
        /// </summary>
        protected bool IsWithin(int target, int buffer, int value)
        {
            return value >= (target - buffer) && value <= (target + buffer);
        }

        /// <summary>
        /// Width of the image
        /// </summary>
        protected int Width { get; private set; }

        /// <summary>
        /// Analysis result
        /// </summary>
        public DialogAnalysisResult Result { get; protected set; }

        /// <summary>
        /// Set button labels
        /// </summary>
        private void SetButtonLabels()
        {
            if (this.UndefinedLabels.Count() == 0)
            {
                return;
            }

            // Find the labels which are undefined, within the button grid
            IEnumerable<DialogLabel> potentialButtonLabels =
                this.UndefinedLabels.Where(label => this.ButtonsGrid.Contains(label.TextLabel.CenterX, label.TextLabel.CenterY) == true && IsValidText(label.TextLabel.Text) == true);

            if (potentialButtonLabels.Count() > 0)
            {
                // Find the y-coordinate of the lowest label
                int lowestButtom = potentialButtonLabels.OrderByDescending(label => label.TextLabel.Y).First().TextLabel.Bottom;

                IEnumerable<DialogLabel> buttonLabels = potentialButtonLabels.Where(label => this.IsWithin(lowestButtom, YBuffer, label.TextLabel.Bottom));

                // If exist, set label type to button
                foreach (DialogLabel buttonLabel in buttonLabels)
                {
                    buttonLabel.DialogLabelType = DialogLabel.DialogLabelTypes.Button;
                }
            }
        }

        /// <summary>
        /// Set content labels
        /// </summary>
        private void SetContentLabels()
        {
            if (this.UndefinedLabels.Count() == 0)
            {
                return;
            }

            // If exist, set label type to content
            foreach (DialogLabel contentLabel in this.UndefinedLabels)
            {
                if (IsValidText(contentLabel.TextLabel.Text) == true)
                {
                    contentLabel.DialogLabelType = DialogLabel.DialogLabelTypes.Content;
                }
            }
        }

        /// <summary>
        /// Set title label
        /// </summary>
        private void SetTitleLabel()
        {
            if (this.UndefinedLabels.Count() == 0)
            {
                return;
            }

            // Find the label which is on top and within the title grid
            DialogLabel titleLabel =
                this.UndefinedLabels.Where(label => this.TitleGrid.Contains(label.TextLabel.CenterX, label.TextLabel.CenterY) == true
                    && IsValidText(label.TextLabel.Text) == true).OrderBy(label => label.TextLabel.Y).FirstOrDefault();

            // If exist, set label type to title
            if (titleLabel != null)
            {
                titleLabel.DialogLabelType = DialogLabel.DialogLabelTypes.Title;
            }
        }

        /// <summary>
        /// Image data grid for title
        /// </summary>
        private ImageDataGrid TitleGrid => new ImageDataGrid(0, 0, this.DefaultGridHeight, this.Width);

        /// <summary>
        /// Retrive undefined labels
        /// </summary>
        private IEnumerable<DialogLabel> UndefinedLabels => this.DialogLabels.Where(label => label.IsDefined == false);

        /// <summary>
        /// Buffer on the y-axis
        /// </summary>
        protected int YBuffer => 20;

        /// <summary>
        /// Object to define the image data grid
        /// </summary>
        protected class ImageDataGrid
        {
            public ImageDataGrid(int x, int y, int height, int width)
            {
                this.X = x;
                this.Y = y;
                this.Height = height;
                this.Width = width;
            }

            public int Height { get; set; }

            public int Width { get; set; }

            public int X { get; set; }

            public int Y { get; set; }

            public bool Contains(int x, int y)
            {
                return x >= this.X
                    && x <= (this.X + this.Width)
                    && y >= this.Y
                    && y <= (this.Y + this.Height);
            }
        }

    }
}
