using UnityEngine;

[CreateAssetMenu]
public class ThrusterAsset : ScriptableObject
{
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
    [HideInInspector]
    public AnimationCurve inversedCurve = AnimationCurve.Linear(0,0,1,1);
 
    public static implicit operator AnimationCurve(ThrusterAsset me)
    {
        return me.curve;
    }
    public static implicit operator ThrusterAsset(AnimationCurve curve)
    {
        ThrusterAsset asset = ScriptableObject.CreateInstance<ThrusterAsset>();
        asset.curve = curve;
        asset.inversedCurve = inverseCurve(asset.curve);
        return asset;
    }

    private static AnimationCurve inverseCurve(AnimationCurve curve)
    {
        //create inverse speedcurve
        var inverseCurve = new AnimationCurve();
        for (int i = 0; i < curve.length; i++)
        {
            Keyframe inverseKey = new Keyframe(curve.keys[i].value, curve.keys[i].time);
            inverseCurve.AddKey(inverseKey);
        }
        return inverseCurve;

    }
}
