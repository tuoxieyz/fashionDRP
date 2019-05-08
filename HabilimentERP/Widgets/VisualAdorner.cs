using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace HabilimentERP
{
    public class VisualAdorner : Adorner
    {
        private Rectangle _child = null;
        private double _leftOffset = 0;
        private double _topOffset = 0;

        public VisualAdorner(UIElement ue, UIElement visual)
            : base(ue)
        {
            VisualBrush vb = new VisualBrush(visual);

            _child = new Rectangle();

            _child.Width = ue.RenderSize.Width;
            _child.Height = ue.RenderSize.Height;


            DoubleAnimation animation = new DoubleAnimation(0.3, 1, new Duration(TimeSpan.FromSeconds(1)));
            animation.AutoReverse = true;
            animation.RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever;
            vb.BeginAnimation(System.Windows.Media.Brush.OpacityProperty, animation);

            _child.Fill = vb;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _child.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _child;
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        public double LeftOffset
        {
            get
            {
                return _leftOffset;
            }
            set
            {
                _leftOffset = value - _child.ActualWidth / 2;
                UpdatePosition();
            }
        }

        public double TopOffset
        {
            get
            {
                return _topOffset;
            }
            set
            {
                _topOffset = value - _child.ActualHeight / 2;
                UpdatePosition();

            }
        }

        private void UpdatePosition()
        {
            AdornerLayer adornerLayer = this.Parent as AdornerLayer;
            if (adornerLayer != null)
            {
                adornerLayer.Update(AdornedElement);
            }
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();

            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(this._leftOffset, this._topOffset));
            return result;
        }
    }
}
