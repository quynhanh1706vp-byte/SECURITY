namespace DeMasterProCloud.DataModel.Setting;

public class PasswordSettingModel
{
    public bool ChangeInFirstTime { get; set; }
    public bool HaveUpperCase { get; set; }
    public bool HaveNumber { get; set; }
    public bool HaveSpecial { get; set; }
    public long TimeNeedToChange { get; set; }
    public long MaximumTimeUsePassword { get; set; }
    public long MaximumEnterWrongPassword { get; set; }
    public long TimeoutWhenWrongPassword { get; set; }
}