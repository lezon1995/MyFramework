using System;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A helper class to copy and paste feedback properties
	/// </summary>
	static class MMF_PlayerCopy
	{
		public static System.Type Type { get; private set; }
		public static readonly List<MMF_Feedback> CopiedFeedbacks = new List<MMF_Feedback>();
		public static readonly Dictionary<MMF_Player, List<MMF_Feedback>> RuntimeChanges = new Dictionary<MMF_Player, List<MMF_Feedback>>();

		static string[] IgnoreList = new string[]
		{
			"m_ObjectHideFlags",
			"m_CorrespondingSourceObject",
			"m_PrefabInstance",
			"m_PrefabAsset",
			"m_GameObject",
			"m_Enabled",
			"m_EditorHideFlags",
			"m_Script",
			"m_Name",
			"m_EditorClassIdentifier"
		};
		
		static MMF_PlayerCopy()
		{
			EditorApplication.playModeStateChanged += ModeChanged;
		}

		private static void ModeChanged(PlayModeStateChange playModeState)
		{
			switch (playModeState)
			{
				case PlayModeStateChange.ExitingPlayMode:
					StoreRuntimeChanges();
					break;
        
				case PlayModeStateChange.EnteredEditMode:
					ApplyRuntimeChanges();
					break;
			}
		}

		private static void StoreRuntimeChanges()
		{
			foreach (MMF_Player player in MonoBehaviour.FindObjectsOfType<MMF_Player>(true).Where(p => p.KeepPlayModeChanges))
			{
				MMF_PlayerCopy.StoreRuntimeChanges(player);
			}
		}

		private static void ApplyRuntimeChanges()
		{
			foreach (MMF_Player player in MonoBehaviour.FindObjectsOfType<MMF_Player>(true).Where(MMF_PlayerCopy.RuntimeChanges.ContainsKey))
			{
				MMF_PlayerCopy.ApplyRuntimeChanges(player);
			}
		}

		public static bool HasCopy()
		{
			return CopiedFeedbacks != null && CopiedFeedbacks.Count == 1;
		}

		public static bool HasMultipleCopies()
		{
			return CopiedFeedbacks != null && CopiedFeedbacks.Count > 1;
		}

		public static void Copy(MMF_Feedback feedback)
		{
			Type feedbackType = feedback.GetType();
			MMF_Feedback newFeedback = (MMF_Feedback)Activator.CreateInstance(feedbackType);
			EditorUtility.CopySerializedManagedFieldsOnly(feedback, newFeedback);
			CopiedFeedbacks.Clear();
			CopiedFeedbacks.Add(newFeedback);
		}
        
		public static void CopyAll(MMF_Player sourceFeedbacks)
		{
			CopiedFeedbacks.Clear();
			foreach (MMF_Feedback feedback in sourceFeedbacks.FeedbacksList)
			{
				Type feedbackType = feedback.GetType();
				MMF_Feedback newFeedback = (MMF_Feedback)Activator.CreateInstance(feedbackType);
				EditorUtility.CopySerializedManagedFieldsOnly(feedback, newFeedback);
				CopiedFeedbacks.Add(newFeedback);    
			}
		}

		// Multiple Copy ----------------------------------------------------------

		public static void PasteAll(MMF_PlayerEditor targetEditor)
		{
			foreach (MMF_Feedback feedback in MMF_PlayerCopy.CopiedFeedbacks)
			{
				targetEditor.TargetMmfPlayer.AddFeedback(feedback);
			}
			CopiedFeedbacks.Clear();
		}
		
		// Runtime Changes

		public static void StoreRuntimeChanges(MMF_Player player)
		{
			RuntimeChanges[player] = new List<MMF_Feedback>();
			foreach (MMF_Feedback feedback in player.FeedbacksList)
			{
				Type feedbackType = feedback.GetType();
				MMF_Feedback newFeedback = (MMF_Feedback)Activator.CreateInstance(feedbackType);
				EditorUtility.CopySerializedManagedFieldsOnly(feedback, newFeedback);
				RuntimeChanges[player].Add(newFeedback);    
			}
		}

		public static void ApplyRuntimeChanges(MMF_Player player)
		{
			SerializedObject playerSerialized = new SerializedObject(player);
			playerSerialized.Update();
			Undo.RecordObject(player, "Replace all feedbacks");
			player.FeedbacksList.Clear();
			foreach (MMF_Feedback feedback in MMF_PlayerCopy.RuntimeChanges[player])
			{
				player.AddFeedback(feedback);
			}
			playerSerialized.ApplyModifiedProperties();
			PrefabUtility.RecordPrefabInstancePropertyModifications(player);
			if (MMF_PlayerConfiguration.Instance.AutoDisableKeepPlaymodeChanges)
			{
				playerSerialized.Update();
				player.KeepPlayModeChanges = false;    
				playerSerialized.ApplyModifiedProperties();
			}
			player.RefreshCache();
		}
	}
}