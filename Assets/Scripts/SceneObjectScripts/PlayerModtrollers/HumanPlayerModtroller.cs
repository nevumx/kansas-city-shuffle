using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nx;

#pragma warning disable IDE0044 // Add readonly modifier

public class HumanPlayerModtroller : AbstractPlayerModtroller
{
	private	static	readonly	float					CARD_DRAG_ACCELERATION		= 3.0f;
	public	static	readonly	int[]					EASTER_EGG_CODE				= { 4, 2, 3, 1, 3, 2, 4 };

	private						List<int>				_selectedCardIndexes		= new List<int>();
	private						List<int>				_allowedCardIndexes;
	private						bool					_isSelectingCards;

	private						GameObject				_submitCardsButton;
	private						AdaptiveTutorialSystem	_tutorialSystem;

	private						Queue<int>				_easterEggNumbersEntered	= new Queue<int>();

	public	override			bool					IsHuman						{ get { return true; } }

	public override AbstractPlayerModtroller Init(MainGameModtroller mainGameModtroller)
	{
		AbstractPlayerModtroller toReturn = base.Init(mainGameModtroller);
		_submitCardsButton = _MainGameModtroller.SubmitCardsButton;
		_tutorialSystem = _MainGameModtroller.TutorialSystem;
		return toReturn;
	}

	public override void BeginCardSelection()
	{
		_allowedCardIndexes = GetAllowedCardIndexes();

		_isSelectingCards = true;

		_MainGameModtroller.ProcessCommandSystemOnHumanPlayerTurn();

		ReadOnlyCollection<CardController> cards = Hand.ReadOnlyCards;
		cards.ForEach(c =>
		{
			c.Button.CancelDrag();
			c.Button.ClearAllDelegates();
		});

		for (int i = 0, iMax = cards.Count; i < iMax; ++i)
		{
			if (_allowedCardIndexes.Contains(i))
			{
				SetupInteractionDelegatesForCardAtIndex(i);
			}
		}

		if (_MainGameModtroller.Direction == MainGameModtroller.PlayDirection.UNDECIDED)
		{
			_tutorialSystem.StartTutorialIfNecessary(AdaptiveTutorialSystem.TutorialType.ANY_CARD_TUTORIAL);
		}
		if (_MainGameModtroller.Direction == MainGameModtroller.PlayDirection.UP && _allowedCardIndexes.Count > 0
			&& _allowedCardIndexes.Exists(i => Hand.ReadOnlyCards[i].CardValue >= _MainGameModtroller.DiscardPileLastValue))
		{
			_tutorialSystem.StartTutorialIfNecessary(AdaptiveTutorialSystem.TutorialType.UP_CARD_TUTORIAL);
		}
		if (_MainGameModtroller.Direction == MainGameModtroller.PlayDirection.DOWN && _allowedCardIndexes.Count > 0
			&& _allowedCardIndexes.Exists(i => Hand.ReadOnlyCards[i].CardValue <= _MainGameModtroller.DiscardPileLastValue))
		{
			_tutorialSystem.StartTutorialIfNecessary(AdaptiveTutorialSystem.TutorialType.DOWN_CARD_TUTORIAL);
		}
		if (_allowedCardIndexes.Count <= 0)
		{
			_tutorialSystem.StartTutorialIfNecessary(AdaptiveTutorialSystem.TutorialType.NO_CARD_TUTORIAL);
		}
		if (_allowedCardIndexes.Exists(i => _allowedCardIndexes.Exists(j => Hand.ReadOnlyCards[i].CardValue == Hand.ReadOnlyCards[j].CardValue && i != j)))
		{
			_tutorialSystem.StartTutorialIfNecessary(AdaptiveTutorialSystem.TutorialType.MULTIPLE_CARD_TUTORIAL);
		}
		if (_MainGameModtroller.WildcardRule && _allowedCardIndexes.Exists(i => Hand.ReadOnlyCards[i].CardValue == _MainGameModtroller.WildCardValue))
		{
			_tutorialSystem.StartTutorialIfNecessary(AdaptiveTutorialSystem.TutorialType.WILD_CARD_TUTORIAL);
		}
		if (_MainGameModtroller.OptionalPlayRule && _allowedCardIndexes.Count > 0)
		{
			_tutorialSystem.StartTutorialIfNecessary(AdaptiveTutorialSystem.TutorialType.OPTIONAL_PLAY_TUTORIAL);
		}
		if (_MainGameModtroller.MaxDeviationRule && (_MainGameModtroller.Direction == MainGameModtroller.PlayDirection.UP
													  ||  _MainGameModtroller.Direction == MainGameModtroller.PlayDirection.DOWN))
		{
			_tutorialSystem.StartTutorialIfNecessary(AdaptiveTutorialSystem.TutorialType.MAX_DEVIATION_TUTORIAL);
		}
		_tutorialSystem.StartTutorialIfNecessary(AdaptiveTutorialSystem.TutorialType.OBJECTIVE_TUTORIAL);

		Hand.CardsTextVisibility = true;
		SetCardStates();
	}

