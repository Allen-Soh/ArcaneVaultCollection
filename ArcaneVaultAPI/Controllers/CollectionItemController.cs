//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using ArcaneVaultAPI.Data;
using ArcaneVaultAPI.DataTransferObject;

[Route("api/[controller]")]
[ApiController]
public class CollectionItemController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    // GET: api/CollectionItem
    [HttpGet]
    public ActionResult<IEnumerable<CollectionItem>> GetCollectionItem()
    {
        var items = new List<CollectionItem>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "SELECT ItemId, ItemName, IsDeleted, StartingQuantity, CurrentQuantity, UserName, ImageUrl, ImagePublicId FROM CollectionItems",
            connection);

        using var reader = command.ExecuteReader();

        // LINQ method syntax — project reader rows into CollectionItem objects
        while (reader.Read())
        {
            items.Add(new CollectionItem
            {
                ItemId = Convert.ToInt32(reader["ItemId"]),
                ItemName = reader["ItemName"].ToString(),
                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                StartingQuantity = Convert.ToInt32(reader["StartingQuantity"]),
                CurrentQuantity = Convert.ToInt32(reader["CurrentQuantity"]),
                UserName = reader["UserName"].ToString(),
                ImageUrl = reader["ImageUrl"] == DBNull.Value ? null : reader["ImageUrl"].ToString(),
                ImagePublicId = reader["ImagePublicId"] == DBNull.Value ? null : reader["ImagePublicId"].ToString()
            });
        }

        // LINQ — order by ItemName
        var result = items.OrderBy(c => c.ItemName).ToList();

        return result;
    }

    // GET: api/CollectionItem/withcategory
    [HttpGet("withcategory")]
    public ActionResult<IEnumerable<object>> GetCollectionItemWithCategory()
    {
        var items = new List<object>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT ci.ItemId, ci.ItemName, ci.IsDeleted, ci.StartingQuantity, 
                 ci.CurrentQuantity, ci.UserName, ci.ImageUrl, ci.ImagePublicId,
                 cic.CategoryCode, c.CategoryName
          FROM CollectionItems ci
          LEFT JOIN CollectionItemCategories cic ON ci.ItemId = cic.ItemId
          LEFT JOIN Categories c ON cic.CategoryCode = c.CategoryCode",
            connection);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new
            {
                ItemId = Convert.ToInt32(reader["ItemId"]),
                ItemName = reader["ItemName"].ToString(),
                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                StartingQuantity = Convert.ToInt32(reader["StartingQuantity"]),
                CurrentQuantity = Convert.ToInt32(reader["CurrentQuantity"]),
                UserName = reader["UserName"].ToString(),
                ImageUrl = reader["ImageUrl"] == DBNull.Value ? null : reader["ImageUrl"].ToString(),
                ImagePublicId = reader["ImagePublicId"] == DBNull.Value ? null : reader["ImagePublicId"].ToString(),
                CategoryCode = reader["CategoryCode"] == DBNull.Value ? null : reader["CategoryCode"].ToString(),
                CategoryName = reader["CategoryName"] == DBNull.Value ? null : reader["CategoryName"].ToString()
            });
        }

        return Ok(items);
    }

    // PUT: api/CollectionItem/5
    [HttpPut("{itemid}")]
    public IActionResult PutCollectionItem(int? itemid, CollectionItemDTO dto)
    {
        if (itemid != dto.ItemId) return BadRequest();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var checkCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM CollectionItems WHERE ItemId = @ItemId", connection);
        checkCommand.Parameters.AddWithValue("@ItemId", itemid);
        if (Convert.ToInt32(checkCommand.ExecuteScalar()) == 0) return NotFound();

        var command = new SqliteCommand(
            @"UPDATE CollectionItems
          SET ItemName        = @ItemName,
              IsDeleted       = @IsDeleted,
              StartingQuantity= @StartingQuantity,
              CurrentQuantity = @CurrentQuantity,
              UserName        = @UserName,
              ImageUrl        = @ImageUrl,
              ImagePublicId   = @ImagePublicId
          WHERE ItemId = @ItemId",
            connection);
        command.Parameters.AddWithValue("@ItemId", dto.ItemId);
        command.Parameters.AddWithValue("@ItemName", dto.ItemName);
        command.Parameters.AddWithValue("@IsDeleted", dto.IsDeleted);
        command.Parameters.AddWithValue("@StartingQuantity", dto.StartingQuantity);
        command.Parameters.AddWithValue("@CurrentQuantity", dto.CurrentQuantity);
        command.Parameters.AddWithValue("@UserName", dto.UserName);
        command.Parameters.AddWithValue("@ImageUrl", dto.ImageUrl ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ImagePublicId", dto.ImagePublicId ?? (object)DBNull.Value);
        command.ExecuteNonQuery();

        return NoContent();
    }

    // POST: api/CollectionItem
    [HttpPost]
    public ActionResult<CollectionItem> PostCollectionItem(CollectionItemDTO dto)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"INSERT INTO CollectionItems
          (ItemName, IsDeleted, StartingQuantity, CurrentQuantity, UserName, ImageUrl, ImagePublicId)
          VALUES
          (@ItemName, @IsDeleted, @StartingQuantity, @CurrentQuantity, @UserName, @ImageUrl, @ImagePublicId);
          SELECT last_insert_rowid();",
            connection);
        command.Parameters.AddWithValue("@ItemName", dto.ItemName);
        command.Parameters.AddWithValue("@IsDeleted", dto.IsDeleted);
        command.Parameters.AddWithValue("@StartingQuantity", dto.StartingQuantity);
        command.Parameters.AddWithValue("@CurrentQuantity", dto.CurrentQuantity);
        command.Parameters.AddWithValue("@UserName", dto.UserName);
        command.Parameters.AddWithValue("@ImageUrl", dto.ImageUrl ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ImagePublicId", dto.ImagePublicId ?? (object)DBNull.Value);

        var newId = Convert.ToInt32(command.ExecuteScalar());

        return CreatedAtAction("GetCollectionItem", new { itemid = newId }, new CollectionItem
        {
            ItemId = newId,
            ItemName = dto.ItemName,
            IsDeleted = dto.IsDeleted,
            StartingQuantity = dto.StartingQuantity,
            CurrentQuantity = dto.CurrentQuantity,
            UserName = dto.UserName,
            ImageUrl = dto.ImageUrl,
            ImagePublicId = dto.ImagePublicId
        });
    }

    // DELETE: api/CollectionItem/5
    [HttpDelete("{itemid}")]
    public IActionResult DeleteCollectionItem(int? itemid)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Check exists first
        var checkCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM CollectionItems WHERE ItemId = @ItemId",
            connection);
        checkCommand.Parameters.AddWithValue("@ItemId", itemid);
        var count = Convert.ToInt32(checkCommand.ExecuteScalar());

        if (count == 0)
        {
            return NotFound();
        }

        // Delete CollectionItemCategory link first — prevents orphaned records
        var deleteCategoryLink = new SqliteCommand(
            "DELETE FROM CollectionItemCategories WHERE ItemId = @ItemId",
            connection);
        deleteCategoryLink.Parameters.AddWithValue("@ItemId", itemid);
        deleteCategoryLink.ExecuteNonQuery();

        // Then delete CollectionItem
        var command = new SqliteCommand(
            "DELETE FROM CollectionItems WHERE ItemId = @ItemId",
            connection);
        command.Parameters.AddWithValue("@ItemId", itemid);
        command.ExecuteNonQuery();

        return NoContent();
    }
}