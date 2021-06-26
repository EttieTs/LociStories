#define INSTALLED_MACHINE_NAMES
//#define VIOLETAS_MACHINE_NAMES
//#define GAVINS_MACHINE_NAMES

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// https://www.sciencedirect.com/topics/computer-science/multicasting
public class BWNetworking : MonoBehaviour
{
    // 224.0.0.0 to 239.255.255.255.
    static IPEndPoint endPointIP;
    static private int port = 30000;
    static private UdpClient multicastClient;
    static IPAddress multicastAddress = IPAddress.Parse("239.255.255.255");

    // CAPITALS HERE
#if INSTALLED_MACHINE_NAMES
    public static string machineNameAB = "ONE";
    public static string machineNameCD = "TWO";
    public static string machineNameEF = "THREE";
#endif

#if VIOLETAS_MACHINE_NAMES
    // VIOLETAS SETUP
    public static string machineNameAB = "DESKTOP-F5AK0BV"; //Violeta actual PC ->"DESKTOP-P6BB6S7"
    public static string machineNameCD = "DESKTOP-OA33INC"; //DESKTOP-EN2BEG6"
    public static string machineNameEF = "DESKTOP-P6BB6S7"; //
#endif

#if GAVINS_MACHINE_NAMES
    // GAVINS SETUP
    public static string machineNameAB = "GAVIN-I9"; 
    public static string machineNameCD = "I7-DESKTOP";
    public static string machineNameEF = "DESKTOP-P6BB6S7";
#endif

    static bool isShutdown = false;

    //========================================================================================================================================================================

    public enum VideoAction
    {
        PlayingVideo,
        StoppedVideo,
    }
    public enum SoundAction
    {
        PlayingSound,
        StoppedSound,
    }

    [Serializable]
    public class Message
    {
        public int messageTime;
        public string address;
        public string sender;
        public bool isShutdown;
        public bool isPlayingVideo;
        public bool isPlayingSound;
    };

    static Message currentOutMessage = new Message();
    static Dictionary<string, Message> currentReceivedMessages = new Dictionary<string, Message>();

    Task receiveTask = null;
    Task sendTask = null;
    static Task shutdownTask = null;

    void Start()
    {
        Debug.Log("Starting Client");

        endPointIP = new IPEndPoint(IPAddress.Any, port);
        multicastClient = new UdpClient(endPointIP);
        multicastClient.EnableBroadcast = true;

        try
        {
            // The packet dies after 50 router hops.
            multicastClient.JoinMulticastGroup(multicastAddress, 50);

            Debug.Log("Joined multicast Address: " + multicastAddress.ToString());
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }

        receiveTask = ReceiveDatagramAsync();
        sendTask = SendDatagramAsync();
    }

    public static String GetDeviceName()
    {
        return SystemInfo.deviceName;
    }


