﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Nx;

public partial class MainGameModtroller : MonoBehaviour
{
	private class Command
	{
		public virtual void Do() {}
		public virtual void Undo() {}
		public virtual void Redo() { Do(); }
		public virtual void SetupUndoData() {}
		public virtual bool IsTweened { get { return false; } }
	}

	private class MGMCommand : Command
	{
		protected MainGameModtroller _mgmUnderControl;

		protected MGMCommand() {}

		protected MGMCommand(MainGameModtroller mgmUnderControl)
		{
			_mgmUnderControl = mgmUnderControl;
		}
	}

	private class SetDirectionCommand : MGMCommand
	{
		private PlayDirection _oldDirection;
		private readonly PlayDirection _newDirection;

		private SetDirectionCommand() {}

		public SetDirectionCommand(MainGameModtroller mgmUnderControl, PlayDirection newDirection) : base(mgmUnderControl)
		{
			_newDirection = newDirection;
		}

		public override void SetupUndoData()
		{
			_oldDirection = _mgmUnderControl._direction;
		}

		public override void Do()
		{
			_mgmUnderControl.Direction = _newDirection;
		}

		public override void Undo()
		{
			_mgmUnderControl.Direction = _oldDirection;
		}
	}

	private class SetCardWhenDirectionWasLastUpdatedCommand : MGMCommand
	{
		private CardController _oldCard;
		private readonly CardController _newCard;

		private SetCardWhenDirectionWasLastUpdatedCommand() {}

		public SetCardWhenDirectionWasLastUpdatedCommand(MainGameModtroller mgmUnderControl, CardController newCard) : base(mgmUnderControl)
		{
			_newCard = newCard;
		}

		public override void SetupUndoData()
		{
			_oldCard = _mgmUnderControl._cardWhenDirectionWasLastUpdated;
		}

		public override void Do()
		{
			_mgmUnderControl._cardWhenDirectionWasLastUpdated = _newCard;
		}

		public override void Undo()
		{
			_mgmUnderControl._cardWhenDirectionWasLastUpdated = _oldCard;
		}
	}

	private class SetCurrentPlayerCommand : MGMCommand
	{
		private int _oldPlayer;
		private readonly int _newPlayer;

		private SetCurrentPlayerCommand() {}

		public SetCurrentPlayerCommand(MainGameModtroller mgmUnderControl, int newPlayer) : base(mgmUnderControl)
		{
			_newPlayer = newPlayer;
		}

		public override void SetupUndoData()
		{
			_oldPlayer = _mgmUnderControl._currentPlayer;
		}

		public override void Do()
		{
			_mgmUnderControl._currentPlayer = _newPlayer;
		}

		public override void Undo()
		{
			_mgmUnderControl._currentPlayer = _oldPlayer;
		}
	}

	private class SetIndexOfLastPlayerToPlayACardCommand : MGMCommand
	{
		private int _oldIndex;
		private readonly int _newIndex;

		private SetIndexOfLastPlayerToPlayACardCommand() {}

		public SetIndexOfLastPlayerToPlayACardCommand(MainGameModtroller mgmUnderControl, int newIndex) : base(mgmUnderControl)
		{
			_newIndex = newIndex;
		}

		public override void SetupUndoData()
		{
			_oldIndex = _mgmUnderControl._indexOfLastPlayerToPlayACard;
		}

		public override void Do()
		{
			_mgmUnderControl._indexOfLastPlayerToPlayACard = _newIndex;
		}

		public override void Undo()
		{
			_mgmUnderControl._indexOfLastPlayerToPlayACard = _oldIndex;
		}
	}

	private class SetPlayerScoreCommand : MGMCommand
	{
		private readonly int _playerIndex;
		private int _oldScore;
		private readonly int _newScore;

		private SetPlayerScoreCommand() {}

		public SetPlayerScoreCommand(MainGameModtroller mgmUnderControl, int playerIndex, int newScore) : base(mgmUnderControl)
		{
			_playerIndex = playerIndex;
			_newScore = newScore;
		}

		public override void SetupUndoData()
		{
			_oldScore = _mgmUnderControl._players[_playerIndex].Points;
		}

		public override void Do()
		{
			_mgmUnderControl._players[_playerIndex].Points = _newScore;
		}

		public override void Undo()
		{
			_mgmUnderControl._players[_playerIndex].Points = _oldScore;
		}
	}

	private class SetPlayerEliminatedCommand : MGMCommand
	{
		private readonly int _playerIndex;
		private readonly bool _newEliminated;
		private bool _oldEliminated;

		private SetPlayerEliminatedCommand() {}

		public SetPlayerEliminatedCommand(MainGameModtroller mgmUnderControl, int playerIndex, bool newEliminated) : base(mgmUnderControl)
		{
			_playerIndex = playerIndex;
			_newEliminated = newEliminated;
		}

		public override void SetupUndoData()
		{
			_oldEliminated = _mgmUnderControl._players[_playerIndex].Eliminated;
		}

