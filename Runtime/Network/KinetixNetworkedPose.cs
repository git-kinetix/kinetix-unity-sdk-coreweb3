using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Kinetix.Internal
{
    [System.Serializable]
    public class KinetixNetworkedPose
    {
        [SerializeField] public Guid guid;
        [SerializeField] public double timestamp;
        [SerializeField] public bool posEnabled = false;
        [SerializeField] public bool scaleEnabled = false;
        [SerializeField] public BonePoseInfo[] bones;

        public byte[] ToByteArray()
		{
			if (bones.Length == 0)
				return new byte[0];

			using MemoryStream m = new MemoryStream();
			using BinaryWriter b = new BinaryWriter(m, Encoding.Default, true);

            b.Write(guid.ToString());
            b.Write(bones.Length);
            b.Write(posEnabled);
            b.Write(scaleEnabled);

			for (int i = 0; i < bones.Length; i++)
			{
                // -- Rotation --------------------------//
                b.Write(KinetixCustomShort.FloatToCustomHalf(bones[i].localRotation.x));
                b.Write(KinetixCustomShort.FloatToCustomHalf(bones[i].localRotation.y));
                b.Write(KinetixCustomShort.FloatToCustomHalf(bones[i].localRotation.z));
                b.Write(KinetixCustomShort.FloatToCustomHalf(bones[i].localRotation.w));

                // -- Position --------------------------//
                if (posEnabled || i == 0) {
                    b.Write(KinetixCustomShort.FloatToCustomHalf(bones[i].localPosition.x));
                    b.Write(KinetixCustomShort.FloatToCustomHalf(bones[i].localPosition.y));
                    b.Write(KinetixCustomShort.FloatToCustomHalf(bones[i].localPosition.z));
                }

                // -- Scale -----------------------------//
                if (scaleEnabled) {
                    b.Write(KinetixCustomShort.FloatToCustomHalf(bones[i].localScale.x));
                    b.Write(KinetixCustomShort.FloatToCustomHalf(bones[i].localScale.y));
                    b.Write(KinetixCustomShort.FloatToCustomHalf(bones[i].localScale.z));
                }
			}

			b.Dispose();
            
            return m.ToArray();
		}

        public static KinetixNetworkedPose FromByte(byte[] poseData)
		{
            if (poseData.Length == 0) return null;

            using MemoryStream m = new MemoryStream(poseData);
			using BinaryReader reader = new BinaryReader(m, Encoding.Default, true);

            KinetixNetworkedPose pose = new KinetixNetworkedPose();

            string guid = reader.ReadString();

            if (!Guid.TryParse(guid, out pose.guid)) {
                return null;
            }

            int boneCount = reader.ReadInt32();

            pose.posEnabled = reader.ReadBoolean();
            pose.scaleEnabled = reader.ReadBoolean();

            pose.bones = new BonePoseInfo[boneCount];

            for (int i = 0; i < boneCount; i++)
			{
                BonePoseInfo boneInfo = new BonePoseInfo();

                boneInfo.localRotation = new Quaternion {
					x = KinetixCustomShort.CustomHalfToFloat(reader.ReadInt16()),
					y = KinetixCustomShort.CustomHalfToFloat(reader.ReadInt16()),
					z = KinetixCustomShort.CustomHalfToFloat(reader.ReadInt16()),
					w = KinetixCustomShort.CustomHalfToFloat(reader.ReadInt16())
				};

                if (pose.posEnabled || i == 0)
                    boneInfo.localPosition = new Vector3 {
                        x = KinetixCustomShort.CustomHalfToFloat(reader.ReadInt16()),
                        y = KinetixCustomShort.CustomHalfToFloat(reader.ReadInt16()),
                        z = KinetixCustomShort.CustomHalfToFloat(reader.ReadInt16())
                    };

                if (pose.scaleEnabled)
                    boneInfo.localScale = new Vector3 {
                        x = KinetixCustomShort.CustomHalfToFloat(reader.ReadInt16()),
                        y = KinetixCustomShort.CustomHalfToFloat(reader.ReadInt16()),
                        z = KinetixCustomShort.CustomHalfToFloat(reader.ReadInt16())
                    };

                pose.bones[i] = boneInfo;
			}

            reader.Dispose();

            return pose;
        }
    }

    [System.Serializable]
    public class BonePoseInfo
    {
        [SerializeField] public Vector3 localPosition;
        [SerializeField] public Quaternion localRotation;
        [SerializeField] public Vector3 localScale;

        public BonePoseInfo(Transform origin)
        {
            if (origin == null) return;

            localPosition = origin.localPosition;
            localRotation = origin.localRotation;
            localScale = origin.localScale;
        }

        public BonePoseInfo()
        {
            localPosition = Vector3.zero;
            localRotation = Quaternion.identity;
            localScale = Vector3.zero;
        }
    }
}
