﻿using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _messageRepository = messageRepository;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var username = User.GetUsername();

        if (username.ToLower() == createMessageDto.RecipientUsername.ToLower())
            return BadRequest("You cannot send messages to yourself");

        var sender = await _userRepository.GetUserByUsernameAsync(username)!;
        var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (recipient == null) return NotFound($"Failed to find user with name {createMessageDto.RecipientUsername}");

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender!.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };
        
        _messageRepository.AddMessage(message);

        if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));

        return BadRequest("Failed to send message");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();

        
        var messages = await _messageRepository.GetMessagesForUser(messageParams);
        
        Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize,
            messages.TotalCount, messages.TotalPages));

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"GetMessageForUsers: {messages.Count}");
        Console.ForegroundColor = ConsoleColor.White;

        return messages;
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        var currentUsername = User.GetUsername();

        return Ok(await _messageRepository.GetMessageThread(currentUsername, username));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();

        var message = await _messageRepository.GetMessage(id);

        if (username != message.SenderUsername && username != message.RecipientUsername)
        {
            return Unauthorized();
        }

        if (username == message.SenderUsername) message.SenderDeleted = true;
        if (username == message.RecipientUsername) message.RecipientDeleted = true;

        if (message.SenderDeleted && message.RecipientDeleted)
        {
            _messageRepository.DeleteMessage(message);
        }

        if (await _messageRepository.SaveAllAsync()) return Ok();

        return BadRequest("Problem deleting the message");
    }
}