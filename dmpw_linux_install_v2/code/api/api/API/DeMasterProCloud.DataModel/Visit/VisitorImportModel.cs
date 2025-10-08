using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Visit
{
    public class VisitorImportModel
    {
        public Field<string> VisitorName { get; set; }
        public List<SymptomDetail> Covid19 { get; set; }
        public Field<string> CardId { get; set; }
        public Field<DateTime?> BirthDay { get; set; }
        public Field<string> VisitorDepartment { get; set; }
        public Field<string> VisitorEmpNumber { get; set; }
        public Field<string> Position { get; set; }
        public Field<string> StartDate { get; set; }
        public Field<string> EndDate { get; set; }
        public Field<string> StartDateTime { get; set; }
        public Field<string> EndDateTime { get; set; }
        public Field<string> VisiteeSite { get; set; }
        public Field<int?> VisitType { get; set; }
        public Field<string> VisitReason { get; set; }
        public Field<int?> VisiteeId { get; set; }
        public Field<string> VisiteeName { get; set; }
        public Field<int?> VisiteeDepartmentId { get; set; }
        public Field<string> VisiteeDepartment { get; set; }
        public Field<string> VisiteeEmpNumber { get; set; }
        public Field<string> Phone { get; set; }
        public Field<string> Email { get; set; }
        public Field<string> Address { get; set; }
        public Field<int?> ApproverId2 { get; set; }
        public Field<int?> ApproverId1 { get; set; }
        public Field<int?> AccessGroupId { get; set; }
        public Field<string> NationalIdNumber { get; set; }
        public Field<string> GReCaptchaResponse { get; set; }
        public Field<string> Doors { get; set; }
        public Field<string> AllowedBelonging { get; set; }
        public bool IsValid { get; set; } = true;

        public void SetAccessGroupId(string o)
        {
            AccessGroupId = new NumField(o, true);
            if (!AccessGroupId.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetAddress(string o)
        {
            Address = new StringField(o, true);
            if (!Address.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetAllowedBelonging(string o)
        {
            AllowedBelonging = new StringField(o, true);
            if (!AllowedBelonging.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetApproverId1(string o)
        {
            ApproverId1 = new NumField(o, true);
            if (!ApproverId1.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetApproverId2(string o)
        {
            ApproverId2 = new NumField(o, true);
            if (!ApproverId2.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetBirthDay(string o)
        {
            BirthDay = new DateTimeField(o, true);
            if (!BirthDay.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetDoors(string o)
        {
            Doors = new StringField(o, true);
            if (!Doors.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetEmail(string o)
        {
            Email = new StringField(o, true);
            if (!Email.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetEndDate(string o)
        {
            EndDate = new StringField(o, true);
            if (!EndDate.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetEndDateTime(string o)
        {
            EndDateTime = new StringField(o, true);
            if (!EndDateTime.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetGReCaptchaResponse(string o)
        {
            GReCaptchaResponse = new StringField(o, true);
            if (!GReCaptchaResponse.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetNationalIdNumber(string o)
        {
            NationalIdNumber = new StringField(o, true);
            if (!NationalIdNumber.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetPhone(string o)
        {
            Phone = new StringField(o, true);
            if (!Phone.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetPosition(string o)
        {
            Position = new StringField(o, true);
            if (!Position.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetStartDate(string o)
        {
            StartDate = new StringField(o, true);
            if (!StartDate.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetStartDateTime(string o)
        {
            StartDateTime = new StringField(o, true);
            if (!StartDateTime.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetVisitReason(string o)
        {
            VisitReason = new StringField(o, true);
            if (!VisitReason.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetVisitType(string o)
        {
            VisitType = new NumField(o, true);
            if (!VisitType.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetVisiteeDepartment(string o)
        {
            VisiteeDepartment = new StringField(o, true);
            if (!VisiteeDepartment.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetVisiteeDepartmentId(string o)
        {
            VisiteeDepartmentId = new NumField(o, true);
            if (!VisiteeDepartmentId.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetVisiteeEmpNumber(string o)
        {
            VisiteeEmpNumber = new StringField(o, true);
            if (!VisiteeEmpNumber.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetVisiteeId(string o)
        {
            VisiteeId = new NumField(o, true);
            if (!VisiteeId.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetVisiteeName(string o)
        {
            VisiteeName = new StringField(o, true);
            if (!VisiteeName.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetVisiteeSite(string o)
        {
            VisiteeSite = new StringField(o, true);
            if (!VisiteeSite.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetVisitorDepartment(string o)
        {
            VisitorDepartment = new StringField(o, true);
            if (!VisitorDepartment.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetVisitorEmpNumber(string o)
        {
            VisitorEmpNumber = new StringField(o, true);
            if (!VisitorEmpNumber.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetVisitorName(string o)
        {
            VisitorName = new StringField(o, true);
            if (!VisitorName.IsValid)
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

    public class BooleanField : Field<bool?>
    {
        public BooleanField()
        {
        }

        public BooleanField(string value, bool required) : base(value, required)
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
                Value = Convert.ToBoolean(value);
                IsValid = true;
            }
            catch (Exception)
            {
                IsValid = false;
                Error = "Value must be true or false";
            }
        }

        public override string ToString()
        {
            if (Value.HasValue)
            {
                return Value.Value ? "1" : "0";
            }
            return PreValue;
        }
    }

    public class DateTimeField : Field<DateTime?>
    {
        public DateTimeField()
        {
        }

        public DateTimeField(string value, bool required) : base(value, required)
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
                Value = DateTime.FromOADate(double.Parse(value));
                IsValid = true;
            }
            catch (Exception)
            {
                IsValid = false;
                Error = "Value must be a valid date with format yyyy-MM-dd";
            }
        }
        public override string ToString()
        {
            if (Value.HasValue)
            {
                return Value.Value.ToString("yyyy-MM-dd");
            }
            return PreValue;
        }
    }
}