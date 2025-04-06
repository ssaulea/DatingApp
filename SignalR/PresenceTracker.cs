using System;
using System.Collections.Immutable;

namespace API.SignalR;

public class PresenceTracker
{
    private static Dictionary<string, List<string>> OnlineUsers = [];

    public Task<bool> UserConnected(string username, string connectionId)
    {
        bool isOnline = false;
        lock (OnlineUsers) 
        {
            if (OnlineUsers.ContainsKey(username))
            {
                OnlineUsers[username].Add(connectionId);
            }
            else
            {
                OnlineUsers.Add(username, [connectionId]);
                isOnline = true;
            }
        }
        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnected(string username, string connectionId)
    {
        var isOffline = false;
        lock (OnlineUsers) 
        {
            if (OnlineUsers.TryGetValue(username, out List<string>? value))
            {
                value.Remove(connectionId);

                if (OnlineUsers[username].Count == 0)
                {
                    OnlineUsers.Remove(username);
                    isOffline = true;
                }
            }
        }
        return Task.FromResult(isOffline);
    }

    public Task<string[]> GetOnlineUsers()
    {
        string[] onlineUsers; 
        lock (OnlineUsers)
        {
            onlineUsers = [.. OnlineUsers.Keys.ToArray().OrderBy(k => k)];
        }
        return Task.FromResult(onlineUsers);
    }

    public static Task<List<string>> GetConnectionsForUser(string username)
    {
        List<string> connectionIds = [];

        if (OnlineUsers.TryGetValue(username, out var connections))
        {
            lock(connections)
            {
                connectionIds = [.. connections];
            }
        }
        
        return Task.FromResult(connectionIds);
    }
}