    // Proper asynchronous reading - I got this wrong in the last example
    private static async Task ReceiveDatagramAsync()
    {
        for (; ; )
        {
            UdpReceiveResult udpReceiveResult = await multicastClient.ReceiveAsync();

            if (udpReceiveResult.Buffer.Length > 0)
            {
                Message objectThatWasDeserialized = new Message();
                MemoryStream stream = new MemoryStream();
                stream.Write(udpReceiveResult.Buffer, 0, udpReceiveResult.Buffer.Length);
                stream.Seek(0, SeekOrigin.Begin);
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    objectThatWasDeserialized = (Message)formatter.Deserialize(stream);
                }
                catch (SerializationException e)
                {
                    Debug.Log("Deserialization Failed : " + e.Message);
                }
                stream.Close();
                stream.Dispose();
                stream = null;

                // First time it comes - through - Its an empty dictionary - because no messages have been received
                // Fill the dictionary hole with a blank message
                if (currentReceivedMessages.ContainsKey(objectThatWasDeserialized.address) == false )
                {
                    Message blankMessage = new Message();
                    blankMessage.messageTime = -1;
                    currentReceivedMessages[objectThatWasDeserialized.address] = blankMessage;                    
                }

                // Have we got a new message
                Message currentMessage = currentReceivedMessages[objectThatWasDeserialized.address];

                // Have we seen the message before?
                // if we haven't - we've got a new message
                if (objectThatWasDeserialized.messageTime > currentMessage.messageTime)
                {
                    // replace message with newer one
                    currentReceivedMessages[objectThatWasDeserialized.address] = objectThatWasDeserialized;

                    // New message recevied
                    OnReceiveMessage(objectThatWasDeserialized);
                }
            }
        }
    }

    private static async Task SendDatagramAsync()
    {
        for (; ; )
        {
            await Task.Yield();

            // Have we started sending messages yet?
            if (messageNumber > 0)
            {
                // Add extra stuff to the message
                currentOutMessage.address = GetDeviceName();

                // Now serialise it
                MemoryStream stream = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(stream, currentOutMessage);
                }
                catch (SerializationException e)
                {
                    Debug.Log("Serialization Failed : " + e.Message);
                }
                byte[] objectAsBytes = stream.ToArray();
                stream.Close();
                stream.Dispose();
                stream = null;

                endPointIP = new IPEndPoint(multicastAddress, port);
                await multicastClient.SendAsync(objectAsBytes, objectAsBytes.Length, endPointIP);

                // Send all messages every 3 seconds
                await Task.Delay(3000);
            }
        }
    }

    static int messageNumber = 0;

    // Simple network state
    static bool lastPlayingVideo = false;
    static bool lastPlayingSound = false;
    static bool firstTime = true;
    static bool lastShutdown = false;

    public static void SendDatagram(bool isPlayingVideo, bool isPlayingSound )
    {
        // Only updates the message time if something has changed
        if ( (lastPlayingVideo != isPlayingVideo) || (lastPlayingSound != isPlayingSound) || (lastShutdown != isShutdown) || firstTime )
        {
            // Only send it if the status has changed 
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            int cur_time = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;

            messageNumber = cur_time;

            // Set the currently sending message
            currentOutMessage.messageTime = cur_time;
            currentOutMessage.isPlayingVideo = isPlayingVideo;
            currentOutMessage.isPlayingSound = isPlayingSound;
            currentOutMessage.isShutdown = isShutdown;

            lastPlayingVideo = isPlayingVideo;
            lastPlayingSound = isPlayingSound;
            lastShutdown = isShutdown;
            firstTime = false;
        }
    }

    static bool HasDoneSomething(string machineName)
    {
        if (currentReceivedMessages.ContainsKey(machineName) )
        {
            return true;
        }
        return false;
    }
    static bool IsPlayingSound(string machineName)
    {
        if (HasDoneSomething(machineName) && currentReceivedMessages[machineName].isPlayingSound)
        {
            return true;
        }
        return false;
    }
    static bool IsPlayingVideo(string machineName)
    {
        if (HasDoneSomething(machineName) && currentReceivedMessages[machineName].isPlayingVideo )
        {
            return true;
        }
        return false;
    }
    static bool IsShutdown(string machineName)
    {
        if (HasDoneSomething(machineName) && currentReceivedMessages[machineName].isShutdown)
        {
            return true;
        }
        return false;
    }

    // ================================================================================================================================================================

    static List<string> list = new List<string>();

    // This is alled when something happend on a specific machine
    static void OnReceiveMessage(Message newMessage)
    {
        // Youcan do something here
                
        SomethingChanged();
        RotatingSound();
    }

    static void RotatingSound()
    {
        if((IsPlayingVideo(machineNameAB)) && 
            ((IsPlayingSound(machineNameCD) == false) && (IsPlayingVideo(machineNameCD) == false)))
        {
            EtVoiceRecognition.instance.PlaySoundCD();
            EtVoiceRecognition.instance.StopSoundEF();
        }
        if((IsPlayingVideo(machineNameCD)) && 
            ((IsPlayingSound(machineNameEF) == false) && (IsPlayingVideo(machineNameEF) == false)))
        {
            EtVoiceRecognition.instance.PlaySoundEF();
            EtVoiceRecognition.instance.StopSoundAB();
        }
        if((IsPlayingVideo(machineNameEF)) && 
            ((IsPlayingSound(machineNameAB) == false) && (IsPlayingVideo(machineNameAB) == false)))
        {
            EtVoiceRecognition.instance.PlaySoundAB();
            EtVoiceRecognition.instance.StopSoundCD();
        }        
    }

    private static async Task ShutdownAsync(bool force = false)
    {
        // Wait five seconds so this message has been sent out on the network (we send messages every 3 seconds ATM)
        await Task.Delay(5000);

        Application.Quit();
    }

    static void CheckShutdown(Message message)
    {
        if (message.isShutdown == true)
        {
            Debug.Log("Shutdown command received from another machine");
            Application.Quit();
        }
    }

    public static void SetShutdown(bool force = false)
    {
        if (isShutdown == true)
        {
            Debug.Log("SetShutdown: We are already shutting down");

            // We are already shutting down
            return;
        }

        Debug.Log("SetShutdown has been called on this machine");

        // Shutdown this machine
        int hour = System.DateTime.Now.Hour;

        // You'll need to test this - before and after 17 hundred hours
        if (hour >= 17)
        {
            Debug.Log("After 17 hundred hours so shutting down");
            EtLog.Log("After 17 hundred hours so shutting down");

            // Sets the global flag which will be sent out with the rest of the network information
            isShutdown = true;

            // Start the shutdown task
            shutdownTask = ShutdownAsync(force);
        }
        else if (force)
        {
            Debug.Log("ForcedShutdown");
            EtLog.Log("ForcedShutdown");

            // Sets the global flag which will be sent out with the rest of the network information
            isShutdown = true;

            // Start the shutdown task
            shutdownTask = ShutdownAsync(force);
        }
        else
        {
            Debug.Log("Not after 17 hundred hours so will not run shutdown");
            EtLog.Log("Not after 17 hundred hours so will not run shutdown");
        }
    }

    // This is called every time something updates on a machine
    static void SomethingChanged()
    {
        // e.g. Have we seen some action from machine A and machine B
        if( HasDoneSomething(machineNameAB) )
        {
            Debug.Log("Is video playing on machineAB " + currentReceivedMessages[machineNameAB].isPlayingVideo);
            Debug.Log("Is sound playing on machineAB " + currentReceivedMessages[machineNameAB].isPlayingSound);

            CheckShutdown(currentReceivedMessages[machineNameAB]);
        }
        if ( HasDoneSomething(machineNameCD) )
        {
            Debug.Log("Is video playing on machineCD " + currentReceivedMessages[machineNameCD].isPlayingVideo);
            Debug.Log("Is sound playing on machineCD " + currentReceivedMessages[machineNameCD].isPlayingSound);

            CheckShutdown(currentReceivedMessages[machineNameCD]);
        }
        if ( HasDoneSomething(machineNameEF) )
        {
            Debug.Log("Is video playing on machineEF " + currentReceivedMessages[machineNameEF].isPlayingVideo);
            Debug.Log("Is sound playing on machineEF " + currentReceivedMessages[machineNameEF].isPlayingSound);

            CheckShutdown(currentReceivedMessages[machineNameEF]);
        }
    }
}
