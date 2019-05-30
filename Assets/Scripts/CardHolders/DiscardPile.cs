using UnityEngine;

public class DiscardPile : CardPile
{
	[SerializeField]	private	TextMesh	_pileSizeText;
						public	GameObject	PileSizeText	{ get { return _pileSizeText.gameObject; } }

	protected override void OnCardSent(CardController sentCard)
	{
		base.OnCardSent(sentCard);
		UpdatePileSizeText();
	}

	protected override void OnCardRecieveTweenFinished(CardController card)
	{
		base.OnCardRecieveTweenFinished(card);
		UpdatePileSizeText();
	}

	private void UpdatePileSizeText()
	{
		int numCardsVisuallyInPile = ReadOnlyCards.Count - _CardsInTransition.Count;
		if (numCardsVisuallyInPile > 0)
		{
			_pileSizeText.text = numCardsVisuallyInPile.ToString();
		}
		else
		{
			_pileSizeText.text = string.Empty;
		}
	}
}
