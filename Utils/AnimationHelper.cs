using System.Windows;
using System.Windows.Media.Animation;

namespace SpectrumAnalyzer.Utils
{
    /// <summary>
    /// Provides smooth animation utilities for spectrum visualization
    /// </summary>
    public static class AnimationHelper
    {
        /// <summary>
        /// Creates a smooth height animation for spectrum bars
        /// </summary>
        /// <param name="target">Target element to animate</param>
        /// <param name="newHeight">New height value</param>
        /// <param name="duration">Animation duration in milliseconds</param>
        public static void AnimateHeight(FrameworkElement target, double newHeight, int duration = 100)
        {
            if (target == null) return;

            var animation = new DoubleAnimation
            {
                To = newHeight,
                Duration = TimeSpan.FromMilliseconds(duration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            target.BeginAnimation(FrameworkElement.HeightProperty, animation);
        }

        /// <summary>
        /// Creates a glow effect animation
        /// </summary>
        /// <param name="target">Target element to animate</param>
        /// <param name="intensity">Glow intensity (0-1)</param>
        /// <param name="duration">Animation duration in milliseconds</param>
        public static void AnimateGlow(FrameworkElement target, double intensity, int duration = 150)
        {
            if (target == null) return;

            var animation = new DoubleAnimation
            {
                To = intensity,
                Duration = TimeSpan.FromMilliseconds(duration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            target.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        /// <summary>
        /// Creates a smooth scale animation
        /// </summary>
        /// <param name="target">Target element to animate</param>
        /// <param name="scaleX">Scale factor X</param>
        /// <param name="scaleY">Scale factor Y</param>
        /// <param name="duration">Animation duration in milliseconds</param>
        public static void AnimateScale(FrameworkElement target, double scaleX, double scaleY, int duration = 200)
        {
            if (target?.RenderTransform is not System.Windows.Media.ScaleTransform scaleTransform)
                return;

            var animationX = new DoubleAnimation
            {
                To = scaleX,
                Duration = TimeSpan.FromMilliseconds(duration),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
            };

            var animationY = new DoubleAnimation
            {
                To = scaleY,
                Duration = TimeSpan.FromMilliseconds(duration),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
            };

            scaleTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, animationX);
            scaleTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, animationY);
        }
    }
}