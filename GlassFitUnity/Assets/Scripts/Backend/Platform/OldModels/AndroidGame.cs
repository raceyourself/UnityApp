﻿using UnityEngine;
using System.Collections.Generic;
using System;

using RaceYourself.Models;
#if UNITY_ANDROID
/// <summary>
/// Game model class. Contains all metadata about each game - title, description, cost, whether it is unlocked or not and so on.
/// </summary>
public class AndroidGame : Game
{
	
	private AndroidJavaObject javaGame;  // reference to JNI object so we can call methods on it
	
	/// <summary>
	/// Initialise this model with data from an equivalent Java game model
	/// </summary>
	/// <param name='javaGame'>
	/// The AndroidJavaObject to extract the game details from
	/// </param>
	public void Initialise (AndroidJavaObject javaGame)
	{
		this.javaGame = javaGame;
		try {
			// Extract fields from the java object using JNI calls
			gameId = javaGame.Call<string> ("getGameId");
			name = javaGame.Call<string> ("getName");
			iconName = javaGame.Call<string>("getIconName");
			activity = javaGame.Call<string> ("getActivity");
			description = javaGame.Call<string> ("getDescription");
			state = javaGame.Call<string> ("getState");
			tier = javaGame.Call<int> ("getTier");
			priceInPoints = javaGame.Call<long> ("getPriceInPoints");
			priceInGems = javaGame.Call<long> ("getPriceInGems");
			type = javaGame.Call<string> ("getType");
			column = javaGame.Call<int> ("getColumn");
			row = javaGame.Call<int> ("getRow");
			sceneName = javaGame.Call<string> ("getSceneName");
			UnityEngine.Debug.Log ("Game: Successfuly imported game: " + gameId);
		}
		catch (Exception e) {
			// JNI exception, or any exception in the java code
			UnityEngine.Debug.LogWarning ("Game: Error importing game");
			UnityEngine.Debug.LogException (e);
		}
	}
	
	/// <summary>
	/// Unlock this game, subject to sufficient funds being available in the user's account.
	/// The transaction happens in Java, which will throw an InsufficientFundsException if 
	/// the user doesn't have enough points/gems to buy the game. Still need to work out how 
	/// to turn this into a nice C# exception.
	/// </summary>
	public override void Unlock ()
	{
#if !UNITY_EDITOR
		try {
			// JNI method to perforn the unlock transaction
			AndroidJavaObject updatedJavaGame = javaGame.Call<AndroidJavaObject> ("unlock");
			// Update the fields of this game to show the new unlocked status
			this.Initialise (updatedJavaGame);
			base.Unlock();
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning ("Game: Error unlocking game: " + gameId);
			UnityEngine.Debug.LogException (e);
		}
#else
#endif
	}
	
}
#endif