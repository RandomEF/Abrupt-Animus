using UnityEngine;

[System.Serializable]
public struct SerVector3
{
    public float x;
    public float y;
    public float z;

    public SerVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public static implicit operator Vector3(SerVector3 v)
    { // This is used for casting from SerVector to Vector3
        return new Vector3(v.x, v.y, v.z);
    }
    public static implicit operator SerVector3(Vector3 v)
    { // This is used for casting from Vector3 to SerVector 
        return new SerVector3(v.x, v.y, v.z);
    }
}

[System.Serializable]
public struct SerQuaternion
{
    public float x;
    public float y;
    public float z;
    public float w;
    public SerQuaternion(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
    public static implicit operator Quaternion(SerQuaternion q)
    { // This is used for casting from SerQuaternion to Quaternion
        return new Quaternion(q.x, q.y, q.z, q.w);
    }
    public static implicit operator SerQuaternion(Quaternion q)
    { // This is used for casting from Quaternion to SerQuaternion
        return new SerQuaternion(q.x, q.y, q.z, q.w);
    }
}