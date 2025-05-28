using UnityEngine;
using System.Collections.Generic;

namespace VNEngine
{
    public class CharacterStageRouterNode : Node
    {
        [System.Serializable]
        public class StageConversation
        {
            public Character character; // Exact match, e.g., "Breanna"
            public int stage;
            public ConversationManager conversation;
        }

        public string currentSceneName; // Match PlayerPrefsExtra location value (e.g., "Library")
        public List<StageConversation> routes;
        public List<ConversationManager> fallbackConversations;
        public ConversationManager repeatableFallback; 
        public override void Run_Node()
{
    List<CharacterLocation> characterLocations = PlayerPrefsExtra.GetList<CharacterLocation>("characterLocations", new List<CharacterLocation>());
    CharacterLocation? selected = null;

    // Find the first character placed on the map for this scene
    foreach (CharacterLocation loc in characterLocations)
    {
        if (loc.location == currentSceneName)
        {
            selected = loc;
            break;
        }
    }

    if (selected.HasValue)
    {
        var loc = selected.Value;
        string statKey = $"{loc.character} - {currentSceneName} - Stage";
        float stage = StatsManager.Get_Numbered_Stat(statKey);

        foreach (StageConversation route in routes)
        {
            if (route.character == loc.character && route.stage == stage)
            {
                Debug.Log($"Routing {loc.character} at stage {stage} to conversation: {route.conversation.name}");
                route.conversation.Start_Conversation();
                Finish_Node();
                return;
            }
        }
    }

    // Fallbacks (unchanged)
    for (int i = 0; i < fallbackConversations.Count; i++)
    {
        var fallback = fallbackConversations[i];
        if (fallback == null) continue;

        string fallbackKey = $"Seen - {currentSceneName} - Fallback {i}";

        if (!StatsManager.Get_Boolean_Stat(fallbackKey))
        {
            Debug.Log($"Routing unseen fallback {i}: {fallback.name}");
            StatsManager.Set_Boolean_Stat(fallbackKey, true);
            fallback.Start_Conversation();
            Finish_Node();
            return;
        }
    }

    // Repeatable fallback
    if (repeatableFallback != null)
    {
        Debug.Log($"All fallbacks seen. Routing to repeatable fallback: {repeatableFallback.name}");
        repeatableFallback.Start_Conversation();
    }
    else
    {
        Debug.Log("No matching character/stage found, and no repeatable fallback.");
    }

    Finish_Node();
}



        public override void Button_Pressed() { }
        public override void Finish_Node()
        {
            StopAllCoroutines();
            base.Finish_Node();
        }
    }
}
