using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class NetworkEventSubscriber : MonoBehaviour {
    async void Start() {
        var server = "localhost";
        var message = "Hey server\n";
        try {
            // Create a TcpClient.
            // Note, for this client to work you need to have a TcpServer 
            // connected to the same address as specified by the server, port
            // combination.
            Int32 port = 13000;
            TcpClient client = new TcpClient(server, port);

            // Translate the passed message into ASCII and store it as a Byte array.
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

            // Get a client stream for reading and writing.
            //  Stream stream = client.GetStream();

            NetworkStream stream = client.GetStream();

            // Send the message to the connected TcpServer. 
            await stream.WriteAsync(data, 0, data.Length);

            Debug.LogFormat("Sent: {0}", message);

            // Receive the TcpServer.response.

            // Buffer to store the response bytes.
            data = new Byte[256];

            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            Int32 bytes = await stream.ReadAsync(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Debug.LogFormat("Received 1: {0}", responseData);

            // Read the first batch of the TcpServer response bytes.
            bytes = await stream.ReadAsync(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Debug.LogFormat("Received 2: {0}", responseData);

            // Read the first batch of the TcpServer response bytes.
            bytes = await stream.ReadAsync(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Debug.LogFormat("Received 3: {0}", responseData);

            // Close everything.
            stream.Close();
            client.Close();
        } catch (ArgumentNullException e) {
            Debug.LogErrorFormat("ArgumentNullException: {0}", e);
        } catch (SocketException e) {
            Debug.LogErrorFormat("SocketException: {0}", e);
        } catch (System.Exception e) {
            Debug.LogException(e);
        }
    }
}
