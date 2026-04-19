using UnityEngine;

public class Utils
{
	/// Clamp magnitude of vector
	public static Vector3 ClampMagnitude(Vector3 v, float maxLength)
	{
		if (v.sqrMagnitude > (maxLength*maxLength))
		{
			return v.normalized * maxLength;
		}

		return new Vector3(v.x, v.y, v.z);
	}
}