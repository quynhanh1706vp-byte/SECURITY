using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Account;

public class LoginConfigModel
{
    public bool IsFirstLogin { get; set; }
    public DateTime TimeChangedPassWord { get; set; }
    public int LoginFailCount { get; set; }
    public DateTime LastTimeLoginFail { get; set; }
}

public class PasswordStatusResult
{
    public bool IsExpired { get; set; } = false;
    public bool IsFirstLogin { get; set; } = false;
    public bool IsInWarningPeriod { get; set; } = false;
    public bool BlockLogin { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    
    public bool HasWarning => IsFirstLogin || IsInWarningPeriod;
}

public class PasswordValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> ErrorMessages { get; set; } = new List<string>();
    public bool MissingUppercase { get; set; } = false;
    public bool MissingNumber { get; set; } = false;
    public bool MissingSpecialChar { get; set; } = false;
    
    public string GetCombinedMessage() => string.Join(" ", ErrorMessages);
}