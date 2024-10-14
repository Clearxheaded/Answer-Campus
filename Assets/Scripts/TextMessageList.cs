using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Text.RegularExpressions;
using System.Linq;  // For LINQ methods

[System.Serializable]
public struct ProfilePicture
{
    public Character character;
    public Sprite picture;
}

public class TextMessageList : MonoBehaviour
{
    public GameObject listItemTemplate;
    public GameObject messageTemplate;
    public GameObject inbox;
    public TextMeshProUGUI inboxHeader;
    public Image inboxProfile;
    public Button likeMessageButton;
    public Phone phone;

    public ProfilePicture[] profiles;
    private List<TextMessage> messages;
    private Dictionary<Character, List<TextMessage>> groupedMessages;

    // Start is called before the first frame update
    void Start()
    {
        messages = PlayerPrefsExtra.GetList<TextMessage>("messages", new List<TextMessage>());

        // Group messages by character
        groupedMessages = messages
            .GroupBy(m => m.from)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var entry in groupedMessages)
        {
            Character from = entry.Key;
            GameObject go = Instantiate(listItemTemplate, transform);

            // Set the "from" text to display the sender's name
            go.GetComponent<ViewTextMessage>().from.text = from.ToString();

            // Assign profile picture if available
            for (int j = 0; j < profiles.Length; j++)
            {
                if (profiles[j].character == from)
                {
                    go.GetComponent<ViewTextMessage>().profile.sprite = profiles[j].picture;
                    break;
                }
            }

            // Button to load the message thread
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(() => LoadThread(from, groupedMessages[from]));
        }
    }

    private void ClearMessages()
    {
        ViewTextMessage[] list = inbox.GetComponentsInChildren<ViewTextMessage>();
        Debug.Log("FOUND " + list.Length + " messages");
        for (int i = 0; i < list.Length; i++)
        {
            Destroy(list[i].gameObject);
        }
    }

    private void LoadThread(Character from, List<TextMessage> messages)
    {
        Debug.Log("LOADING THREAD FOR " + from.ToString());
        phone.ClearNotifications();
        ClearMessages();


        inboxHeader.text = from.ToString();
        inbox.transform.parent.parent.parent.gameObject.SetActive(true);

        for (int i = 0; i < messages.Count; i++)
        {
            // Set profile picture in the inbox
            for (int j = 0; j < profiles.Length; j++)
            {
                if (profiles[j].character == from)
                {
                    inboxProfile.sprite = profiles[j].picture;
                    break;
                }
            }

            // Create multiple message items (split by sentences)
            string[] sentences = Regex.Split(messages[i].message, @"(?<=[\.!\?])\s+");
            Debug.Log("FOUND " + sentences.Length + " SENTENCES.");
            for (int j = 0; j < sentences.Length; j++)
            {
                GameObject go = Instantiate(messageTemplate, inbox.transform);
                go.GetComponent<ViewTextMessage>().message.text = sentences[j];
            }

            // Correctly assign button action
            string loc = messages[i].location;
            likeMessageButton.onClick.RemoveAllListeners(); // Remove previous listeners
            likeMessageButton.onClick.AddListener(() => GoToLocation(loc));
        }

        // Hide the previous view
        transform.parent.parent.parent.gameObject.SetActive(false);
    }

    private void GoToLocation(string location)
    {
        /* REMOVE MESSAGES FROM CHARACTER */

            // Find the character whose messages are being viewed
            Character from = (Character)Enum.Parse(typeof(Character), inboxHeader.text);

            // Remove the messages from the groupedMessages dictionary
            if (groupedMessages.ContainsKey(from))
            {
                groupedMessages.Remove(from);
                Debug.Log($"Removed all messages from {from}");

                // Save the updated messages to PlayerPrefs
                messages = groupedMessages.Values.SelectMany(m => m).ToList();
                PlayerPrefsExtra.SetList("messages", messages);
                PlayerPrefs.Save();
            }


        Debug.Log("Going to " + location);
        SceneManager.LoadScene(location);
    }
}
