using System;
using UnityEngine;

namespace Kinetix.Internal
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class AnimationQueue
    {
        #region Members
        public AnimationIds[]      m_animationsIds;
        public AnimationClip[]     m_animationsClips;
        public bool                m_loop;
        #endregion

        public AnimationQueue(AnimationIds[] m_animationsIds, bool m_loop = false)
        {
            this.m_animationsIds = m_animationsIds;
            this.m_loop = m_loop;
            if (m_animationsIds != null && m_animationsIds.Length > 0)
                m_animationsClips = new AnimationClip[m_animationsIds.Length];
            else
                m_animationsClips = null;
        }

        #region Private Manipulators

        #endregion


        #region Public Manipulators
        public string Serialize()
        {
            return JsonUtility.ToJson(this);
        }

        public AnimationQueue Deserialize(string _JSON)
        {
            return JsonUtility.FromJson<AnimationQueue>(_JSON);
        }
        #endregion
    }
}
