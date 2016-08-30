using UnityEngine;

[System.Serializable]
public class CardAnimationData : ScriptableObject
{
	[Header("Game Sartup Animation Data")]
	[SerializeField] private float _deckFillFancyIntroTweenHeight;
	[SerializeField] private float _deckFillFancyIntroCardSpawnDistance;
	[SerializeField] private float _deckShuffleExplosionSphereRadius;
	[SerializeField] private float _deckShuffleExplosionDuration;
	[SerializeField] private float _deckShuffleExplosionMaxRotations;
	[SerializeField] private float _timeToWaitBeforePopulatingDeck;

	[Header("General Game Animation Data")]
	[SerializeField] private float _deckFillDurationPerCard;
	[SerializeField] private float _cardCreationTotalDuration;
	[SerializeField] private float _deckFillDelayPerCard;
	[SerializeField] private float _deckRefillDelayPerCard;
	[SerializeField] private float _generalCardMoveHeight;
	[SerializeField] private float _generalCardMoveDuration;
	[SerializeField] private float _cardDragSubmitTweenHeight;
	[SerializeField] private float _consectiveCardDealDelay;
	[SerializeField] private float _consectiveCardSubmitDelay;
	[SerializeField] private float _cardFloatingHeight;
	[SerializeField] private float _cardStateChangeDuration;
	[SerializeField] private float _cameraShuffleTweenUpAmount;
	[SerializeField] private float _undoTurnDelay;

	public float DeckFillFancyIntroTweenHeight			{ get { return _deckFillFancyIntroTweenHeight; } }
	public float DeckFillFancyIntroCardSpawnDistance	{ get { return _deckFillFancyIntroCardSpawnDistance; } }
	public float DeckShuffleExplosionSphereRadius		{ get { return _deckShuffleExplosionSphereRadius; } }
	public float DeckShuffleExplosionDuration			{ get { return _deckShuffleExplosionDuration; } }
	public float DeckShuffleExplosionMaxRotations		{ get { return _deckShuffleExplosionMaxRotations; } }
	public float TimeToWaitBeforePopulatingDeck			{ get { return _timeToWaitBeforePopulatingDeck; } }
	public float DeckFillDurationPerCard				{ get { return _deckFillDurationPerCard; } }
	public float CardCreationTotalDuration				{ get { return _cardCreationTotalDuration; } }
	public float DeckFillDelayPerCard					{ get { return _deckFillDelayPerCard; } }
	public float DeckRefillDelayPerCard					{ get { return _deckRefillDelayPerCard; } }
	public float GeneralCardMoveHeight					{ get { return _generalCardMoveHeight; } }
	public float GeneralCardMoveDuration				{ get { return _generalCardMoveDuration; } }
	public float CardDragSubmitTweenHeight				{ get { return _cardDragSubmitTweenHeight; } }
	public float ConsecutiveCardDealDelay				{ get { return _consectiveCardDealDelay; } }
	public float ConsecutiveCardSubmitDelay				{ get { return _consectiveCardSubmitDelay; } }
	public float CardFloatingHeight						{ get { return _cardFloatingHeight; } }
	public float CardStateChangeDuration				{ get { return _cardStateChangeDuration; } }
	public float CameraShuffleTweenUpAmount				{ get { return _cameraShuffleTweenUpAmount; } }
	public float UndoTurnDelay							{ get { return _undoTurnDelay; } }
}
