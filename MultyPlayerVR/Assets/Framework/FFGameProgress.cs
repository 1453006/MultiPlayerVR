#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FFGameProgress
{
    private const string PlayerPrefsKey = "ff-game-progress";
    private TimeSpan mPlayingTime;
    private DateTime mLoadedTime;

    // do we have modifications to write to disk/cloud?
    private bool mDirty = false;

    FFUserData mUserData;

    public FFGameProgress(FFUserData dt)
    {
        mUserData = dt;
    }

    public bool Dirty
    {
        get
        {
            return mDirty;
        }
        set
        {
            mDirty = value;
        }
    }

    public void LoadFromDisk()
    {
        string s = PlayerPrefs.GetString(PlayerPrefsKey, "");
        if (s == null || s.Trim().Length == 0)
        {
            return;
        }
        FromString(s);
    }

    public void FromBytes(byte[] b)
    {
        FromString(System.Text.ASCIIEncoding.Default.GetString(b));
    }

    public void MergeWith(FFGameProgress other)
    {
        mUserData.MergeWith(other.mUserData);
    }

    public void MergeWithCloud(byte[] cloudData)
    {
        mUserData.MergeWithCloud(cloudData);
    }

    public void SaveToDisk()
    {
        PlayerPrefs.SetString(PlayerPrefsKey, ToSaveString());
        mDirty = false;
    }

    public TimeSpan TotalPlayingTime
    {
        get
        {
            TimeSpan delta = DateTime.Now.Subtract(mLoadedTime);
            return mPlayingTime.Add(delta);
        }
    }

    public byte[] ToBytes()
    {
        return System.Text.ASCIIEncoding.Default.GetBytes(ToSaveString());
    }

    public void FromString(string s)
    {
        if (!String.IsNullOrEmpty(s))
        {
            mUserData.FromString(s);
        }
    }

    public string ToSaveString()
    {
        return mUserData.ToSaveString();
    }
}

public abstract class FFUserData
{
    internal abstract void FromString(string s);
    internal abstract void Initialize();
    internal abstract void MergeWith(FFUserData mUserData);
    internal abstract void MergeWithCloud(byte[] cloudData);
    internal abstract string ToSaveString();
}
#endif