using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.IO;
using System.Windows.Markup;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace WorkflowEditor.PublicMethods
{
    public static class ExtensionMethod
    {
        public static double Adsorb(this double value, double gridSize)
        {
            if (double.IsNaN(value))
            {
                return value;
            }
            int quotient = (int)(value / gridSize);
            var min = Math.Max(0, gridSize * quotient);
            var max = min + gridSize;

            if (value - min > gridSize / 2)
            {
                return max;
            }
            else
            {
                return min;
            }
        }

        public static T GetFirstVisualHit<T>(this Visual visual, System.Windows.Point point) where T : DependencyObject
        {
            List<T> hitElements = new List<T>();

            // 使用HitTest方法和回调函数获取所有命中的控件
            VisualTreeHelper.HitTest(
                visual,
                null,
                new HitTestResultCallback(
                    result =>
                    {
                        HitTestResultBehavior behavior = HitTestResultBehavior.Continue;
                        if (result.VisualHit is FrameworkElement frameworkElement && frameworkElement.TemplatedParent is T t && frameworkElement.IsVisible)
                        {
                            hitElements.Add(t);
                        }
                        return behavior;
                    }),
                new PointHitTestParameters(point));

            return hitElements.FirstOrDefault();
        }

        public static T GetVisualHit<T>(this Visual visual, System.Windows.Point point)
        {
            var hitObject = VisualTreeHelper.HitTest(visual, point)?.VisualHit;
            if (hitObject == null)
            {
                return default;
            }
            else
            {
                return hitObject.FindParentObject<T>();
            }
        }

        public static SolidColorBrush ToSolidColorBrush(this string hexString)
        {
            return new BrushConverter().ConvertFromString(hexString) as SolidColorBrush;
        }

        public static T FindParentObject<T>(this DependencyObject obj)
        {
            DependencyObject parent = obj is Visual or Visual3D ? VisualTreeHelper.GetParent(obj) : LogicalTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is T t)
                {
                    return t;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return default;
        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <param name="childName">x:Name or Name of child.</param>
        /// <returns>The first parent item that matches the submitted type parameter. If not matching item can be found, a null parent is being returned.</returns>
        public static T FindChild<T>(this DependencyObject parent, string childName = null) where T : DependencyObject
        {
            if (parent == null)
            {
                return null;
            }

            T val = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is not T)
                {
                    val = child.FindChild<T>(childName);
                    if (val != null)
                    {
                        break;
                    }

                    continue;
                }

                if (!string.IsNullOrEmpty(childName))
                {
                    if (child is IFrameworkInputElement frameworkInputElement && frameworkInputElement.Name == childName)
                    {
                        val = (T)child;
                        break;
                    }

                    val = child.FindChild<T>(childName);
                    if (val != null)
                    {
                        break;
                    }

                    continue;
                }

                val = (T)child;
                break;
            }

            return val;
        }

        /// <summary>
        /// Analyzes both visual and logical tree in order to find all elements of a given type that are descendants of the source item.
        /// </summary>
        /// <typeparam name="T">The type of the queried items.</typeparam>
        /// <param name="source">The root element that marks the source of the search. If the source is already of the requested type, it will not be included in the result.</param>
        /// <param name="forceUsingTheVisualTreeHelper">Sometimes it's better to search in the VisualTree (e.g. in tests)</param>
        /// <returns>All descendants of source that match the requested type.</returns>
        public static IEnumerable<T> FindChildren<T>(this DependencyObject source, bool forceUsingTheVisualTreeHelper = false) where T : DependencyObject
        {
            if (source == null)
            {
                yield break;
            }

            IEnumerable<DependencyObject> childObjects = source.GetChildObjects(forceUsingTheVisualTreeHelper);
            foreach (DependencyObject child in childObjects)
            {
                if (child != null && child is T t)
                {
                    yield return t;
                }

                foreach (T item in child.FindChildren<T>(forceUsingTheVisualTreeHelper))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// This method is an alternative to WPF's System.Windows.Media.VisualTreeHelper.GetChild(System.Windows.DependencyObject,System.Int32)
        /// method, which also supports content elements.Keep in mind that for content elements, this method falls back to the logical tree of the element.
        /// </summary>
        /// <param name="parent">The item to be processed.</param>
        /// <param name="forceUsingTheVisualTreeHelper">Sometimes it's better to search in the VisualTree (e.g. in tests)</param>
        /// <returns>The submitted item's child elements, if available.</returns>
        public static IEnumerable<DependencyObject> GetChildObjects(this DependencyObject parent, bool forceUsingTheVisualTreeHelper = false)
        {
            if (parent == null)
            {
                yield break;
            }

            if (!forceUsingTheVisualTreeHelper && (parent is ContentElement || parent is FrameworkElement))
            {
                foreach (object child in LogicalTreeHelper.GetChildren(parent))
                {
                    if (child is DependencyObject dependencyObject)
                    {
                        yield return dependencyObject;
                    }
                }
            }
            else if (parent is Visual || parent is Visual3D)
            {
                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    yield return VisualTreeHelper.GetChild(parent, i);
                }
            }
        }

        public static BitmapImage ToBitmapImage(this Bitmap bitmap, ImageFormat imageFormat = null, bool isDisposeBitmap = true)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }
            try
            {
                using MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, imageFormat ?? ImageFormat.Bmp);
                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
            finally
            {
                if (isDisposeBitmap)
                {
                    bitmap?.Dispose();
                }
            }
        }
    }

    public class EnumBindingSourceExtension : MarkupExtension
    {
        private Type _enumType;
        public Type EnumType
        {
            get => _enumType;
            set
            {
                if (value != _enumType)
                {
                    if (value != null)
                    {
                        Type enumType = Nullable.GetUnderlyingType(value) ?? value;
                        if (!enumType.IsEnum)
                        {
                            throw new ArgumentException("Type must be for an Enum.");
                        }
                    }
                    _enumType = value;
                }
            }
        }

        public EnumBindingSourceExtension() { }

        public EnumBindingSourceExtension(Type enumType)
        {
            EnumType = enumType;
        }

        public EnumBindingSourceExtension(Type enumType, bool isIgnoreZero) : this(enumType)
        {
            IsIgnoreZero = isIgnoreZero;
        }

        public bool IsIgnoreZero { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (EnumType == null)
            {
                throw new InvalidOperationException("The EnumType must be specified.");
            }

            Type actualEnumType = Nullable.GetUnderlyingType(EnumType) ?? EnumType;
            Array enumValues = Enum.GetValues(actualEnumType);

            if (actualEnumType == EnumType)
            {
                if (IsIgnoreZero)
                {
                    return enumValues.Cast<object>().Where(x => Convert.ToInt64(x) != 0);
                }
                return enumValues;
            }

            Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return tempArray;
        }
    }

    public class EnumDescriptionConverter : EnumConverter
    {
        public EnumDescriptionConverter(Type type) : base(type) { }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return GetEnumDesc(value);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public static string GetEnumDesc(object obj)
        {
            if (obj != null)
            {
                FieldInfo fieldInfo = obj.GetType().GetField(obj.ToString());
                if (fieldInfo != null)
                {
                    if (fieldInfo.GetCustomAttribute<DescriptionAttribute>(false) is DescriptionAttribute descriptionAttribute && !string.IsNullOrEmpty(descriptionAttribute.Description))
                    {
                        return descriptionAttribute.Description;
                    }
                    else
                    {
                        return obj.ToString();
                    }
                }
            }
            return string.Empty;
        }
    }
}