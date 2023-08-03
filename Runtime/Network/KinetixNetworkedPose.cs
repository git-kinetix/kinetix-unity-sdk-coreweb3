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
		[SerializeField] public bool hasBlendshape = false;
		[SerializeField] public BonePoseInfo[] bones;
		[SerializeField] public float[] blendshapes;

		[SerializeField] public bool hasArmature;
		[SerializeField] public BonePoseInfo armature;

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

			int length = bones.Length;
			for (int i = 0; i < length; i++)
			{
				// -- Rotation --------------------------//
				b.Write(KinetixCustomHalf.FloatToCustomHalf(bones[i].localRotation.x));
				b.Write(KinetixCustomHalf.FloatToCustomHalf(bones[i].localRotation.y));
				b.Write(KinetixCustomHalf.FloatToCustomHalf(bones[i].localRotation.z));
				b.Write(KinetixCustomHalf.FloatToCustomHalf(bones[i].localRotation.w));

				// -- Position --------------------------//
				if (posEnabled || i == 0) {
					b.Write(KinetixCustomHalf.FloatToCustomHalf(bones[i].localPosition.x));
					b.Write(KinetixCustomHalf.FloatToCustomHalf(bones[i].localPosition.y));
					b.Write(KinetixCustomHalf.FloatToCustomHalf(bones[i].localPosition.z));
				}

				// -- Scale -----------------------------//
				if (scaleEnabled) {
					b.Write(KinetixCustomHalf.FloatToCustomHalf(bones[i].localScale.x));
					b.Write(KinetixCustomHalf.FloatToCustomHalf(bones[i].localScale.y));
					b.Write(KinetixCustomHalf.FloatToCustomHalf(bones[i].localScale.z));
				}
			}

			b.Write(hasArmature);
			if (hasArmature)
			{
				// -- Rotation --------------------------//
				b.Write(KinetixCustomHalf.FloatToCustomHalf(armature.localRotation.x));
				b.Write(KinetixCustomHalf.FloatToCustomHalf(armature.localRotation.y));
				b.Write(KinetixCustomHalf.FloatToCustomHalf(armature.localRotation.z));
				b.Write(KinetixCustomHalf.FloatToCustomHalf(armature.localRotation.w));

				// -- Position --------------------------//
				if (posEnabled)
				{
					b.Write(KinetixCustomHalf.FloatToCustomHalf(armature.localPosition.x));
					b.Write(KinetixCustomHalf.FloatToCustomHalf(armature.localPosition.y));
					b.Write(KinetixCustomHalf.FloatToCustomHalf(armature.localPosition.z));
				}

				// -- Scale -----------------------------//
				if (scaleEnabled)
				{
					b.Write(KinetixCustomHalf.FloatToCustomHalf(armature.localScale.x));
					b.Write(KinetixCustomHalf.FloatToCustomHalf(armature.localScale.y));
					b.Write(KinetixCustomHalf.FloatToCustomHalf(armature.localScale.z));
				}
			}

			//NOTE: Assume blendshape is defined
			length = (int)ARKitBlendshapes.Count;
			for (int i = 0; i < length; i++)
			{
				b.Write(KinetixCustomHalf.FloatToCustomHalf(blendshapes[i]));
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
#pragma warning disable IDE0017 // Simplify object initialisation
				BonePoseInfo boneInfo = new BonePoseInfo();
#pragma warning restore IDE0017

				boneInfo.localRotation = new Quaternion {
					x = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16()),
					y = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16()),
					z = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16()),
					w = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16())
				};

				if (pose.posEnabled || i == 0)
					boneInfo.localPosition = new Vector3 {
						x = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16()),
						y = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16()),
						z = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16())
					};

				if (pose.scaleEnabled)
					boneInfo.localScale = new Vector3 {
						x = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16()),
						y = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16()),
						z = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16())
					};

				pose.bones[i] = boneInfo;
			}

			pose.hasArmature = reader.ReadBoolean();
			if (pose.hasArmature)
			{
#pragma warning disable IDE0017 // Simplify object initialisation
				pose.armature = new BonePoseInfo();
#pragma warning restore IDE0017

				pose.armature.localRotation = new Quaternion
				{
					x = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16()),
					y = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16()),
					z = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16()),
					w = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16())
				};

				if (pose.posEnabled)
					pose.armature.localPosition = new Vector3
					{
						x = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16()),
						y = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16()),
						z = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16())
					};

				if (pose.scaleEnabled)
					pose.armature.localScale = new Vector3
					{
						x = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16()),
						y = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16()),
						z = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16())
					};
			}

			int length = (int)ARKitBlendshapes.Count;
			pose.blendshapes = new float[length];
			for (int i = 0; i < length; i++)
			{
				pose.blendshapes[i] = KinetixCustomHalf.CustomHalfToFloat(reader.ReadInt16());
			}


			reader.Dispose();

			return pose;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bones">Human bones</param>
		/// <param name="transforms"></param>
		/// <param name="blendshape">Can be null if there is no blendshape. Note: This parameter is directly assigned to <see cref="blendshapes"/></param>
		/// <param name="frame">Current frame of the animation</param>
		/// <param name="sendLocalPosition"></param>
		/// <param name="sendLocalScale"></param>
		/// <returns></returns>
		public static KinetixNetworkedPose FromClip(HumanBodyBones[] bones, TransformData[] transforms, float[] blendshape, TransformData? armature, int frame, bool sendLocalPosition, bool sendLocalScale)
		{
			int count = transforms.Length;
			KinetixNetworkedPose toReturn = new KinetixNetworkedPose
			{
				bones = new BonePoseInfo[count]
			};

			bool hasPose = false;
			bool hasScale = false;
			for (int i = 0; i < count; i++)
			{
				TransformData transformData = transforms[i];
				hasPose	 |= transformData.position.HasValue;
				hasScale |= transformData.scale.HasValue;
				toReturn.bones[i] = transformData;
			}

			toReturn.posEnabled = sendLocalPosition && hasPose;
			toReturn.scaleEnabled = sendLocalScale && hasScale;
			toReturn.blendshapes = blendshape;

			toReturn.hasArmature = armature.HasValue;
			if (armature.HasValue)
				toReturn.armature = armature.Value; //Don't use nullable operator, there's an implicit cast
			
			return toReturn;
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
			localScale = Vector3.one;
		}

		public static implicit operator BonePoseInfo(TransformData data) 
			=> new BonePoseInfo() 
			{ 
				localPosition = data.position.GetValueOrDefault(Vector3.zero), 
				localRotation = data.rotation.GetValueOrDefault(Quaternion.identity),
				localScale    = data.scale   .GetValueOrDefault(Vector3.one),
			};

		public static implicit operator TransformData(BonePoseInfo data) 
			=> new TransformData() 
			{ 
				position = data.localPosition , 
				rotation = data.localRotation ,
				scale    = data.localScale    ,
			};
	}
}