	public void SubmitCards(bool handCardsWereDragged = false)
	{
		Hand.ReadOnlyCards.ForEach(c => 
		{
			c.Button.CancelDrag();
			c.Button.ClearAllDelegates();
		});

		_isSelectingCards = false;

		_easterEggNumbersEntered.Clear();
		Hand.ReadOnlyCards.ForEach(c => c.RefreshFaceText());

		_submitCardsButton.SetActive(false);
		_MainGameModtroller.EndPlayerTurn(_selectedCardIndexes.ToArray(), handCardsWereDragged);
		_selectedCardIndexes.Clear();
	}

	public void CancelCardSelection(Action onFinished)
	{
		Hand.ReadOnlyCards.ForEach(c => 
		{
			c.Button.CancelDrag();
			c.Button.ClearAllDelegates();
		});

		_isSelectingCards = false;

		_easterEggNumbersEntered.Clear();
		Hand.ReadOnlyCards.ForEach(c => c.RefreshFaceText());

		_selectedCardIndexes.Clear();
		_submitCardsButton.SetActive(false);
		Hand.SetCardsAnimStates(CardController.CardViewFSM.AnimState.OBSCURED, onFinished: () =>
		{
			Hand.CardsTextVisibility = false;
			onFinished();
		});
	}

	private void OnApplicationPause(bool isPaused)
	{
		if (isPaused && _isSelectingCards)
		{
			Hand.ReadOnlyCards.ForEach(c => c.Button.CancelDrag());
			_selectedCardIndexes.Clear();
			SetCardStates();
		}
	}

	private void SetupInteractionDelegatesForCardAtIndex(int index)
	{
		ReadOnlyCollection<CardController> cards = Hand.ReadOnlyCards;

		cards[index].Button.AddToOnClicked(() =>
		{
			if (_selectedCardIndexes.Contains(index))
			{
				_selectedCardIndexes.Remove(index);
			}
			else
			{
				_selectedCardIndexes.ForEach(i =>
				{
					if (cards[i].CardValue != cards[index].CardValue)
					{
						cards[i].Button.CancelDrag();
					}
				});
				_selectedCardIndexes.Add(index);
				_selectedCardIndexes.RemoveAll(i => Hand.ReadOnlyCards[i].CardValue != cards[index].CardValue);
			}
			AddNumberToEasterEggSequence(index);
			SetCardStates();
		});

		void selectCurrentAndSubmit()
		{
			if (!_selectedCardIndexes.Contains(index))
			{
				_selectedCardIndexes.Add(index);
				_selectedCardIndexes.RemoveAll(i => cards[i].CardValue != cards[index].CardValue);
			}
			SubmitCards();
		}

		cards[index].Button.AddToOnDoubleClicked(selectCurrentAndSubmit);

		cards[index].Button.AddToOnClickedHard(selectCurrentAndSubmit);

		cards[index].Button.AddToOnBeginDrag(() =>
		{
			if (!_selectedCardIndexes.Contains(index))
			{
				_selectedCardIndexes.ForEach(i =>
				{
					if (cards[i].CardValue != cards[index].CardValue)
					{
						cards[i].Button.CancelDrag();
					}
				});
				_selectedCardIndexes.Add(index);
				_selectedCardIndexes.RemoveAll(i => cards[i].CardValue != cards[index].CardValue);
			}
			SetCardStates();
		});

		cards[index].Button.AddToOnDrag(p =>
		{
			Ray ray = _MainGameModtroller.MainCamera.ScreenPointToRay(p.position);
			new Plane(cards[index].ParentCardHolder.transform.up, cards[index].ParentCardHolder.GetFinalPositionOfCard(cards[index])).Raycast(ray, out float distance);
			IncrementalPositionTween posTween = null;
			Vector3 targetPosition = ray.GetPoint(distance);
			if ((posTween = cards[index].Holder.GetTweenOfType<IncrementalPositionTween>()) != null)
			{
				posTween.PositionTo = targetPosition;
			}
			else
			{
				cards[index].transform.position += CARD_DRAG_ACCELERATION * Time.deltaTime * (targetPosition - cards[index].transform.position);
			}
		});

		cards[index].Button.AddToOnDrop(p =>
		{
			if (p.delta.y > 0.0f || (p.delta.y == 0.0f && p.position.y > Screen.height / 2.0f))
			{
				SubmitCards(handCardsWereDragged: true);
			}
			else
			{
				if (!_selectedCardIndexes.Exists(i => cards[i].CardValue == cards[index].CardValue && i != index && !cards[i].Button.IsBeingDragged))
				{
					_selectedCardIndexes.Remove(index);
				}
				SetCardStates();
				cards[index].AddIncrementalPositionTween(Hand.GetFinalPositionOfCardAtIndex(index))
							.AddIncrementalScaleTween(cards[index].ViewFSM.GetAnimScale())
							.SetDuration(cards[index].CardAnimationData.CardStateChangeDuration);
			}
		});
	}

