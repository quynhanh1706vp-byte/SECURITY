using System;
using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using OfficeOpenXml.Style;

namespace DeMasterProCloud.DataModel.User
{
    public class ResultImported
    {
        public bool Result { get; set; }
        public string Message { get; set; }
    }

    public class ExcelCardData
    {
        /// <summary>
        /// Card Id
        /// </summary>
        public string CardId { get; set; }

        /// <summary>
        /// Issue count
        /// </summary>
        public int IssueCount { get; set; }
    }

    public class UserImportExportModel
    {
        public Field<string> FirstName { get; set; }
        public Field<string> LastName { get; set; }
        public Field<bool?> Sex { get; set; }
        public Field<string> Position { get; set; }
        public Field<string> CompanyPhone { get; set; }
        public Field<string> PostCode { get; set; }
        public Field<string> Job { get; set; }
        public Field<string> Address { get; set; }
        public Field<string> DepartmentName { get; set; }
        public Field<string> ParentDepartmentName { get; set; }
        public Field<string> TypeOfWork { get; set; }
        public Field<string> DepartmentNo { get; set; }
        public Field<string> CardId { get; set; }
        public Field<string> EmployeeNumber { get; set; }
        public Field<string> ExpiredDate { get; set; }
        public Field<string> EffectiveDate { get; set; }
        public Field<string> Nationality { get; set; }
        public Field<string> City { get; set; }
        public Field<string> HomePhone { get; set; }
        public Field<string> Responsibility { get; set; }
        public Field<string> Remarks { get; set; }
        public bool IsValid { get; set; } = true;
        public Field<string> Birthday { get; set; }


        public Field<string> UserCode { get; set; }
        
        public Field<string> Note { get; set; }
        public Field<string> AccessGroupName { get; set; }
        public Field<int?> AccessGroupId { get; set; }
        public Field<int?> IssueCount { get; set; }
        public Field<bool?> IsMasterCard { get; set; }
        public Field<int?> CardStatus { get; set; }
        
        public Field<string> CardType { get; set; }
        
        public Field<string> Email { get; set; }
        public Field<string> Avatar { get; set; }