		public override void Do()
		{
			_mgmUnderControl._players[_playerIndex].Eliminated = _newEliminated;
		}

		public override void Undo()
		{
			_mgmUnderControl._players[_playerIndex].Eliminated = _oldEliminated;
		}
	}

	private abstract class TweenedCommand : Command, IFinishable
	{
		public abstract TweenHolder OutTween { get; }

		public abstract void AddToOnFinished(Action toAdd);

		public override bool IsTweened { get { return true; } }
	}

	private class MoveCardCommand : TweenedCommand
	{
		private readonly CardHolder _fromHolder;
		private readonly CardHolder _toHolder;
		private readonly int _fromIndex;
		private int _toIndex;
		private readonly bool? _visibleDuringTween;
		private TweenHolder _outTween;

		public override TweenHolder OutTween { get { return _outTween; } }

		private MoveCardCommand() {}

		public MoveCardCommand(CardHolder fromHolder, int fromIndex, CardHolder toHolder,
							   int toIndex = -1, bool? visibleDuringTween = null)
		{
			_fromHolder = fromHolder;
			_toHolder = toHolder;
			_fromIndex = fromIndex;
			_toIndex = toIndex;
			_visibleDuringTween = visibleDuringTween;
		}

		private MoveCardCommand(CardHolder fromHolder, CardController fromCard, CardHolder toHolder, int toIndex = -1, bool? visibleDuringTween = null)
				: this(fromHolder, fromHolder.ReadOnlyCards.IndexOf(fromCard), toHolder, toIndex, visibleDuringTween) {}

		public override void SetupUndoData()
		{
			if (!_toHolder.ReadOnlyCards.IndexIsValid(_toIndex))
			{
				_toIndex = _toHolder.CardCount;
			}
		}

		public override void Do()
		{
			_fromHolder.MoveCard(_fromIndex, _toHolder, out _outTween, _visibleDuringTween, _toIndex);
		}

		public override void Undo()
		{
			_toHolder.MoveCard(_toIndex, _fromHolder, out _outTween, _visibleDuringTween, _fromIndex);
		}

		public override void AddToOnFinished(Action toAdd)
		{
			_outTween.AddToOnFinishedOnce(toAdd);
		}
	}

	private class DealToCommand : TweenedCommand
	{
		private readonly Deck _deckToDealFrom;
		private readonly CardHolder _cardHolderToDealTo;
		private readonly bool? _visibleDuringTween;
		private TweenHolder _outTween;

		public override TweenHolder OutTween { get { return _outTween; } }

		private DealToCommand() {}

		public DealToCommand(Deck deckToDealFrom, CardHolder cardHolderToDealTo, bool? visibleDuringTween = null)
		{
			_deckToDealFrom = deckToDealFrom;
			_cardHolderToDealTo = cardHolderToDealTo;
			_visibleDuringTween = visibleDuringTween;
		}

		public DealToCommand(Deck deckToDealFrom, AbstractPlayerModtroller playerToDealTo, bool? visibleDuringTween = null)
		: this(deckToDealFrom, playerToDealTo.Hand, visibleDuringTween) {}

		public override void Do()
		{
			_deckToDealFrom.DealTo(_cardHolderToDealTo, out _outTween, _visibleDuringTween);
		}

		public override void Undo()
		{
			_cardHolderToDealTo.MoveCard(_cardHolderToDealTo.ReadOnlyCards.LastIndex(), _deckToDealFrom, out _outTween, _visibleDuringTween);
		}

		public override void AddToOnFinished(Action toAdd)
		{
			_outTween.AddToOnFinishedOnce(toAdd);
		}
	}

	private class ShuffleCommand : Command, IFinishable
	{
		private readonly Deck _DeckToShuffle;
		private int[] _unShuffleData;
		private Action _onFinished;

		private ShuffleCommand() {}

		public ShuffleCommand(Deck deckToShuffle, Action onFinished)
		{
			_DeckToShuffle = deckToShuffle;
			_onFinished = onFinished;
		}

		public override void Do()
		{
			_DeckToShuffle.Shuffle(out _unShuffleData, _onFinished);
			_onFinished = null;
		}

		public override void Undo()
		{
			_DeckToShuffle.UnShuffle(_unShuffleData, _onFinished);
			_onFinished = null;
		}

		public override void Redo()
		{
			_DeckToShuffle.ReShuffle(_unShuffleData, _onFinished);
			_onFinished = null;
		}

		public void AddToOnFinished(Action toAdd)
		{
			_onFinished += toAdd;
		}
	}

	private class RefillDeckCommand : Command, IFinishable
	{
		private readonly Deck _deckToRefill;
		private readonly CardHolder _cardHolderToRefillFrom;
		private readonly int _numberOfCardsToLeave;
		private int _deckRefillerCardCount;
		private Action _onFinished;

		private RefillDeckCommand() {}

