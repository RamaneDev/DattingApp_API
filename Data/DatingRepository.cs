using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DattingApp.API.Helpers;
using DattingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DattingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        
        public DatingRepository(DataContext context)
        {
            _context = context;
        }

        public void Add<T>(T entity) where T : class
        {
            this._context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && 
                 u.LikeeId == recipientId);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return  await _context.Photos.Where(ph => ph.UserId == userId)
                            .FirstOrDefaultAsync(p => p.IsMain);         
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(ph => ph.Id == id);

            return photo;
        }

        public async Task<User> GetUser(int id)
        {
           var user =  await _context.Users.FirstOrDefaultAsync(u =>u.Id == id);

           return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.OrderByDescending(u => u.LastActive).AsQueryable();

            users = users.Where(u => u.Id != userParams.UserId);

            users = users.Where(u => u.Gender == userParams.Gender);

            if (userParams.Likers)
            {
              var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
              users = users.Where(u => userLikers.Contains(u.Id));
            }
            
            if (userParams.Likees)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.Id));
            }

            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            switch(userParams.OrderBy)
            {
                case "created" :
                    users = users.OrderByDescending(u => u.Created);
                    break;
                default :
                break;
            }

            var userPagedList = await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);

            return userPagedList;
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            
            if (likers)
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages.AsQueryable();
            
            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(u => u.SenderId == messageParams.UserId && u.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted == false && u.IsRead == false );
                    break;
            } 

            messages = messages.OrderByDescending(m => m.MessageSent);

            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
            
        }
             
        

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
               .Where(u => u.SenderId == userId && u.RecipientId == recipientId && u.SenderDeleted == false ||
                       u.SenderId == recipientId && u.RecipientId == userId && u.RecipientDeleted == false )
               .OrderByDescending(m => m.MessageSent).ToListAsync();
             
             return messages;
            
        }
    }
}