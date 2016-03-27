using UnityEngine;
using Nx;
using System;

[Serializable]
public struct TweenTransformPair
{
	[SerializeField]	private	Transform	_from;
	[SerializeField]	private	Transform	_to;

						public	Transform	From	{ get { return _from; } }
						public	Transform	To		{ get { return _to; } }
}

public class MainMenuCamera : MonoBehaviour, ITweenable
{
	[SerializeField]	private	TweenTransformPair[]	_tweenTransformPairs		= null;
	[SerializeField]	private	float					_durationForEachTween		= 6.0f;
	[SerializeField]	private	TweenHolder				_tweenHolder;
						public	TweenHolder				TweenHolder					{ get { return _tweenHolder; } }
						private	System.Random			_randomNumber				= new System.Random();
						private	int						_lastTransformPairIndex		= -1;

	[SerializeField]	private	Camera					_cardEffectCamera;
	[SerializeField]	private	Material				_blitOverlayMaterial;
	[SerializeField]	private	Material				_blitFadeAwayMaterial;
						private	RenderTexture			_cardFadeTexture;
						private	RenderTexture			_cardFadeSwapTexture;
						private	int						_framesToClearCardBuffer	= 1;

	private void Start()
	{
		TweenThroughNewTweenTransformPair();

		_cardFadeTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
		_cardFadeSwapTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
		_cardEffectCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);

		RenderTexture.active = _cardEffectCamera.targetTexture;
		GL.Clear(true, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));
		RenderTexture.active = null;
	}

	private void TweenThroughNewTweenTransformPair()
	{
		int nextIndex;
		while ((nextIndex = _randomNumber.Next(_tweenTransformPairs.Length)) == _lastTransformPairIndex);
		_lastTransformPairIndex = nextIndex;
		TweenTransformPair nextPair = _tweenTransformPairs[nextIndex];
		this.AddPositionTween(nextPair.From.position, nextPair.To.position)
			.AddQuaternionRotationTween(nextPair.From.rotation, nextPair.To.rotation)
			.SetDuration(_durationForEachTween)
			.AddToOnFinishedOnce(TweenThroughNewTweenTransformPair);
		_framesToClearCardBuffer = 2;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		_tweenTransformPairs.ForEach(t => 
		{
			if (t.From != null && t.To != null)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(t.From.position, t.To.position);
				for (int i = 0, iMax = 15; i <= iMax; ++i)
				{
					float percentLerped = TweenHolder.EaseInOutAnimationCurve((float) i / iMax);
					Vector3 positionLerp = Vector3.Lerp(t.From.position, t.To.position, percentLerped);
					Gizmos.color = Color.red;
					Gizmos.DrawLine(positionLerp, positionLerp + Vector3.Slerp(t.From.forward, t.To.forward, percentLerped));
				}
			}
		});
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (_framesToClearCardBuffer > 0)
		{
			RenderTexture.active = _cardFadeTexture;
			GL.Clear(true, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));
			RenderTexture.active = null;
			--_framesToClearCardBuffer;
		}

		_blitFadeAwayMaterial.SetFloat("_FadeAmt", Time.deltaTime);

		Graphics.Blit(_cardFadeTexture, _cardFadeSwapTexture);
		_cardFadeTexture.DiscardContents();
		Graphics.Blit(_cardFadeSwapTexture, _cardFadeTexture, _blitFadeAwayMaterial);
		_cardFadeSwapTexture.DiscardContents();

		Graphics.Blit(_cardEffectCamera.targetTexture, _cardFadeTexture, _blitOverlayMaterial);
		Graphics.Blit(_cardFadeTexture, src, _blitOverlayMaterial);
		Graphics.Blit(src, dest);
	}
}
