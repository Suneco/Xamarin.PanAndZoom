using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using PanAndZoom;
using PanAndZoom.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(PanZoomView), typeof(PanZoomImageRenderer))]

namespace PanAndZoom.iOS
{
    public class PanZoomImageRenderer : ImageRenderer, IUIGestureRecognizerDelegate
    {
        private const float MinZoomLevel = 1.0f;

        private float _maxZoomLevel;
        private nfloat _currentScale = 1;

        private CGPoint _lastPoint;
        private nfloat _posX;
        private nfloat _posY;

        public PanZoomImageRenderer()
        {
            SetupGestures();
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            if (e.NewElement != null)
            {
                var control = (PanZoomView)e.NewElement;
                _maxZoomLevel = control.MaxZoomLevel;
            }

            base.OnElementChanged(e);
        }

        protected override Task TrySetImage(Image previous = null)
        {
            var result = base.TrySetImage(previous);

            _posX = Bounds.Size.Width / 2;
            _posY = Bounds.Size.Height / 2;

            return result;
        }

        private void SetupGestures()
        {
            UserInteractionEnabled = true;

            var pinchRecognizer = new UIPinchGestureRecognizer(HandlePinch);
            var panRecognizer = new UIPanGestureRecognizer(HandlePan);

            pinchRecognizer.Delegate = this;
            panRecognizer.Delegate = this;

            AddGestureRecognizer(pinchRecognizer);
            AddGestureRecognizer(panRecognizer);

            ContentMode = UIViewContentMode.ScaleAspectFit;
        }

        private void HandlePinch(UIPinchGestureRecognizer recognizer)
        {
            // Prevent the object to become too large or too small
            var newScale = (nfloat)Math.Max(MinZoomLevel, Math.Min(_currentScale * recognizer.Scale, _maxZoomLevel));

            if (_currentScale != newScale)
            {
                _currentScale = newScale;
                Transform = CGAffineTransform.MakeScale(_currentScale, _currentScale);
            }
            recognizer.Scale = 1;
        }

        private void HandlePan(UIPanGestureRecognizer recognizer)
        {
            if (recognizer.State != UIGestureRecognizerState.Began &&
                recognizer.State != UIGestureRecognizerState.Changed)
                return;

            var translation = recognizer.TranslationInView(Superview);

            _posX += translation.X;
            _posY += translation.Y;

            var maxX = (Bounds.Size.Width / 2) * _currentScale;
            var maxY = (Bounds.Size.Height / 2) * _currentScale;

            var minX = -(maxX - Bounds.Size.Width);
            var minY = -(maxY - Bounds.Size.Height);

            if (_posX > maxX)
                _posX = maxX;
            else if (_posX < minX)
                _posX = minX;
            if (_posY > maxY)
                _posY = maxY;
            else if (_posY < minY)
                _posY = minY;

            var translatedCenter = new CGPoint(_posX, _posY);
            Center = translatedCenter;

            recognizer.SetTranslation(CGPoint.Empty, this);
        }

        [Export("gestureRecognizer:shouldRecognizeSimultaneouslyWithGestureRecognizer:")]
        public bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
        {
            return true;
        }
    }
}
