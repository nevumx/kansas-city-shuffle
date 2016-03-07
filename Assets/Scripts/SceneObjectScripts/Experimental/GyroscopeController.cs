using UnityEngine;
using System;
using System.Collections;
using Nx;

public class GyroscopeController : MonoBehaviour
{
	public float MaxAngularDeviation = 20.0f;
	public float MinAngularVelocity = 10.0f;
	public float MaxAngularVelocity = 70.0f;
	public float AngularEqualityEpsilon = 0.1f;
	public bool KeepScreenOn = true;
	
	static int _gyroscopeEnabledCount = 0;
	
	private float _xAngularVelocity = 0.0f;
	private float _yAngularVelocity = 0.0f;
	
	private float _xStartAngle = 0.0f;
	private float _yStartAngle = 0.0f;

	private Vector3 _previousTargetRot = Vector3.zero;
	private Vector3 _previousActualRot = Vector3.zero;
	
	[SerializeField]
	private Transform _gyroHelper;
	
	void Awake()
	{
		_gyroscopeEnabledCount++;
		Input.gyro.enabled = true;
		if (KeepScreenOn)
		{
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}
		if (_gyroHelper == null)
		{
			_gyroHelper = new GameObject("GyroSimulator").transform;
			_gyroHelper.transform.parent = transform.parent;
			_gyroHelper.transform.localPosition = Vector3.zero;
			_gyroHelper.transform.localRotation = Quaternion.identity;
			_gyroHelper.transform.localScale = Vector3.one;
		}
	}
	
	void OnDestroy()
	{
		_gyroscopeEnabledCount--;
		if (_gyroscopeEnabledCount <= 0)
		{
			_gyroscopeEnabledCount = 0;
			Input.gyro.enabled = false;
		}
	}

	void Update()
	{
		if (Input.touches.Exists(t => t.phase == TouchPhase.Began))
		{
			_gyroHelper.gameObject.SetActive(!_gyroHelper.gameObject.activeSelf);
			if (Input.touchCount >= 2)
			{
				_gyroHelper.localRotation = Quaternion.identity;
			}
		}

		_gyroHelper.localRotation *= Quaternion.Euler(Vector3.Scale(Input.gyro.rotationRate, new Vector3(1.0f, 1.0f, -1.0f)));
		_gyroHelper.localRotation *= Quaternion.Euler(new Vector3(0, 0, -ConvertToPositiveOrNegative180DegreeSpace(_gyroHelper.localEulerAngles.z)));
		Vector3 targetRot = ClampEulerAngles(_gyroHelper.eulerAngles, MaxAngularDeviation);
		Vector3 actualRot = ConvertEulerAnglesToPositiveOrNegative180DegreeSpace(transform.localEulerAngles);
		
		if ((targetRot.x - actualRot.x < 0.0f) != (_previousTargetRot.x - _previousActualRot.x < 0.0f))
		{
			_xStartAngle = actualRot.x;
		}
		if ((targetRot.y - actualRot.y < 0.0f) != (_previousTargetRot.y - _previousActualRot.y < 0.0f))
		{
			_yStartAngle = actualRot.y;
		}
		
		_xAngularVelocity = GetVelocityFunction(_xStartAngle, targetRot.x)(actualRot.x);
		_yAngularVelocity = GetVelocityFunction(_yStartAngle, targetRot.y)(actualRot.y);
		
		Vector3 deltaRot = new Vector3(_xAngularVelocity, _yAngularVelocity, -ConvertToPositiveOrNegative180DegreeSpace(transform.localEulerAngles.z)) * Time.deltaTime;
		
		transform.localRotation *= Quaternion.Euler(deltaRot);
		transform.localRotation *= Quaternion.Euler(new Vector3(0, 0, -ConvertToPositiveOrNegative180DegreeSpace(transform.localEulerAngles.z)));
		
		_previousTargetRot = targetRot;
		_previousActualRot = actualRot;
	}
	
	private Vector3 ClampEulerAngles(Vector3 eulerAngles, float maxDeviation)
	{
		eulerAngles = ConvertEulerAnglesToPositiveOrNegative180DegreeSpace(eulerAngles);
		return new Vector3(Mathf.Clamp(eulerAngles.x, -maxDeviation, maxDeviation),
		                   Mathf.Clamp(eulerAngles.y, -maxDeviation, maxDeviation),
		                   Mathf.Clamp(eulerAngles.z, -maxDeviation, maxDeviation));
	}
	
	private Vector3 ConvertEulerAnglesToPositiveOrNegative180DegreeSpace(Vector3 eulerAngles)
	{
		
		return new Vector3(ConvertToPositiveOrNegative180DegreeSpace(eulerAngles.x),
		                   ConvertToPositiveOrNegative180DegreeSpace(eulerAngles.y),
		                   ConvertToPositiveOrNegative180DegreeSpace(eulerAngles.z));
	}
	
	private float ConvertToPositiveOrNegative180DegreeSpace(float angle)
	{
		return Mathf.Repeat(angle + 180.0f, 360.0f) - 180.0f;
	}
	
	private Func<float, float> GetVelocityFunction(float originalAngle, float targetAngle)
	{
		float a = 0.0f;
		float b = 0.0f;
		float aDenominator = 2.0f * Mathf.Pow(Mathf.Sqrt(MaxAngularVelocity) / MaxAngularDeviation, 2.0f) * (originalAngle - targetAngle);
		bool destReached = aDenominator == 0 || Mathf.Abs(originalAngle - targetAngle) <= AngularEqualityEpsilon;
		if (!destReached)
		{
			a = (-Mathf.Pow(Mathf.Sqrt(MaxAngularVelocity) / MaxAngularDeviation, 2.0f)
			     * (targetAngle * targetAngle - originalAngle * originalAngle)) / aDenominator;
			b = Mathf.Pow(Mathf.Sqrt(MaxAngularVelocity) / MaxAngularDeviation * (originalAngle - a), 2.0f);
		}
		return (angle) =>
		{
			if (destReached)
			{
				return 0.0f;
			}
			
			float proposedResult1 = -Mathf.Pow(Mathf.Sqrt(MaxAngularVelocity) / MaxAngularDeviation * (angle - a), 2.0f) + b;
			float proposedResult2 = Mathf.Pow(Mathf.Sqrt(MaxAngularVelocity) / MaxAngularDeviation * (angle - a), 2.0f) - b;
			
			if (Mathf.Abs(targetAngle - angle) >= Mathf.Abs(angle - originalAngle))
			{
				return originalAngle < targetAngle
					? Mathf.Max(proposedResult1, MinAngularVelocity) : Mathf.Min(proposedResult2, -MinAngularVelocity);
			}
			return originalAngle < targetAngle ? proposedResult1 : proposedResult2;
		};
	}
}