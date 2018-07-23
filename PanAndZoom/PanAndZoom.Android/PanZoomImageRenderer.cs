using Android.Content;
using Android.Graphics;
using Android.Views;
using PanAndZoom;
using PanAndZoom.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Math = System.Math;

[assembly: ExportRenderer(typeof(PanZoomView), typeof(PanZoomImageRenderer))]
namespace PanAndZoom.Droid
{
    public sealed class PanZoomImageRenderer : ImageRenderer
    {
        private const int InvalidPointerId = -1;
        private const float MinZoomLevel = 1.0f;

        private float _maxZoomLevel;

        private float _posX;
        private float _posY;

        private float _lastTouchX;
        private float _lastTouchY;
        private float _lastGestureX;
        private float _lastGestureY;
        private int _activePointerId = InvalidPointerId;

        private int _viewHeight;
        private int _viewWidth;

        private readonly ScaleGestureDetector _scaleDetector;
        private float _scaleFactor = 1.0f;

        public PanZoomImageRenderer(Context context) : base(context)
        {
            _scaleDetector = new ScaleGestureDetector(context, new ScaleListener(this));

            SetWillNotDraw(false);
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

        public override bool OnTouchEvent(MotionEvent e)
        {
            // Let the detectors inspect the touch events.
            _scaleDetector.OnTouchEvent(e);

            switch (e.Action & MotionEventActions.Mask)
            {
                case MotionEventActions.Down:
                    if (!_scaleDetector.IsInProgress)
                    {
                        _lastTouchX = e.GetX();
                        _lastTouchY = e.GetY();
                        _activePointerId = e.GetPointerId(0);
                    }
                    break;
                case MotionEventActions.Pointer1Down:
                    if (_scaleDetector.IsInProgress)
                    {
                        _lastGestureX = _scaleDetector.FocusX;
                        _lastGestureY = _scaleDetector.FocusY;
                    }
                    break;
                case MotionEventActions.Move:
                    // Only move if the ScaleGestureDetector isn't processing a gesture.
                    if (!_scaleDetector.IsInProgress)
                    {
                        var pointerIndex = e.FindPointerIndex(_activePointerId);
                        var x = e.GetX(pointerIndex);
                        var y = e.GetY(pointerIndex);

                        _posX += x - _lastTouchX;
                        _posY += y - _lastTouchY;

                        _lastTouchX = x;
                        _lastTouchY = y;
                    }
                    else
                    {
                        var x = _scaleDetector.FocusX;
                        var y = _scaleDetector.FocusY;

                        _posX += x - _lastGestureX;
                        _posY += y - _lastGestureY;

                        _lastGestureX = x;
                        _lastGestureY = y;
                    }

                    Invalidate();

                    break;
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    _activePointerId = InvalidPointerId;
                    break;
                case MotionEventActions.PointerUp:
                    var index = ((int)e.Action & (int)MotionEventActions.PointerIndexMask) >> (int)MotionEventActions.PointerIndexShift;
                    var pointerId = e.GetPointerId(index);
                    if (pointerId == _activePointerId)
                    {
                        // This was our active pointer going up. Choose a new
                        // active pointer and adjust accordingly.
                        var pointerIndex = index == 0 ? 1 : 0;
                        _lastTouchX = e.GetX(pointerIndex);
                        _lastTouchY = e.GetY(pointerIndex);
                        _activePointerId = e.GetPointerId(pointerIndex);
                    }
                    else
                    {
                        var tempPointerIndex = e.FindPointerIndex(_activePointerId);
                        _lastTouchX = e.GetX(tempPointerIndex);
                        _lastTouchY = e.GetY(tempPointerIndex);
                    }
                    break;
            }
            return true;
        }

        protected override void OnDraw(Canvas canvas)
        {
            // Calculate the boundaries of the canvas
            var minX = (int)((_viewWidth / _scaleFactor) - canvas.Width);
            var minY = (int)((_viewHeight / _scaleFactor) - canvas.Height);

            if (_posX > 0)
                _posX = 0;
            else if (_posX < minX)
                _posX = minX;
            if (_posY > 0)
                _posY = 0;
            else if (_posY < minY)
                _posY = minY;

            // Change image position
            canvas.Scale(_scaleFactor, _scaleFactor);
            canvas.Translate(_posX, _posY);

            base.OnDraw(canvas);
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            _viewHeight = h;
            _viewWidth = w;
        }

        private class ScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener
        {
            private readonly PanZoomImageRenderer _parent;

            public ScaleListener(PanZoomImageRenderer parent)
            {
                _parent = parent;
            }

            public override bool OnScale(ScaleGestureDetector detector)
            {
                _parent._scaleFactor *= detector.ScaleFactor;

                // Don't let the object get too small or too large.
                _parent._scaleFactor = Math.Max(MinZoomLevel, Math.Min(_parent._scaleFactor, _parent._maxZoomLevel));

                _parent.Invalidate();
                return true;
            }
        }
    }
}
