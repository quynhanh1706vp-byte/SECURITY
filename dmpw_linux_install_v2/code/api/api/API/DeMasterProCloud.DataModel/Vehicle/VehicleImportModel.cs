using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Vehicle
{
    public class VehicleImportModel
    {
        public Field<int?> UserId { get; set; }
        public Field<string> PlateNumber { get; set; }
        public Field<string> Model { get; set; }
        public Field<int?> VehicleType { get; set; }
        public Field<string> Color { get; set; }
        public bool IsValid { get; set; } = true;
    
    
        public void SetUserId(string o)
        {
            UserId = new NumField(o, true);
            if (!UserId.IsValid)
            {
                IsValid = false;
            }
        }
    
        public void SetPlateNumber(string o)
        {
            PlateNumber = new StringField(o, true);
            if (!PlateNumber.IsValid)
            {
                IsValid = false;
            }
        }
    
        public void SetModel(string o)
        {
            Model = new StringField(o, true);
            if (!Model.IsValid)
            {
                IsValid = false;
            }
        }    
        public void SetVehicleType(string o)
        {
            VehicleType = new NumField(o, true);
            if (!VehicleType.IsValid)
            {
                IsValid = false;
            }
        }
    
        public void SetColor(string o)
        {
            Color = new StringField(o, true);
            if (!Color.IsValid)
            {
                IsValid = false;
            }
        }
    }
    
    public abstract class Field<T>
    {
        protected Field()
        {
        }
    
        protected Field(string value, bool required)
        {
            Required = required;
            PreValue = value;
            InvokeSetValue(value);
        }
    
        public string PreValue { get; set; }
        public T Value { get; set; }
        public bool IsValid { get; set; } = true;
        public string Error { get; set; } = string.Empty;
        public bool Required { get; set; }
        public abstract void SetValue(string value);
    
        private void InvokeSetValue(string value)
        {
            SetValue(value);
        }
    }
    
    public class StringField : Field<string>
    {
        private int? _maxLenght;
    
        public StringField()
        {
        }
    
        public StringField(string value, bool required) : base(value, required)
        {
        }
    
        public StringField(string value, bool required, int? maxLenght)
        {
            _maxLenght = maxLenght;
            Required = required;
            PreValue = value;
            SetValue(value);
        }
    
        public sealed override void SetValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (Required)
                {
                    IsValid = false;
                    Error = "This field is required.";
                }
                else
                {
                    Value = null;
                    IsValid = true;
                }
    
                return;
            }
    
            try
            {
                Value = value;
                IsValid = true;
    
                if (_maxLenght.HasValue)
                {
                    if (value.Length > _maxLenght.Value)
                    {
                        IsValid = false;
                        Error = $"Field length can not be greater than {_maxLenght} characters.";
                    }
                }
            }
            catch (Exception)
            {
                IsValid = false;
                Error = "Value must be a text";
            }
        }
    
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Value))
            {
                return Value;
            }
    
            return PreValue;
        }
    }
    
    public class NumField : Field<int?>
    {
        public NumField()
        {
        }
    
        public NumField(string value, bool required) : base(value, required)
        {
        }
    
        public override void SetValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (Required)
                {
                    IsValid = false;
                    Error = "This field is required.";
                }
                else
                {
                    Value = null;
                    IsValid = true;
                }
    
                return;
            }
    
            try
            {
                Value = Convert.ToInt32(value);
                IsValid = true;
            }
            catch (Exception)
            {
                IsValid = false;
                Error = "Value must be a number";
            }
        }
    
        public override string ToString()
        {
            if (Value.HasValue)
            {
                return Value.Value.ToString();
            }
    
            return PreValue;
        }
    }
}

