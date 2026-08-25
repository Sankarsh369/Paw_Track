using Microsoft.EntityFrameworkCore;
using PawTrack.Api.Data;
using PawTrack.Api.DTOs.Chatbot;
using PawTrack.Api.Models;
using System.Text;

namespace PawTrack.Api.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly PawTrackDbContext _context;

        public ChatbotService(PawTrackDbContext context)
        {
            _context = context;
        }

        public async Task<ChatbotConversationDto> SendMessageAsync(int? userId, SendChatMessageDto dto)
        {
            var conversation = await _context.ChatbotConversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.SessionId == dto.SessionId);

            if (conversation is null)
            {
                conversation = new ChatbotConversation { SessionId = dto.SessionId, UserId = userId };
                _context.ChatbotConversations.Add(conversation);
            }

            var userMessage = new ChatbotMessage
            {
                Conversation = conversation,
                Sender = ChatSender.User,
                MessageText = dto.MessageText
            };
            _context.ChatbotMessages.Add(userMessage);

            var (reply, handoff) = await BuildReplyAsync(dto.MessageText);
            if (handoff) conversation.HandedOffToStaff = true;

            var botMessage = new ChatbotMessage
            {
                Conversation = conversation,
                Sender = ChatSender.Bot,
                MessageText = reply
            };
            _context.ChatbotMessages.Add(botMessage);

            await _context.SaveChangesAsync();

            var full = await _context.ChatbotConversations
                .Include(c => c.Messages)
                .FirstAsync(c => c.Id == conversation.Id);

            return ToDto(full);
        }

        public async Task<ChatbotConversationDto?> GetBySessionIdAsync(string sessionId)
        {
            var conversation = await _context.ChatbotConversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            return conversation is null ? null : ToDto(conversation);
        }

        private async Task<(string reply, bool handoff)> BuildReplyAsync(string message)
        {
            var text = message.ToLowerInvariant();

            // 1. Staff / Handoff
            if (text.Contains("staff") || text.Contains("human") || text.Contains("person") || text.Contains("agent") || text.Contains("support") || text.Contains("contact"))
            {
                return ("Of course — I've flagged this conversation for a staff member. They will review our chat history and get back to you as soon as possible!", true);
            }

            // 2. Animal Searching (cats, dogs, rabbits, birds)
            string? searchSpecies = null;
            if (text.Contains("cat")) searchSpecies = "Cat";
            else if (text.Contains("dog")) searchSpecies = "Dog";
            else if (text.Contains("rabbit") || text.Contains("bun")) searchSpecies = "Rabbit";
            else if (text.Contains("bird")) searchSpecies = "Bird";

            if (searchSpecies != null)
            {
                var animals = await _context.Animals
                    .Include(a => a.Branch)
                    .Include(a => a.Category)
                    .Where(a => a.Status == AnimalStatus.Available && 
                                (a.Species.ToLower() == searchSpecies.ToLower() || (a.Category != null && a.Category.Name.ToLower() == searchSpecies.ToLower())))
                    .Take(5)
                    .ToListAsync();

                if (animals.Any())
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"Here are some available **{searchSpecies}s** ready for adoption at PawTrack:");
                    foreach (var a in animals)
                    {
                        sb.AppendLine($"- **{a.Name}** ({a.Breed}, {a.Age} yrs, {a.Gender}) at *{a.Branch?.Name}* — [View Profile](/Animals/Details/{a.Id})");
                    }
                    sb.AppendLine("\nWould you like to book a visit to meet one of them?");
                    return (sb.ToString(), false);
                }
                else
                {
                    return ($"We don't currently have any available **{searchSpecies}s** in our network. Check back soon or try searching for another type of animal!", false);
                }
            }

            // General animal query
            if (text.Contains("animal") || text.Contains("pet") || text.Contains("show me all") || text.Contains("look for"))
            {
                var animals = await _context.Animals
                    .Include(a => a.Branch)
                    .Where(a => a.Status == AnimalStatus.Available)
                    .Take(5)
                    .ToListAsync();

                if (animals.Any())
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Here are some available animals ready for adoption at PawTrack:");
                    foreach (var a in animals)
                    {
                        sb.AppendLine($"- **{a.Name}** ({a.Species} - {a.Breed}) at *{a.Branch?.Name}* — [View Profile](/Animals/Details/{a.Id})");
                    }
                    return (sb.ToString(), false);
                }
                else
                {
                    return ("There are currently no available animals ready for adoption. Please check back later!", false);
                }
            }

            // 3. Branches / Locations list
            if (text.Contains("branch") || text.Contains("location") || text.Contains("shelter") || text.Contains("where are you"))
            {
                var branches = await _context.Branches.ToListAsync();
                var sb = new StringBuilder();
                sb.AppendLine("We operate the following shelter branches:");
                foreach (var b in branches)
                {
                    sb.AppendLine($"- **{b.Name}** ({b.RegionCity}) — Address: *{b.Address}*, Phone: *{b.Phone ?? "N/A"}*");
                }
                sb.AppendLine("\nYou can ask me about available animals or visit slots at any of these locations!");
                return (sb.ToString(), false);
            }

            // 4. Visit Slots checking / booking support
            if (text.Contains("slot") || text.Contains("hour") || text.Contains("open") || text.Contains("visit") || text.Contains("schedule") || text.Contains("book") || text.Contains("time") || text.Contains("date"))
            {
                // Check if they mentioned a specific branch
                var branches = await _context.Branches.ToListAsync();
                var matchedBranch = branches.FirstOrDefault(b => text.Contains(b.Name.ToLower()) || text.Contains(b.RegionCity.ToLower().Split(',')[0]));

                if (matchedBranch != null)
                {
                    var slots = await _context.VisitSlots
                        .Where(s => s.BranchId == matchedBranch.Id && s.SlotDate >= DateTime.UtcNow.Date && s.BookedCount < s.Capacity)
                        .OrderBy(s => s.SlotDate).ThenBy(s => s.StartTime)
                        .Take(5)
                        .ToListAsync();

                    if (slots.Any())
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"Here are some upcoming open visit slots at **{matchedBranch.Name}**:");
                        foreach (var s in slots)
                        {
                            sb.AppendLine($"- **{s.SlotDate:yyyy-MM-dd}** from **{s.StartTime:hh\\:mm}** to **{s.EndTime:hh\\:mm}** ({s.Capacity - s.BookedCount} spots left)");
                        }
                        sb.AppendLine("\nTo book a visit, go to the profile of the animal you want to meet and select one of these slots!");
                        return (sb.ToString(), false);
                    }
                    else
                    {
                        return ($"There are currently no open visit slots scheduled for **{matchedBranch.Name}**. Please check back later or contact the branch directly at **{matchedBranch.Phone ?? "our main line"}**.", false);
                    }
                }
                else
                {
                    return ("You can book a free visit slot to meet any available animal. Just click **'View Profile'** on the animal's page, choose an open date and time from the calendar, and click 'Book Visit'.\n\nIf you want me to search open slots, ask me for slots and specify the branch (e.g. *PawTrack Central Shelter* or *PawTrack North Branch*).", false);
                }
            }

            // 5. Donations
            if (text.Contains("donat") || text.Contains("fund"))
            {
                return ("Thank you for wanting to help! You can donate to our general fund or to a " +
                        "specific animal's care from the [Donate](/Donate) page — every bit helps with food, " +
                        "vet care, and shelter.", false);
            }

            // 6. Fees
            if (text.Contains("fee") || text.Contains("cost") || text.Contains("price"))
            {
                return ("Adoption fees vary by branch and animal and typically cover vaccinations and " +
                        "a health check. You'll see the exact fee before you confirm an adoption.", false);
            }

            // 7. General Adopt query
            if (text.Contains("adopt"))
            {
                return ("To adopt, browse available animals, book a visit at your nearest branch, " +
                        "then submit an adoption application from the animal's page. Our staff review " +
                        "every application before it's approved.", false);
            }

            // 8. Fallback: Handoff to staff if we can't answer (helpful support in any situation)
            return ("I'm not sure I have a direct answer for that, so I have flagged this conversation for our shelter staff. A team member will review our chat and get back to you! Is there anything else I can help with?", true);
        }

        private static ChatbotConversationDto ToDto(ChatbotConversation c) => new()
        {
            Id = c.Id,
            SessionId = c.SessionId,
            HandedOffToStaff = c.HandedOffToStaff,
            StartedAt = c.StartedAt,
            Messages = (c.Messages ?? new List<ChatbotMessage>())
                .OrderBy(m => m.Timestamp)
                .Select(m => new ChatbotMessageDto
                {
                    Id = m.Id,
                    Sender = m.Sender.ToString(),
                    MessageText = m.MessageText,
                    Timestamp = m.Timestamp
                }).ToList()
        };
    }
}
