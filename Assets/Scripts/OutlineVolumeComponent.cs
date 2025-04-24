using System;
using UnityEngine;
using UnityEngine.Rendering;

public class OutlineVolumeComponent: VolumeComponent
{
    public ClampedFloatParameter thickness =
        new ClampedFloatParameter(0.05f, 0, 10f);
    public FloatRangeParameter depthRange =
    new FloatRangeParameter(new Vector2(0f, 0.5f), 0f, 5f);

    public ColorParameter color = new ColorParameter(Color.black);
}