		public RefillDeckCommand(Deck deckToRefill, CardHolder cardHolderToRefillFrom, int numberOfCardsToLeave, Action onFinished)
		{
			_deckToRefill = deckToRefill;
			_cardHolderToRefillFrom = cardHolderToRefillFrom;
			_numberOfCardsToLeave = numberOfCardsToLeave;
			_onFinished = onFinished;
		}

		public override void SetupUndoData()
		{
			_deckRefillerCardCount = _cardHolderToRefillFrom.CardCount;
		}

		public override void Do()
		{
			_deckToRefill.RefillWith(_cardHolderToRefillFrom, _numberOfCardsToLeave, _onFinished);
			_onFinished = null;
		}

		public override void Undo()
		{
			_deckToRefill.Refill(_cardHolderToRefillFrom, _deckRefillerCardCount - _numberOfCardsToLeave, _onFinished);
			_onFinished = null;
		}

		public void AddToOnFinished(Action toAdd)
		{
			_onFinished += toAdd;
		}
	}

	private class TurnCommandBundle : LinkedList<Command>
	{
		private TurnCommandBundle() {}

		public TurnCommandBundle(Command firstCommand)
		{
			AddLast(firstCommand);
		}

		public bool Finished { get; private set; }

		public void Finish()
		{
			Finished = true;
		}
	}

	private class Commander
	{
		private Stack<TurnCommandBundle> _undoStack;
		private Stack<TurnCommandBundle> _redoStack;
		private bool _startCommandTracking;
		private readonly CardAnimationData _cardAnimationData;

		public bool UndoIsPossible { get { return !_undoStack.IsEmpty(); } }
		public bool RedoIsPossible { get { return !_redoStack.IsEmpty(); } }

		public Commander(CardAnimationData cardAnimationData)
		{
			Reset();
			_cardAnimationData = cardAnimationData;
		}

		public void Reset()
		{
			_undoStack = new Stack<TurnCommandBundle>();
			_redoStack = new Stack<TurnCommandBundle>();
			_startCommandTracking = false;
		}

		public void ExecuteAndAddToCurrentTurnBundle(Command command)
		{
			command.SetupUndoData();
			command.Do();
			if (_startCommandTracking)
			{
				if (_undoStack.Count == 0 || _undoStack.Peek().Finished)
				{
					var newCommandBundle = new TurnCommandBundle(command);
					_undoStack.Push(newCommandBundle);
				}
				else
				{
					_undoStack.Peek().AddLast(command);
				}
				_redoStack.Clear();
			}
		}

		public void FinishTurnBundle()
		{
			if (_undoStack.Count == 0 || _undoStack.Peek().Finished)
			{
				_startCommandTracking = true;
				return;
			}
			_undoStack.Peek().Finish();
		}

		public IEnumerator UndoPlayerTurn(Action onFinished)
		{
			TurnCommandBundle bundle = _undoStack.Pop();
			var animationWaiter = new FinishableGroupWaiter(onFinished);
			for (LinkedListNode<Command> node = bundle.Last; node != null; node = node.Previous)
			{
				if (node.Value is IFinishable)
				{
					if (node.Value.IsTweened)
					{
						node.Value.Undo();
					}
					else
					{
						yield return new WaitUntil(() => animationWaiter.NumFinishableToWaitFor <= 0);
					}
					animationWaiter.AddFinishable((IFinishable) node.Value);
					if (node.Value.IsTweened)
					{
						yield return new WaitForSeconds(_cardAnimationData.UndoTurnDelay);
					}
					else
					{
						bool finished = false;
						((IFinishable) node.Value).AddToOnFinished(() => finished = true);
						node.Value.Undo();
						yield return new WaitUntil(() => finished == true);
					}
				}
				else
				{
					node.Value.Undo();
				}
			}
			_redoStack.Push(bundle);
			animationWaiter.Ready = true;
		}

		public IEnumerator RedoPlayerTurn(Action onFinished)
		{
			TurnCommandBundle bundle = _redoStack.Pop();
			var animationWaiter = new FinishableGroupWaiter(onFinished);
			for (LinkedListNode<Command> node = bundle.First; node != null; node = node.Next)
			{
				if (node.Value is IFinishable)
				{
					bool isTweenedCommand = node.Value is TweenedCommand;
					if (isTweenedCommand)
					{
						node.Value.Redo();
					}
					else
					{
						yield return new WaitUntil(() => animationWaiter.NumFinishableToWaitFor <= 0);
					}
					animationWaiter.AddFinishable((IFinishable) node.Value);
					if (isTweenedCommand)
					{
						yield return new WaitForSeconds(_cardAnimationData.UndoTurnDelay);
					}
					else
					{
						bool finished = false;
						((IFinishable) node.Value).AddToOnFinished(() => finished = true);
						node.Value.Redo();
						yield return new WaitUntil(() => finished == true);
					}
				}
				else
				{
					node.Value.Redo();
				}
			}
			_undoStack.Push(bundle);
			animationWaiter.Ready = true;
		}
	}
}
