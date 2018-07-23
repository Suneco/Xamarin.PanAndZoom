using System.Collections.ObjectModel;
using Xamarin.Forms;
using Image = Xamarin.Forms.Image;

namespace PanAndZoom
{
    /// <inheritdoc />
    /// <summary>
    /// The PanZoomView is a custom Image View which can handle logic such as scaling and moving
    /// See the platform implementations for details.
    /// </summary>
    public class PanZoomView : Image
    {
        public static readonly BindableProperty MaxZoomLevelProperty = BindableProperty.Create(nameof(MaxZoomLevel), typeof(float), typeof(PanZoomView), 10.0f);

        /// <summary>
        /// Defines the maximum zoom level for the image
        /// </summary>
        public float MaxZoomLevel
        {
            get => (float)GetValue(MaxZoomLevelProperty);
            set => SetValue(MaxZoomLevelProperty, value);
        }
    }
}
