using UnityEngine;
using Nx;

#pragma warning disable IDE0044 // Add readonly modifier

public class Hand : CardHolder
{
	[SerializeField]	private	float	_handMaxSizeInUnits	= 8.2f;

	protected override void RepositionCards()
	{
		Vector3 leftmostPosition = transform.position - transform.right * _handMaxSizeInUnits / 2.0f;
		float distBetweenCards = _handMaxSizeInUnits / (Mathf.Max(1.0f, CardCount) + 1.0f);
		CardController bestCardToMimic = _CardsInTransition.Best((a, b) => a.Holder.TimeRemaining < b.Holder.TimeRemaining);

		for (int i = 0, iMax = ReadOnlyCards.Count; i < iMax; ++i)
		{
			if (ReadOnlyCards[i].ViewFSM.State != CardController.CardViewFSM.AnimState.SELECTED)
			{
				TweenHolder cardShiftTween = ReadOnlyCards[i].Holder;
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
						if (Mathf.Approximately(ReadOnlyCards[i].Holder.Duration, ReadOnlyCards[i].Holder.TimeRemaining))
						{
							ReadOnlyCards[i].Holder.SetDuration(bestCardToMimic.Holder.TimeRemaining);
						}
						else
						{
							ReadOnlyCards[i].Holder.SetDuration(bestCardToMimic.Holder.Duration);
						}
					}
					else
					{
						ReadOnlyCards[i].Holder.SetDuration(_CardAnimationData.GeneralCardMoveDuration);
					}
				}
			}
		}
	}

	protected override Vector3 GetCardPositionAtIndex(int index)
	{
		Vector3 firstCardPosition = transform.position - transform.right * _handMaxSizeInUnits / 2.0f;
		firstCardPosition += (index + 1.0f) * transform.right * _handMaxSizeInUnits / (Mathf.Max(1.0f, CardCount) + 1.0f);
		return firstCardPosition;
	}
}

#pragma warning restore IDE0044 // Add readonly modifier