	private void SetCardStates(Action onFinished = null)
	{
		FinishableGroupWaiter cardStateTransitionWaiter = null;
		if (onFinished != null)
		{
			cardStateTransitionWaiter = new FinishableGroupWaiter(onFinished);
		}
		for (int i = 0, iMax = Hand.ReadOnlyCards.Count; i < iMax; ++i)
		{
			TweenHolder transitionTweenHolder;
			if (_selectedCardIndexes.Contains(i))
			{
				transitionTweenHolder = Hand.ReadOnlyCards[i].ViewFSM.SetAnimState(CardController.CardViewFSM.AnimState.SELECTED);
			}
			else if (_allowedCardIndexes.Contains(i) && _selectedCardIndexes.IsEmpty())
			{
				transitionTweenHolder = Hand.ReadOnlyCards[i].ViewFSM.SetAnimState(CardController.CardViewFSM.AnimState.ABLE_TO_BE_SELECTED);
			}
			else
			{
				transitionTweenHolder = Hand.ReadOnlyCards[i].ViewFSM.SetAnimState(CardController.CardViewFSM.AnimState.VISIBLE);
			}
			cardStateTransitionWaiter.IfIsNotNullThen(c => c.AddFinishable(transitionTweenHolder));
		}
		_submitCardsButton.SetActive(_allowedCardIndexes.Count <= 0 || (!Hand.ReadOnlyCards.Any(c => c.Button.IsBeingDragged)
									 && (_MainGameModtroller.OptionalPlayRule || !_selectedCardIndexes.IsEmpty())));
		cardStateTransitionWaiter.IfIsNotNullThen(c => c.Ready = true);
	}

	private void AddNumberToEasterEggSequence(int index)
	{
		_easterEggNumbersEntered.Enqueue(index);
		if (_easterEggNumbersEntered.Count > EASTER_EGG_CODE.Length)
		{
			_easterEggNumbersEntered.Dequeue();
		}
		if (_easterEggNumbersEntered.ToArray().StartsWith(EASTER_EGG_CODE) && Hand.ReadOnlyCards.Count == 6 && _allowedCardIndexes.Count == 6)
		{
			Hand.ReadOnlyCards.ForEach(c =>
			{
				c.CardSuitText.color = c.CardValueText.color = CardController.RedTextColor;
				c.CardSuitText.text = "\n\u2665";
			});
			Hand.ReadOnlyCards[0].CardValueText.text = "M\n";
			Hand.ReadOnlyCards[1].CardValueText.text = "A\n";
			Hand.ReadOnlyCards[2].CardValueText.text = "G\n";
			Hand.ReadOnlyCards[3].CardValueText.text = "G\n";
			Hand.ReadOnlyCards[4].CardValueText.text = "I\n";
			Hand.ReadOnlyCards[5].CardValueText.text = "E\n";
			_selectedCardIndexes.Clear();
			SetCardStates();
		}
	}
}

#pragma warning restore IDE0044 // Add readonly modifier