        public void SetUserCode(string o)
        {
            UserCode = new StringField(o, true);
            if (!UserCode.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetAccessGroupName(string o)
        {
            AccessGroupName = new StringField(o, true);
            if (!AccessGroupName.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetAccessGroupId(string o)
        {
            AccessGroupId = new NumField(o, true);
        }

        public void SetIssueCount(string o)
        {
            IssueCount = new NumField(o, true);
            if (!IssueCount.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetIsMasterCard(string o)
        {
            IsMasterCard = new BooleanField(o, false);
            if (!IsMasterCard.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetCardStatus(string o)
        {
            //var cardStatusId = (int)Enum.Parse(typeof(CardStatus), o);
            var cardStatusId = 0;
            switch (o)
            {
                case Constants.CardStatus.Normal_EN:
                case Constants.CardStatus.Normal_KR:
                case Constants.CardStatus.Normal_JP:
                    cardStatusId = 0;
                    break;
                case Constants.CardStatus.Temp_EN:
                case Constants.CardStatus.Temp_KR:
                case Constants.CardStatus.Temp_JP:
                    cardStatusId = 1;
                    break;
                case Constants.CardStatus.Retire_EN:
                case Constants.CardStatus.Retire_KR:
                case Constants.CardStatus.Retire_JP:
                    cardStatusId = 2;
                    break;
                case Constants.CardStatus.Lost_EN:
                case Constants.CardStatus.Lost_KR:
                case Constants.CardStatus.Lost_JP:
                    cardStatusId = 3;
                    break;
                case Constants.CardStatus.Invalid_EN:
                case Constants.CardStatus.Invalid_KR:
                case Constants.CardStatus.Invalid_JP:
                    cardStatusId = 4;
                    break;
                default:
                    cardStatusId = 0;
                    break;
            }
            CardStatus = new NumField(cardStatusId.ToString(), true);
            if (!CardStatus.IsValid)
            {
                IsValid = false;
            }
        }
        
        public void SetCardType(string o)
        {
            CardType = new StringField(o, true);
            if (!CardType.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetFirstName(string o)
        {
            FirstName = new StringField(o, true, 100);
            if (!FirstName.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetLastName(string o)
        {
            LastName = new StringField(o, false, 100);
            if (!LastName.IsValid)
            {
                IsValid = false;
            }
        }
        
        public void SetEmail(string o)
        {
            Email = new StringField(o, false, 100);
            if (!Email.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetSex(string o)
        {
            switch (o.ToLower())
            {
                case Constants.Sex.Male_EN:
                case Constants.Sex.Male_en:
                case Constants.Sex.Male_KR:
                case Constants.Sex.Male_JP:
                    Sex = new BooleanField("true", true);
                    break;

                case Constants.Sex.Female_EN:
                case Constants.Sex.Female_en:
                case Constants.Sex.Female_KR:
                case Constants.Sex.Female_JP:
                    Sex = new BooleanField("false", true);
                    break;

                default:
                    IsValid = false;
                    break;
            }

        }

        //public void SetKeyPadPassword(string o)
        //{
        //    KeyPadPassword = new StringField(o, false, 8);
        //    if (!KeyPadPassword.IsValid)
        //    {
        //        IsValid = false;
        //    }
        //}

        //public void SetIssuedDate(string o)
        //{
        //    IssuedDate = new DateTimeField(o, false);
        //    if (!IssuedDate.IsValid)
        //    {
        //        IsValid = false;
        //    }
        //}

        public void SetPosition(string o)
        {
            Position = new StringField(o, false, 100);
            if (!Position.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetCompanyPhone(string o)
        {
            CompanyPhone = new StringField(o, false, 20);
            if (!CompanyPhone.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetPostCode(string o)
        {
            PostCode = new StringField(o, false, 20);
            if (!PostCode.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetJob(string o)
        {
            Job = new StringField(o, false, 100);
            if (!Job.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetAddress(string o)
        {
            Address = new StringField(o, false, 100);
            if (!Address.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetDepartment(string o)
        {
            DepartmentName = new StringField(o, true, 100);
            if (!DepartmentName.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetParentDepartment(string o)
        {
            ParentDepartmentName = new StringField(o, true, 100);
            if (!ParentDepartmentName.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetTypeOfWork(string o)
        {
            TypeOfWork = new StringField(o, true);
            if (!TypeOfWork.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetDepartmentNo(string o)
        {
            DepartmentNo = new StringField(o, true, 50);
            if (!DepartmentNo.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetCardId(string o)
        {
            CardId = new StringField(o, true, 50);
            if (!CardId.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetEmployeeNumber(string o)
        {
            EmployeeNumber = new StringField(o, false);
            if (!EmployeeNumber.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetExpiredDate(string o)
        {
            ExpiredDate = new StringField(o, false);
            if (!ExpiredDate.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetEffectiveDate(string o)
        {
            EffectiveDate = new StringField(o, false);
            if (!EffectiveDate.IsValid)
            {
                IsValid = false;
            }
        }
        
        public void SetBirthdayDate(string o)
        {
            Birthday = new StringField(o, false);
            if (!Birthday.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetNationality(string o)
        {
            Nationality = new StringField(o, false, 100);
            if (!Nationality.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetCity(string o)
        {
            City = new StringField(o, false, 100);
            if (!City.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetHomePhone(string o)
        {
            HomePhone = new StringField(o, false, 20);
            if (!HomePhone.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetResponsibility(string o)
        {
            Responsibility = new StringField(o, false, 100);
            if (!Responsibility.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetRemarks(string o)
        {
            Remarks = new StringField(o, false);
            if (!Remarks.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetAvatar(string o, List<string> whiteList)
        {
            if (!string.IsNullOrEmpty(o) && whiteList.Any(m => m == o.Substring(0, m.Length)))
            {
                Avatar = new StringField(o, false);
                if (!Avatar.IsValid)
                {
                    IsValid = false;
                }
            }
        }
    }
    public class UserImportExportModelCsv
    {
        public Field<string> FirstName { get; set; }
        public Field<string> LastName { get; set; }
        public Field<bool?> Sex { get; set; }
        public Field<string> Position { get; set; }
        public Field<string> CompanyPhone { get; set; }
        public Field<string> PostCode { get; set; }
        public Field<string> Job { get; set; }
        public Field<string> Address { get; set; }
        public Field<string> DepartmentName { get; set; }
        public Field<string> ParentDepartmentName { get; set; }
        public Field<int?> TypeOfWork { get; set; }
        public Field<string> DepartmentNo { get; set; }
        public Field<string> CardId { get; set; }
        public Field<string> EmployeeNumber { get; set; }
        public Field<DateTime?> ExpiredDate { get; set; }
        public Field<DateTime?> EffectiveDate { get; set; }
        public Field<string> Nationality { get; set; }
        public Field<string> City { get; set; }
        public Field<string> HomePhone { get; set; }
        public Field<string> Responsibility { get; set; }
        public Field<string> Remarks { get; set; }
        public bool IsValid { get; set; } = true;
        public Field<DateTime?> Birthday { get; set; }


        public Field<string> UserCode { get; set; }
        
        public Field<string> Note { get; set; }
        public Field<string> AccessGroupName { get; set; }
        public Field<int?> AccessGroupId { get; set; }
        public Field<int?> IssueCount { get; set; }
        public Field<bool?> IsMasterCard { get; set; }
        public Field<int?> CardStatus { get; set; }
        
        public Field<string> CardType { get; set; }
        
        public Field<string> Email { get; set; }
        public Field<string> Avatar { get; set; }



        public void SetUserCode(string o)
        {
            UserCode = new StringField(o, true);
            if (!UserCode.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetAccessGroupName(string o)
        {
            AccessGroupName = new StringField(o, true);
            if (!AccessGroupName.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetAccessGroupId(string o)
        {
            AccessGroupId = new NumField(o, true);
        }

        public void SetIssueCount(string o)
        {
            IssueCount = new NumField(o, true);
            if (!IssueCount.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetIsMasterCard(string o)
        {
            IsMasterCard = new BooleanField(o, false);
            if (!IsMasterCard.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetCardStatus(string o)
        {
            //var cardStatusId = (int)Enum.Parse(typeof(CardStatus), o);
            var cardStatusId = 0;
            switch (o)
            {
                case Constants.CardStatus.Normal_EN:
                case Constants.CardStatus.Normal_KR:
                case Constants.CardStatus.Normal_JP:
                    cardStatusId = 0;
                    break;
                case Constants.CardStatus.Temp_EN:
                case Constants.CardStatus.Temp_KR:
                case Constants.CardStatus.Temp_JP:
                    cardStatusId = 1;
                    break;
                case Constants.CardStatus.Retire_EN:
                case Constants.CardStatus.Retire_KR:
                case Constants.CardStatus.Retire_JP:
                    cardStatusId = 2;
                    break;
                case Constants.CardStatus.Lost_EN:
                case Constants.CardStatus.Lost_KR:
                case Constants.CardStatus.Lost_JP:
                    cardStatusId = 3;
                    break;
                case Constants.CardStatus.Invalid_EN:
                case Constants.CardStatus.Invalid_KR:
                case Constants.CardStatus.Invalid_JP:
                    cardStatusId = 4;
                    break;
                default:
                    cardStatusId = 0;
                    break;
            }
            CardStatus = new NumField(cardStatusId.ToString(), true);
            if (!CardStatus.IsValid)
            {
                IsValid = false;
            }
        }
        
        public void SetCardType(string o)
        {
            CardType = new StringField(o, true);
            if (!CardType.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetFirstName(string o)
        {
            FirstName = new StringField(o, true, 100);
            if (!FirstName.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetLastName(string o)
        {
            LastName = new StringField(o, false, 100);
            if (!LastName.IsValid)
            {
                IsValid = false;
            }
        }
        
        public void SetEmail(string o)
        {
            Email = new StringField(o, false, 100);
            if (!Email.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetSex(string o)
        {
            switch (o.ToLower())
            {
                case Constants.Sex.Male_EN:
                case Constants.Sex.Male_en:
                case Constants.Sex.Male_KR:
                case Constants.Sex.Male_JP:
                    Sex = new BooleanField("true", true);
                    break;

                case Constants.Sex.Female_EN:
                case Constants.Sex.Female_en:
                case Constants.Sex.Female_KR:
                case Constants.Sex.Female_JP:
                    Sex = new BooleanField("false", true);
                    break;

                default:
                    IsValid = false;
                    break;
            }

        }

        //public void SetKeyPadPassword(string o)
        //{
        //    KeyPadPassword = new StringField(o, false, 8);
        //    if (!KeyPadPassword.IsValid)
        //    {
        //        IsValid = false;
        //    }
        //}

        //public void SetIssuedDate(string o)
        //{
        //    IssuedDate = new DateTimeField(o, false);
        //    if (!IssuedDate.IsValid)
        //    {
        //        IsValid = false;
        //    }
        //}

        public void SetPosition(string o)
        {
            Position = new StringField(o, false, 100);
            if (!Position.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetCompanyPhone(string o)
        {
            CompanyPhone = new StringField(o, false, 20);
            if (!CompanyPhone.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetPostCode(string o)
        {
            PostCode = new StringField(o, false, 20);
            if (!PostCode.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetJob(string o)
        {
            Job = new StringField(o, false, 100);
            if (!Job.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetAddress(string o)
        {
            Address = new StringField(o, false, 100);
            if (!Address.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetDepartment(string o)
        {
            DepartmentName = new StringField(o, true, 100);
            if (!DepartmentName.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetParentDepartment(string o)
        {
            ParentDepartmentName = new StringField(o, true, 100);
            if (!ParentDepartmentName.IsValid)
            {
                IsValid = false;
            }
        }
        public void SetTypeOfWork(string o)
        {
            TypeOfWork = new NumField(o, true);
            if (!TypeOfWork.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetDepartmentNo(string o)
        {
            DepartmentNo = new StringField(o, true, 50);
            if (!DepartmentNo.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetCardId(string o)
        {
            CardId = new StringField(o, true, 50);
            if (!CardId.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetEmployeeNumber(string o)
        {
            EmployeeNumber = new StringField(o, false);
            if (!EmployeeNumber.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetExpiredDate(string o, ExcelStyle style)
        {
            ExpiredDate = new DateTimeField(o, style, false);
            if (!ExpiredDate.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetEffectiveDate(string o, ExcelStyle style)
        {
            EffectiveDate = new DateTimeField(o, style, false);
            if (!EffectiveDate.IsValid)
            {
                IsValid = false;
            }
        }
        
        public void SetBirthdayDate(string o, ExcelStyle style)
        {
            Birthday = new DateTimeField(o, style, false);
            if (!Birthday.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetNationality(string o)
        {
            Nationality = new StringField(o, false, 100);
            if (!Nationality.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetCity(string o)
        {
            City = new StringField(o, false, 100);
            if (!City.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetHomePhone(string o)
        {
            HomePhone = new StringField(o, false, 20);
            if (!HomePhone.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetResponsibility(string o)
        {
            Responsibility = new StringField(o, false, 100);
            if (!Responsibility.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetRemarks(string o)
        {
            Remarks = new StringField(o, false);
            if (!Remarks.IsValid)
            {
                IsValid = false;
            }
        }

        public void SetAvatar(string o, List<string> whiteList)
        {
            if (!string.IsNullOrEmpty(o) && whiteList.Any(m => m == o.Substring(0, m.Length)))
            {
                Avatar = new StringField(o, false);
                if (!Avatar.IsValid)
                {
                    IsValid = false;
                }
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

        protected Field(string value, ExcelStyle style, bool required)
        {
            PreValue = value;
            Style = style;
            Required = required;

            InvokeSetValue(value, style);
        }

        public string PreValue { get; set; }

        public ExcelStyle Style { get; set; }

        public T Value { get; set; }
        public bool IsValid { get; set; } = true;
        public string Error { get; set; } = string.Empty;
        public bool Required { get; set; }
        public abstract void SetValue(string value, ExcelStyle style = null);
        private void InvokeSetValue(string value, ExcelStyle style = null)
        {
            SetValue(value, style);
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

        public sealed override void SetValue(string value, ExcelStyle style = null)
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

        public override void SetValue(string value, ExcelStyle style = null)
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

        public override void SetValue(string value, ExcelStyle style = null)
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

        public DateTimeField(string value, ExcelStyle style, bool required) : base(value, style, required)
        {

        }

        public override void SetValue(string value, ExcelStyle style = null)
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
                if (style != null)
                {
                    // check NumberFormat ID
                    // typdId = 0 (General)
                    // typeId = 14 (Date)
                    // typeId = 49 (Text)
                    // typeId = 176 (CustomField)

                    switch (style.Numberformat.NumFmtID)
                    {
                        case 0:
                        case 49:
                        case 164:
                            Value = value.Contains("-") ? DateTime.ParseExact(value, Constants.DateTimeFormat.YyyyyMdDdFormat, null) : DateTime.ParseExact(value, Constants.DateTimeFormat.YyyyMMdd, null);
                            break;
                        case 14:
                            Value = DateTime.Parse(value);
                            break;
                        case 166:
                        case 176:
                            Value = DateTime.FromOADate(double.Parse(value));
                            break;
                    }
                }
                else
                {
                    Value = DateTime.FromOADate(double.Parse(value));
                }

                IsValid = true;
            }
            catch (Exception e)
            {
                IsValid = false;
                //Error = "Value must be a valid date with format yyyy-MM-dd";
                Error = $"Value has wrong format. Data : {PreValue}";
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