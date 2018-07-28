﻿namespace Common.EasyMarkup
{
    using System;
    using System.Globalization;
    using Common;

    public class EmProperty<T> : EmProperty, ISerialConfirmation where T : IConvertible
    {
        public bool HasValue { get; private set; } = false;

        protected T DefaultValue { get; private set; } = default(T);

        private T ObjValue;

        public T Value
        {
            get => ObjValue;
            set
            {
                ObjValue = value;
                HasValue = true;
                SerializedValue = ObjValue?.ToString(CultureInfo.InvariantCulture);
            }
        }

        public EmProperty(string key, T defaultValue = default(T))
        {
            Key = key;
            ObjValue = defaultValue;
            DefaultValue = defaultValue;
            SerializedValue = ObjValue?.ToString(CultureInfo.InvariantCulture);
        }

        protected override string ExtractValue(StringBuffer fullString)
        {
            string serialValue = base.ExtractValue(fullString);

            Value = ConvertFromSerial(serialValue);

            HasValue = true;

            return serialValue;
        }

        public virtual T ConvertFromSerial(string value)
        {
            var type = typeof(T);

            if (type.IsEnum)
                return (T)Enum.Parse(type, value, true);
            else
                return (T)Convert.ChangeType(value, typeof(T));
        }

        internal override EmProperty Copy()
        {
            if (HasValue)
                return new EmProperty<T>(Key, ObjValue);

            return new EmProperty<T>(Key, DefaultValue);
        }
    }

}

