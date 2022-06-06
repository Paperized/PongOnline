using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class CreateRoomMenu : MonoBehaviour
{
    private CreationRoomData roomData;

    [Header("Room Name")]
    [SerializeField] private TMPro.TMP_InputField nameInputField;

    [Header("Parameters")]
    [SerializeField] private Slider ballSpeedSlider;
    [SerializeField] private TMPro.TextMeshProUGUI ballSpeedMultiplierText;
    [SerializeField] private Slider playerSpeedSlider;
    [SerializeField] private TMPro.TextMeshProUGUI playerSpeedMultiplierText;

    private void Awake()
    {
        if (!ballSpeedSlider || !ballSpeedMultiplierText || !playerSpeedMultiplierText || !playerSpeedSlider || !nameInputField)
            CDebug.LogError("UI Elements are not all plugged");

        roomData = new CreationRoomData(nameInputField.text, ballSpeedSlider.value, playerSpeedSlider.value);
    }

    public void OnBallSpeedChanged()
    {
        roomData.ballSpeedMultiplier = ballSpeedSlider.value;
        ballSpeedMultiplierText.text = $"x{roomData.ballSpeedMultiplier}";
    }

    public void OnPlayerSpeedChanged()
    {
        roomData.playerSpeedMultiplier = playerSpeedSlider.value;
        playerSpeedMultiplierText.text = $"x{roomData.playerSpeedMultiplier}";
    }

    public void OnRoomNameChanged()
    {
        roomData.title = nameInputField.text;
    }

    public void CreateRoom()
    {
        if (!roomData.IsValid())
            return;

        RoomManager instance = RoomManager.Instance;
        instance.MakeNewRoomRPC(roomData);
    }
}
