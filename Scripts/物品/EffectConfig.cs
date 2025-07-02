using UnityEngine;

[CreateAssetMenu(fileName = "特效配置", menuName = "公用/特效/特效配置")]
public class EffectConfig : ScriptableObject
{
    public string 前摇特效;
    public string 施法特效;
    public string 飞行特效;
    public string 击中特效;
    public string 目标特效;
    public SoundConfig 音效配置;
    public CameraEffect 镜头效果;
}

[System.Serializable]
public class SoundConfig
{
    public string 施法音效;
    public string 击中音效;
}

[System.Serializable]
public class CameraEffect
{
    public float 震荡强度;
    public float 震荡时间;
} 