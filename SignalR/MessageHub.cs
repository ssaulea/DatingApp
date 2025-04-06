using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public class MessageHub(IMessagesRepository messagesRepository, IUserRepository userRepository, IMapper mapper, IHubContext<PresenceHub> presenceHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext?.Request.Query["user"];

        if (Context.User == null || string.IsNullOrEmpty(otherUser)) 
            throw new Exception("Cannot join group");
        var groupName = GetGroupName(Context.User.GetUserName(), otherUser!);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var group = await AddToGroup(groupName);
         
        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await messagesRepository.GetMessageThread(Context.User.GetUserName(), otherUser!);
        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }


    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var group = await RemovefromMessageGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
        await base.OnDisconnectedAsync(exception);
    }

    private async Task<Group> AddToGroup(string groupName)
    {
        var username = Context.User?.GetUserName() ?? throw new Exception("Cannot get username");
        var group = await messagesRepository.GetMessageGroup(groupName);
        var connection = new Connection { ConnectionId = Context.ConnectionId, Username = username };

        if (group == null)
        {
            group = new Group{ Name = groupName };
            messagesRepository.AddGroup(group);
        }

        group.Connections.Add(connection);

        if (await messagesRepository.SaveAllAsync()) return group;

        throw new HubException("Failed to join group");
    }

    private async Task<Group> RemovefromMessageGroup() 
    {
        var group = await messagesRepository.GetGroupForConnection(Context.ConnectionId);
        var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
        if (connection != null && group != null)
        {
            messagesRepository.RemoveConnection(connection);
            if (await messagesRepository.SaveAllAsync()) return group;
        }
        throw new Exception("Failed to remove from group");
    }


    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var username = Context.User?.GetUserName() ?? throw new Exception("could not get user");
        if (createMessageDto.RecipientUsername == username) throw new HubException("you cannot message your self");

        var sender = await userRepository.GetUserByUsernameAsync(username);
        var recipient = await userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (sender is null || recipient is null || sender.UserName is null || recipient.UserName is null) throw new HubException("Cannot send message at this time");

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        var groupName = GetGroupName(sender.UserName, recipient.UserName);
        var group = await messagesRepository.GetMessageGroup(groupName);

        if (group != null && group.Connections.Any(x => x.Username == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
            if (connections.Count != 0)
            {
                await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", 
                    new { username = sender.UserName, knownAs = sender.KnownAs, message = message.Content });
            }
        }

        await messagesRepository.AddMessage(message);

        if (await messagesRepository.SaveAllAsync())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
        }
    }

}
