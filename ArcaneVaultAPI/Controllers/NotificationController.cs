using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Security.Claims;

namespace ArcaneVaultAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=ArcaneVault.db";

        public class LikeSummary
        {
            public int ShowoffId { get; set; }
            public string ShowoffName { get; set; }
            public int LikeCount { get; set; }
        }

        public class CommentNotice
        {
            public int ShowoffId { get; set; }
            public string ShowoffName { get; set; }
            public string CommenterUserName { get; set; }
            public string Text { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class ModerationNotice
        {
            public int ShowoffId { get; set; }
            public string ShowoffName { get; set; }
            public string EventType { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class TicketReplyNotice
        {
            public string TicketUid { get; set; }
            public string Subject { get; set; }
            public DateTime RepliedAt { get; set; }
        }

        public class AppealDecisionNotice
        {
            public int ShowoffId { get; set; }
            public string ShowoffName { get; set; }
            public string Status { get; set; }
            public string? StaffNote { get; set; }
            public DateTime DecidedAt { get; set; }
        }

        public class NotificationPanelResult
        {
            public List<LikeSummary> Likes { get; set; } = new();
            public List<CommentNotice> Comments { get; set; } = new();
            public List<ModerationNotice> ModerationEvents { get; set; } = new();
            public List<TicketReplyNotice> TicketReplies { get; set; } = new();
            public List<AppealDecisionNotice> AppealDecisions { get; set; } = new();
            public bool IsStaff { get; set; }
            public int PendingTicketCount { get; set; }
            public int PendingReportCount { get; set; }
        }

        // GET: api/Notification/unread — cheap poll target, does not advance LastNotificationCheck
        [HttpGet("unread")]
        public ActionResult<object> GetUnread()
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName)) return Unauthorized();
            var isStaff = User.IsInRole("Staff");

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var since = GetLastCheck(connection, userName);

            var checkCommand = new SqliteCommand(
                @"SELECT
                    (SELECT COUNT(*) FROM ShowoffComments c
                     JOIN Showoffs s ON c.ShowoffId = s.ShowoffId
                     WHERE s.UserName = @UserName AND c.UserName != @UserName AND c.CreatedAt > @Since) +
                    (SELECT COUNT(*) FROM ShowoffModerationEvent m
                     JOIN Showoffs s ON m.ShowoffId = s.ShowoffId
                     WHERE s.UserName = @UserName AND m.CreatedAt > @Since) +
                    (SELECT COUNT(*) FROM SupportTickets
                     WHERE UserName = @UserName AND RepliedAt IS NOT NULL AND RepliedAt > @Since) +
                    (SELECT COUNT(*) FROM ShowoffAppeal
                     WHERE UserName = @UserName AND Status != 'Pending' AND DecidedAt > @Since) +
                    (CASE WHEN @IsStaff THEN
                        (SELECT COUNT(*) FROM SupportTickets
                         WHERE IsResolved = 0 AND AssignedStaffUserName = @UserName AND CreatedAt > @Since) +
                        (SELECT COUNT(*) FROM ShowoffReports
                         WHERE IsResolved = 0 AND AssignedStaffUserName = @UserName AND CreatedAt > @Since)
                     ELSE 0 END)",
                connection);
            checkCommand.Parameters.AddWithValue("@UserName", userName);
            checkCommand.Parameters.AddWithValue("@Since", since);
            checkCommand.Parameters.AddWithValue("@IsStaff", isStaff);

            var count = Convert.ToInt32(checkCommand.ExecuteScalar());
            return Ok(new { hasUnread = count > 0 });
        }

        // GET: api/Notification/panel — full data for the dropdown; advances LastNotificationCheck as its last step
        [HttpGet("panel")]
        public ActionResult<NotificationPanelResult> GetPanel()
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName)) return Unauthorized();
            var isStaff = User.IsInRole("Staff");

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var since = GetLastCheck(connection, userName);
            var result = new NotificationPanelResult { IsStaff = isStaff };

            // Likes — current per-showoff counts, not a delta
            var likesCommand = new SqliteCommand(
                @"SELECT s.ShowoffId, s.ShowoffName, COUNT(l.UserName) AS LikeCount
                  FROM Showoffs s
                  JOIN ShowoffLikes l ON l.ShowoffId = s.ShowoffId
                  WHERE s.UserName = @UserName AND s.IsDeleted = 0
                  GROUP BY s.ShowoffId, s.ShowoffName
                  HAVING COUNT(l.UserName) > 0
                  ORDER BY LikeCount DESC",
                connection);
            likesCommand.Parameters.AddWithValue("@UserName", userName);
            using (var reader = likesCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Likes.Add(new LikeSummary
                    {
                        ShowoffId = Convert.ToInt32(reader["ShowoffId"]),
                        ShowoffName = reader["ShowoffName"].ToString(),
                        LikeCount = Convert.ToInt32(reader["LikeCount"])
                    });
                }
            }

            // Comments — new since last check, excluding the owner's own comments on their own showoff
            var commentsCommand = new SqliteCommand(
                @"SELECT c.ShowoffId, s.ShowoffName, c.UserName AS CommenterUserName, c.Text, c.CreatedAt
                  FROM ShowoffComments c
                  JOIN Showoffs s ON c.ShowoffId = s.ShowoffId
                  WHERE s.UserName = @UserName AND c.UserName != @UserName AND c.CreatedAt > @Since
                  ORDER BY c.CreatedAt DESC",
                connection);
            commentsCommand.Parameters.AddWithValue("@UserName", userName);
            commentsCommand.Parameters.AddWithValue("@Since", since);
            using (var reader = commentsCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Comments.Add(new CommentNotice
                    {
                        ShowoffId = Convert.ToInt32(reader["ShowoffId"]),
                        ShowoffName = reader["ShowoffName"].ToString(),
                        CommenterUserName = reader["CommenterUserName"].ToString(),
                        Text = reader["Text"].ToString(),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                    });
                }
            }

            // Moderation events (disable/re-enable) on the user's own showoffs, new since last check
            var moderationCommand = new SqliteCommand(
                @"SELECT m.ShowoffId, m.ShowoffName, m.EventType, m.CreatedAt
                  FROM ShowoffModerationEvent m
                  JOIN Showoffs s ON m.ShowoffId = s.ShowoffId
                  WHERE s.UserName = @UserName AND m.CreatedAt > @Since
                  ORDER BY m.CreatedAt DESC",
                connection);
            moderationCommand.Parameters.AddWithValue("@UserName", userName);
            moderationCommand.Parameters.AddWithValue("@Since", since);
            using (var reader = moderationCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.ModerationEvents.Add(new ModerationNotice
                    {
                        ShowoffId = Convert.ToInt32(reader["ShowoffId"]),
                        ShowoffName = reader["ShowoffName"].ToString(),
                        EventType = reader["EventType"].ToString(),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                    });
                }
            }

            // Support ticket replies, new since last check
            var ticketCommand = new SqliteCommand(
                @"SELECT TicketUid, Subject, RepliedAt
                  FROM SupportTickets
                  WHERE UserName = @UserName AND RepliedAt IS NOT NULL AND RepliedAt > @Since
                  ORDER BY RepliedAt DESC",
                connection);
            ticketCommand.Parameters.AddWithValue("@UserName", userName);
            ticketCommand.Parameters.AddWithValue("@Since", since);
            using (var reader = ticketCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.TicketReplies.Add(new TicketReplyNotice
                    {
                        TicketUid = reader["TicketUid"].ToString(),
                        Subject = reader["Subject"].ToString(),
                        RepliedAt = Convert.ToDateTime(reader["RepliedAt"])
                    });
                }
            }

            // Appeal decisions (approved/denied), new since last check
            var appealCommand = new SqliteCommand(
                @"SELECT ShowoffId, ShowoffName, Status, StaffNote, DecidedAt
                  FROM ShowoffAppeal
                  WHERE UserName = @UserName AND Status != 'Pending' AND DecidedAt > @Since
                  ORDER BY DecidedAt DESC",
                connection);
            appealCommand.Parameters.AddWithValue("@UserName", userName);
            appealCommand.Parameters.AddWithValue("@Since", since);
            using (var reader = appealCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.AppealDecisions.Add(new AppealDecisionNotice
                    {
                        ShowoffId = Convert.ToInt32(reader["ShowoffId"]),
                        ShowoffName = reader["ShowoffName"].ToString(),
                        Status = reader["Status"].ToString(),
                        StaffNote = reader["StaffNote"] == DBNull.Value ? null : reader["StaffNote"].ToString(),
                        DecidedAt = Convert.ToDateTime(reader["DecidedAt"])
                    });
                }
            }

            if (isStaff)
            {
                var pendingTicketsCommand = new SqliteCommand(
                    "SELECT COUNT(*) FROM SupportTickets WHERE IsResolved = 0 AND AssignedStaffUserName = @UserName",
                    connection);
                pendingTicketsCommand.Parameters.AddWithValue("@UserName", userName);
                result.PendingTicketCount = Convert.ToInt32(pendingTicketsCommand.ExecuteScalar());

                var pendingReportsCommand = new SqliteCommand(
                    "SELECT COUNT(*) FROM ShowoffReports WHERE IsResolved = 0 AND AssignedStaffUserName = @UserName",
                    connection);
                pendingReportsCommand.Parameters.AddWithValue("@UserName", userName);
                result.PendingReportCount = Convert.ToInt32(pendingReportsCommand.ExecuteScalar());
            }

            // Opening the panel is what "clears" it — advance the checkpoint last, after all reads above
            var updateCommand = new SqliteCommand(
                "UPDATE ArcaneVaultUsers SET LastNotificationCheck = @Now WHERE UserName = @UserName",
                connection);
            updateCommand.Parameters.AddWithValue("@Now", DateTime.UtcNow);
            updateCommand.Parameters.AddWithValue("@UserName", userName);
            updateCommand.ExecuteNonQuery();

            return Ok(result);
        }

        private DateTime GetLastCheck(SqliteConnection connection, string userName)
        {
            var command = new SqliteCommand(
                "SELECT LastNotificationCheck FROM ArcaneVaultUsers WHERE UserName = @UserName",
                connection);
            command.Parameters.AddWithValue("@UserName", userName);
            var value = command.ExecuteScalar();
            if (value == null || value == DBNull.Value) return DateTime.MinValue;
            return Convert.ToDateTime(value);
        }
    }
}
