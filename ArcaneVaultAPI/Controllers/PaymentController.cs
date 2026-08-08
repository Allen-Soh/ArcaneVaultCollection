using ArcaneVaultAPI.Data;
using ArcaneVaultAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace ArcaneVaultAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=ArcaneVault.db";
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        // Dev-only base URL for the Razor Pages front end — matches the CORS
        // origin already hardcoded for it in Program.cs.
        private const string RpBaseUrl = "https://localhost:7051";

        public PaymentController(IConfiguration config, IEmailService emailService)
        {
            _config = config;
            _emailService = emailService;
        }

        // GET: api/Payment
        [Authorize(Roles = "Staff")]
        [HttpGet]
        public ActionResult<IEnumerable<Payment>> GetPayments()
        {
            var payments = new List<Payment>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = new SqliteCommand(
                @"SELECT PaymentId, UserName, CosmeticId, CosmeticName, CosmeticType, Amount, Currency, StripeSessionId, Status, CreatedAt
                  FROM Payment ORDER BY PaymentId DESC",
                connection);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                payments.Add(new Payment
                {
                    PaymentId = Convert.ToInt32(reader["PaymentId"]),
                    UserName = reader["UserName"].ToString(),
                    CosmeticId = reader["CosmeticId"].ToString(),
                    CosmeticName = reader["CosmeticName"].ToString(),
                    CosmeticType = reader["CosmeticType"].ToString(),
                    Amount = Convert.ToDecimal(reader["Amount"]),
                    Currency = reader["Currency"].ToString(),
                    StripeSessionId = reader["StripeSessionId"].ToString(),
                    Status = reader["Status"].ToString(),
                    CreatedAt = reader["CreatedAt"].ToString()
                });
            }

            return payments;
        }

        public class CheckoutRequest
        {
            public string CosmeticType { get; set; } // "Frame" or "Sticker"
            public string CosmeticCode { get; set; }
        }

        // POST: api/Payment/checkout
        [Authorize]
        [HttpPost("checkout")]
        public async Task<IActionResult> CreateCheckout(CheckoutRequest request)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string cosmeticName;
            decimal price;
            bool isLimited;
            int? limitedQuantity;

            if (request.CosmeticType == "Frame")
            {
                var cmd = new SqliteCommand(
                    "SELECT FrameName, Price, IsLimited, LimitedQuantity, IsDelisted FROM Frames WHERE FrameCode = @Code",
                    connection);
                cmd.Parameters.AddWithValue("@Code", request.CosmeticCode);
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return NotFound("Frame not found.");
                if (Convert.ToBoolean(Convert.ToInt32(reader["IsDelisted"]))) return BadRequest("This item is no longer available.");
                cosmeticName = reader["FrameName"].ToString();
                price = Convert.ToDecimal(reader["Price"]);
                isLimited = Convert.ToBoolean(Convert.ToInt32(reader["IsLimited"]));
                limitedQuantity = reader["LimitedQuantity"] == DBNull.Value ? null : Convert.ToInt32(reader["LimitedQuantity"]);
            }
            else if (request.CosmeticType == "Sticker")
            {
                var cmd = new SqliteCommand(
                    "SELECT StickerName, Price, IsLimited, LimitedQuantity, IsDelisted FROM Stickers WHERE StickerCode = @Code",
                    connection);
                cmd.Parameters.AddWithValue("@Code", request.CosmeticCode);
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return NotFound("Sticker not found.");
                if (Convert.ToBoolean(Convert.ToInt32(reader["IsDelisted"]))) return BadRequest("This item is no longer available.");
                cosmeticName = reader["StickerName"].ToString();
                price = Convert.ToDecimal(reader["Price"]);
                isLimited = Convert.ToBoolean(Convert.ToInt32(reader["IsLimited"]));
                limitedQuantity = reader["LimitedQuantity"] == DBNull.Value ? null : Convert.ToInt32(reader["LimitedQuantity"]);
            }
            else
            {
                return BadRequest("Unknown cosmetic type.");
            }

            if (isLimited && limitedQuantity.HasValue)
            {
                var countCmd = new SqliteCommand(
                    "SELECT COUNT(*) FROM CosmeticInventory WHERE CosmeticId = @Code AND CosmeticType = @Type",
                    connection);
                countCmd.Parameters.AddWithValue("@Code", request.CosmeticCode);
                countCmd.Parameters.AddWithValue("@Type", request.CosmeticType);
                var owned = Convert.ToInt32(countCmd.ExecuteScalar());
                if (owned >= limitedQuantity.Value)
                    return BadRequest("Sold out.");
            }

            var sessionOptions = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "sgd",
                            UnitAmount = (long)Math.Round(price * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = cosmeticName
                            }
                        }
                    }
                },
                SuccessUrl = $"{RpBaseUrl}/PaymentPages/Success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{RpBaseUrl}/PaymentPages/Failed",
                Metadata = new Dictionary<string, string>
                {
                    { "UserName", userName },
                    { "CosmeticType", request.CosmeticType },
                    { "CosmeticId", request.CosmeticCode }
                }
            };

            var sessionService = new SessionService();
            var session = await sessionService.CreateAsync(sessionOptions);

            var insertCmd = new SqliteCommand(
                @"INSERT INTO Payment (UserName, CosmeticId, CosmeticName, CosmeticType, Amount, Currency, StripeSessionId, Status, CreatedAt)
                  VALUES (@UserName, @CosmeticId, @CosmeticName, @CosmeticType, @Amount, @Currency, @StripeSessionId, @Status, @CreatedAt)",
                connection);
            insertCmd.Parameters.AddWithValue("@UserName", userName);
            insertCmd.Parameters.AddWithValue("@CosmeticId", request.CosmeticCode);
            insertCmd.Parameters.AddWithValue("@CosmeticName", cosmeticName);
            insertCmd.Parameters.AddWithValue("@CosmeticType", request.CosmeticType);
            insertCmd.Parameters.AddWithValue("@Amount", price);
            insertCmd.Parameters.AddWithValue("@Currency", "SGD");
            insertCmd.Parameters.AddWithValue("@StripeSessionId", session.Id);
            insertCmd.Parameters.AddWithValue("@Status", "Pending");
            insertCmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("o"));
            insertCmd.ExecuteNonQuery();

            return Ok(new { url = session.Url });
        }

        // POST: api/Payment/webhook
        // Stripe calls this directly (no JWT) — signature verification is what authenticates it.
        // This is the ONLY place a Payment is ever marked Success; the redirect pages never do.
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            if (!Request.Headers.TryGetValue("Stripe-Signature", out var signatureHeader))
                return BadRequest();

            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var webhookSecret = _config["Stripe:WebhookSecret"];

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, webhookSecret);
            }
            catch (StripeException)
            {
                return BadRequest();
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                if (stripeEvent.Data.Object is Session session)
                {
                    await MarkPaymentSuccessAsync(session.Id);
                }
            }

            return Ok();
        }

        private async Task MarkPaymentSuccessAsync(string stripeSessionId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var selectCmd = new SqliteCommand(
                @"SELECT PaymentId, UserName, CosmeticId, CosmeticName, CosmeticType, Amount, Currency, Status
                  FROM Payment WHERE StripeSessionId = @SessionId",
                connection);
            selectCmd.Parameters.AddWithValue("@SessionId", stripeSessionId);

            int paymentId;
            string userName, cosmeticId, cosmeticName, cosmeticType, currency, status;
            decimal amount;

            using (var reader = selectCmd.ExecuteReader())
            {
                if (!reader.Read()) return; // Unknown session — nothing to do
                paymentId = Convert.ToInt32(reader["PaymentId"]);
                userName = reader["UserName"].ToString();
                cosmeticId = reader["CosmeticId"].ToString();
                cosmeticName = reader["CosmeticName"].ToString();
                cosmeticType = reader["CosmeticType"].ToString();
                amount = Convert.ToDecimal(reader["Amount"]);
                currency = reader["Currency"].ToString();
                status = reader["Status"].ToString();
            }

            if (status == "Success") return; // Already processed — Stripe can retry webhook delivery

            var updateCmd = new SqliteCommand(
                "UPDATE Payment SET Status = 'Success' WHERE PaymentId = @PaymentId",
                connection);
            updateCmd.Parameters.AddWithValue("@PaymentId", paymentId);
            updateCmd.ExecuteNonQuery();

            var inventoryCmd = new SqliteCommand(
                @"INSERT INTO CosmeticInventory (Uid, UserName, CosmeticId, CosmeticType, AcquiredAt, PaymentId)
                  VALUES (@Uid, @UserName, @CosmeticId, @CosmeticType, @AcquiredAt, @PaymentId)",
                connection);
            inventoryCmd.Parameters.AddWithValue("@Uid", Guid.NewGuid().ToString());
            inventoryCmd.Parameters.AddWithValue("@UserName", userName);
            inventoryCmd.Parameters.AddWithValue("@CosmeticId", cosmeticId);
            inventoryCmd.Parameters.AddWithValue("@CosmeticType", cosmeticType);
            inventoryCmd.Parameters.AddWithValue("@AcquiredAt", DateTime.UtcNow.ToString("o"));
            inventoryCmd.Parameters.AddWithValue("@PaymentId", paymentId);
            inventoryCmd.ExecuteNonQuery();

            var emailCmd = new SqliteCommand(
                "SELECT Email FROM ArcaneVaultUsers WHERE UserName = @UserName",
                connection);
            emailCmd.Parameters.AddWithValue("@UserName", userName);
            var email = emailCmd.ExecuteScalar() as string;

            if (!string.IsNullOrEmpty(email))
            {
                await _emailService.SendPaymentReceiptAsync(email, userName, cosmeticName, amount, currency);
            }
        }

        // GET: api/Payment/by-session/{sessionId}
        // Used by the Success page to display real DB state rather than trusting the query string.
        [Authorize]
        [HttpGet("by-session/{sessionId}")]
        public IActionResult GetBySession(string sessionId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = new SqliteCommand(
                "SELECT CosmeticName, Amount, Currency, Status FROM Payment WHERE StripeSessionId = @SessionId",
                connection);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return NotFound();

            return Ok(new
            {
                cosmeticName = reader["CosmeticName"].ToString(),
                amount = Convert.ToDecimal(reader["Amount"]),
                currency = reader["Currency"].ToString(),
                status = reader["Status"].ToString()
            });
        }
    }
}
