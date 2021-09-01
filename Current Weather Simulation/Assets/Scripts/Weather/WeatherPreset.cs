using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Weather Preset", menuName = "Scriptables/Weather Preset", order = 2)]
public class WeatherPreset : ScriptableObject
{
    public int weatherId;
    public ParticleSystem weatherParticleSystem;
}