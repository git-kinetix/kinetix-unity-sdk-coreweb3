// // ----------------------------------------------------------------------------
// // <copyright file="AnimationIds.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using UnityEngine;

[Serializable]
public class AnimationIds
{
    // /!\ Not readonly for serialisation
    
    public string UUID;
    public string TokenID;
    public string ContractAddress;


    public AnimationIds(string _UUID, string _TokenID, string _ContractAddress)
    {
        UUID            = _UUID;
        TokenID         = _TokenID;
        ContractAddress = _ContractAddress;
    }

    public AnimationIds(string _UUID)
    {
        UUID            = _UUID;
        TokenID         = string.Empty;
        ContractAddress = string.Empty;
    }

    public string Serialize()
    {
        return JsonUtility.ToJson(this);
    }

    public AnimationIds Deserialize(string _JSON)
    {
        return JsonUtility.FromJson<AnimationIds>(_JSON);
    }

    #region Equals
    public override bool Equals(object obj)
    {
        if ((obj == null) || GetType() != obj.GetType())
            return false;
        AnimationIds animationIds = (AnimationIds)obj;
        if (!string.IsNullOrEmpty(UUID))
            return UUID == animationIds.UUID;
        return (UUID == animationIds.UUID && TokenID == animationIds.TokenID && ContractAddress == animationIds.ContractAddress);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = (UUID != null ? UUID.GetHashCode() : 0);
            if (!string.IsNullOrEmpty(UUID))
                return hashCode;

            
            hashCode = (hashCode * 397) ^ (TokenID != null ? TokenID.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (ContractAddress != null ? ContractAddress.GetHashCode() : 0);
            return hashCode;
        }
    }
    #endregion
    
    public override string ToString()
    {
        return "\n" + "UUID : " + UUID + " // " +
               "Token ID : " + TokenID + " // " + 
               "Contract Address : " + ContractAddress;
    }
}
