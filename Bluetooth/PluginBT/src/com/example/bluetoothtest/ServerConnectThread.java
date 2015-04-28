package com.example.bluetoothtest;

import java.io.IOException;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothServerSocket;
import android.bluetooth.BluetoothSocket;
import android.os.Handler;
import android.util.Log;

/*
/**
 * On Connection accepted class
 * - Open a server-side bluetooth service
 * - (not implemented yet) Scan for available devices
 * - (not implemented yet) Pair a given device
 *
 */
class ServerConnectThread extends Thread 
{
    private final BluetoothServerSocket mmServerSocket;
    private BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
    private Handler mHandler = null;
    
    public ServerConnectThread(Handler handler) {
    	
    	Log.v("MainActivity", "Thread connexion started!");
    	
    	mHandler = handler;
        // Use a temporary object that is later assigned to mmServerSocket,
        // because mmServerSocket is final
        BluetoothServerSocket tmp = null;
        try {
            // MY_UUID is the app's UUID string, also used by the client code
            tmp = mBluetoothAdapter.listenUsingRfcommWithServiceRecord(BluetoothManager.NAME, BluetoothManager.MY_UUID);
            Log.v("MainActivity", "Server socket created");
        } catch (IOException e) { }
        mmServerSocket = tmp;
    }
 
    public void run() {
        BluetoothSocket socket = null;
        Log.v("MainActivity", "Thread connexion running");
        
        // Keep listening until exception occurs or a socket is returned
        while (true) {
        	//Log.v("MainActivity", "Tata");
            try {
            	Log.d("MainActivity", "AcceptThread calling accept()...");
                socket = mmServerSocket.accept();
                Log.d("MainActivity", "AcceptThread accept() returned ");
            	
            } catch (IOException e) {
            	Log.v("MainActivity", "AcceptThread accept() failed ");
            	e.printStackTrace();
                break;
            }

            // If a connection was accepted
            if (socket != null) {
            	Log.v("MainActivity", "AcceptThread accept() successfully");
                // Do work to manage the connection (in a separate thread)
            	final ConnectedThread okThread = new ConnectedThread(socket, mHandler);
            	okThread.start();
            	

            	
                try {
					mmServerSocket.close();
				} catch (IOException e) {
					e.printStackTrace();
				}
                break;
            }
        }
    }
 
    /** Will cancel the listening socket, and cause the thread to finish */
    public void cancel() {
        try {
            mmServerSocket.close();
        } catch (IOException e) { }
    }
}