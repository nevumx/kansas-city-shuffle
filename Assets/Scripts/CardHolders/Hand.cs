using UnityEngine;
using System.Collections.Generic;
using Nx;

public class Hand : DynamicCardHolder
{
	[SerializeField]	private	float	_handMaxSizeInUnits	= 8.2f;

	protected override void RepositionCards()
	{
		Vector3 leftmostPosition = transform.position - transform.right * _handMaxSizeInUnits / 2.0f;
		float distBetweenCards = _handMaxSizeInUnits / (Mathf.Max(1.0f, (float)CardCount) + 1.0f);
		CardModViewtroller bestCardToMimic = _CardsInTransition.Best((a, b) => a.TweenHolder.TimeRemaining < b.TweenHolder.TimeRemaining);

		for (int i = 0, iMax = ReadOnlyCards.Count; i < iMax; ++i)
		{
			if (ReadOnlyCards[i].ViewFSM.State != CardModViewtroller.CardViewFSM.AnimState.SELECTED)
			{
				TweenHolder cardShiftTween = ReadOnlyCards[i].TweenHolder;
				Vector3 targetPosition = leftmostPosition + transform.right * distBetweenCards * (i + 1);

				IncrementalPositionTween posTweenToShift;
				if ((posTweenToShift = cardShiftTween.GetTweenOfType<IncrementalPositionTween>()) == null) // Careful (=)
				{
					ReadOnlyCards[i].AddIncrementalPositionTween(targetPosition)
									.SetShouldChangeLayer(false);
				}
				else
				{
					posTweenToShift.PositionTo = targetPosition;
				}

				if (!_CardsInTransition.Contains(ReadOnlyCards[i]))
				{
					if (bestCardToMimic != null)
					{
						if (Mathf.Approximately(ReadOnlyCards[i].TweenHolder.Duration, ReadOnlyCards[i].TweenHolder.TimeRemaining))
						{
							ReadOnlyCards[i].TweenHolder.SetDuration(bestCardToMimic.TweenHolder.TimeRemaining);
						}
						else
						{
							ReadOnlyCards[i].TweenHolder.SetDuration(bestCardToMimic.TweenHolder.Duration);
						}
					}
					else
					{
						ReadOnlyCards[i].TweenHolder.SetDuration(CardAnimationData.GeneralCardMoveDuration);
					}
				}
			}
		}
	}

	protected override Vector3 GetCardPositionAtIndex(int index)
	{
		Vector3 firstCardPosition = transform.position - transform.right * _handMaxSizeInUnits / 2.0f;
		firstCardPosition += (index + 1.0f) * transform.right * _handMaxSizeInUnits / (Mathf.Max(1.0f, (float)CardCount) + 1.0f);
		return firstCardPosition;
	}
}