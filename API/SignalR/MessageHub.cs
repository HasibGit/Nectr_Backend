using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessageHub(
        IUnitOfWork unitOfWork,
        IHubContext<PresenceHub> presenceHub,
        IMapper mapper
    ) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext?.Request.Query["user"].ToString();

        if (Context.User is null || string.IsNullOrEmpty(otherUser))
        {
            throw new HubException("Invalid user");
        }

        var groupName = GetGroupName(Context.User.GetUserName(), otherUser);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await AddToGroup(groupName);

        var messages = await unitOfWork.MessageRepository.GetMessageThread(Context.User.GetUserName(), otherUser);

        if (unitOfWork.HasChanges())
        {
            await unitOfWork.Complete();
        }

        await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await RemoveFromMessageGroup();

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var username = Context.User?.GetUserName() ?? throw new HubException("User not found");

        if (username == createMessageDto.RecipientUsername.ToLower())
        {
            throw new HubException("You cannot message yourself");
        }

        var sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
        var recipient = await unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (recipient == null || sender == null || sender.UserName == null || recipient.UserName == null)
        {
            throw new HubException("Message sending failed");
        }

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        var groupName = GetGroupName(sender.UserName, recipient.UserName);
        var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);

        if (group is not null && group.Connections.Any(x => x.UserName == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);

            if (connections is not null && connections.Count > 0)
            {
                await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                            new
                            {
                                username = sender.UserName,
                                knownAs = sender.KnownAs
                            }
                );
            }
        }

        unitOfWork.MessageRepository.AddMessage(message);

        if (await unitOfWork.Complete())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
        }
        else
        {
            throw new HubException("Message sending failed");
        }
    }

    private async Task<bool> AddToGroup(string groupName)
    {
        var userName = Context.User?.GetUserName() ?? throw new HubException("cannot get username");
        var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);
        var connection = new Connection { ConnectionId = Context.ConnectionId, UserName = userName };

        if (group is null)
        {
            group = new Group { Name = groupName };
            unitOfWork.MessageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);

        return await unitOfWork.Complete();
    }

    private async Task RemoveFromMessageGroup()
    {
        var connection = await unitOfWork.MessageRepository.GetConnection(Context.ConnectionId);

        if (connection is not null)
        {
            unitOfWork.MessageRepository.RemoveConnection(connection);
            await unitOfWork.Complete();
        }
    }

    private string GetGroupName(string sender, string receiver)
    {
        var stringCompare = string.CompareOrdinal(sender, receiver) < 0;

        return stringCompare ? $"{sender}-{receiver}" : $"{receiver}-{sender}";
    }
}
