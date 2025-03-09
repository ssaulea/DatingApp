using System;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace API.Data;

public class MessageRepository(DataContext context, IMapper mapper) : IMessagesRepository
{
    public async Task AddMessage(Message message)
    {
        await context.Messages.AddAsync(message);
    }

    public void DeleteMessage(Message message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Message?> GetMessage(int id)
    {
        return await context.Messages.FindAsync(id);
    }

    public Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
        var query = context.Messages
            .OrderByDescending(x => x.MessageSent)
            .AsQueryable();

        query = messageParams.Container switch 
        {
            "Inbox" => query.Where(x => x.Recipient.UserName == messageParams.Username && !x.RecipientDeleted),
            "Outbox" => query.Where(x => x.Sender.UserName == messageParams.Username && !x.SenderDeleted),
            _ => query.Where(x => x.Recipient.UserName == messageParams.Username && !x.DateRead.HasValue && !x.SenderDeleted && !x.RecipientDeleted)
        };

        var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

        return PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUserName)
    {
        var messages = await context.Messages
            .Include(m => m.Sender).ThenInclude(s => s.Photos)
            .Include(m => m.Recipient).ThenInclude(s => s.Photos)
            .Where(x => x.RecipientUsername == currentUsername && x.SenderUsername == recipientUserName && !x.RecipientDeleted
                || x.RecipientUsername == recipientUserName && x.SenderUsername == currentUsername && !x.SenderDeleted)
            .OrderBy(x => x.MessageSent)
            .ToListAsync();

        var unreadMessages = messages.Where(x => x.RecipientUsername == currentUsername && !x.DateRead.HasValue).ToList();

        if (unreadMessages.Count != 0)
        {
            unreadMessages.ForEach(x => x.DateRead = DateTime.UtcNow);
            await context.SaveChangesAsync();
        }

        return mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
