/*
* Original by DYLAN ENGELMAN http://jupiterlighthousestudio.com/custom-inspectors-unity/
* Altered by Brecht Lecluyse http://www.brechtos.comS
*/

using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class TagSelectorAttribute : PropertyAttribute {
    public bool useDefaultTagFieldDrawer = false;
}
