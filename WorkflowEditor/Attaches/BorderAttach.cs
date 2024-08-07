using System.Windows;

namespace WorkflowEditor.Attaches
{
    public class BorderAttach
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(BorderAttach), new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.Inherits));

        public static void SetCornerRadius(DependencyObject element, CornerRadius value)
        {
            element.SetValue(CornerRadiusProperty, value);
        }

        public static CornerRadius GetCornerRadius(DependencyObject element)
        {
            return (CornerRadius)element.GetValue(CornerRadiusProperty);
        }
    }
} 