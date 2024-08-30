using UnityEngine;

[System.Serializable]
public struct SerVector3
{
    public float x;
    public float y;
    public float z;

    public SerVector3(float xVec, float yVec, float zVec){
        x = xVec;
        y = yVec;
        z = zVec;
    }
    public static implicit operator Vector3(SerVector3 v){
        return new Vector3(v.x, v.y, v.z);
    }
    public static implicit operator SerVector3(Vector3 v){
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
    public SerQuaternion(float xVec, float yVec, float zVec, float wVec){
        x = xVec;
        y = yVec;
        z = zVec;
        w = wVec;
    }
    public static implicit operator Quaternion(SerQuaternion q){
        return new Quaternion(q.x, q.y, q.z, q.w);
    }
    public static implicit operator SerQuaternion(Quaternion q){
        return new SerQuaternion(q.x, q.y, q.z, q.w);
    }
}