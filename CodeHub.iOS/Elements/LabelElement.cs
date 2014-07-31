using System.Drawing;
using GitHubSharp.Models;
using MonoTouch.UIKit;
using Xamarin.Utilities.DialogElements;

namespace CodeHub.iOS.Elements
{
    public class LabelElement : StyledStringElement
    {
        public LabelModel Label { get; private set; }
        public LabelElement(LabelModel m)
            : base(m.Name)
        {
            Label = m;
            Image = CreateImage(m.Color);
        }

        private static UIImage CreateImage(string color)
        {
            try
            {
                var red = color.Substring(0, 2);
                var green = color.Substring(2, 2);
                var blue = color.Substring(4, 2);

                var redB = System.Convert.ToByte(red, 16);
                var greenB = System.Convert.ToByte(green, 16);
                var blueB = System.Convert.ToByte(blue, 16);

                var size = new SizeF(28f, 28f);
                var cgColor = UIColor.FromRGB(redB, greenB, blueB).CGColor;

                UIGraphics.BeginImageContextWithOptions(size, false, 0);
                var ctx = UIGraphics.GetCurrentContext();
                ctx.SetLineWidth(1.0f);
                ctx.SetStrokeColor(cgColor);
                ctx.AddEllipseInRect(new RectangleF(0, 0, size.Width, size.Height));
                ctx.SetFillColor(cgColor);
                ctx.FillPath();

                var image = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
                return image;
            }
            catch
            {
                return null;
            }
        }
    }

}