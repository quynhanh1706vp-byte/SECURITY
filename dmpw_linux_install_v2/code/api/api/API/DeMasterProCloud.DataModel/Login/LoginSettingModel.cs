
namespace DeMasterProCloud.DataModel.Login;

public class LoginSettingModel
{
    public bool ChangeInFirstTime { get; set; }
    public bool HaveUpperCase { get; set; }
    public bool HaveNumber { get; set; }
    public bool HaveSpecial { get; set; }
    public int TimeNeedToChange { get; set; }
    public int MaximumTimeUsePassword { get; set; }
    public int MaximumEnterWrongPassword { get; set; }
    public double TimeoutWhenWrongPassword { get; set; }
}