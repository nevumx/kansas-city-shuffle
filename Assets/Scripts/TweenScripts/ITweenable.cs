using UnityEngine;

#pragma warning disable IDE1006 // Naming Styles

public interface ITweenable
{
	TweenHolder	Holder		{ get; }
	GameObject	gameObject	{ get; }
}

#pragma warning restore IDE1006 // Naming Styles