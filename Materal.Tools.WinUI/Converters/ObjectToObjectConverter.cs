using Microsoft.UI.Xaml.Data;

namespace Materal.Tools.WinUI.Converters
{
    /// <summary>
    /// 对象转换器
    /// </summary>
    public partial class ObjectToObjectConverter : IValueConverter
    {
        /// <summary>
        /// 值
        /// </summary>
        public object? Value { get; set; }
        /// <summary>
        /// 相等时的值
        /// </summary>
        public object? EqualValue { get; set; }
        /// <summary>
        /// 不相等时的值
        /// </summary>
        public object? NotEqualValue { get; set; }
        /// <inheritdoc/>
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            object trueValue = value;
            Type valueType = trueValue.GetType();
            if (valueType.IsEnum)
            {
                Type enumType = valueType.GetEnumUnderlyingType();
                if (enumType == typeof(int))
                {
                    trueValue = System.Convert.ToInt32(trueValue);
                }
                else if (enumType == typeof(byte))
                {
                    trueValue = System.Convert.ToByte(trueValue);
                }
            }
            if (trueValue is null && Value is null || trueValue is not null && trueValue.Equals(Value))
            {
                return EqualValue;
            }
            else
            {
                return NotEqualValue;
            }
        }
        /// <inheritdoc/>

        public object? ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is null && EqualValue is null || value is not null && value.Equals(EqualValue))
            {
                return Value;
            }
            else
            {
                return value;
            }
        }
    }
}
