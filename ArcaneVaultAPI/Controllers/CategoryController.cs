using ArcaneVaultAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    // GET: api/Category
    [HttpGet]
    public ActionResult<IEnumerable<Category>> GetCategory()
    {
        var categories = new List<Category>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "SELECT CategoryCode, CategoryName FROM Categories",
            connection);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            categories.Add(new Category
            {
                CategoryCode = reader["CategoryCode"].ToString(),
                CategoryName = reader["CategoryName"].ToString()
            });
        }

        // LINQ — order by CategoryName
        var result = categories.OrderBy(c => c.CategoryName).ToList();

        return result;
    }

    // GET: api/Category/5
    [HttpGet("{categorycode}")]
    public ActionResult<Category> GetCategory(string categorycode)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "SELECT CategoryCode, CategoryName FROM Categories WHERE CategoryCode = @CategoryCode",
            connection);
        command.Parameters.AddWithValue("@CategoryCode", categorycode);

        using var reader = command.ExecuteReader();

        Category? category = null;
        while (reader.Read())
        {
            category = new Category
            {
                CategoryCode = reader["CategoryCode"].ToString(),
                CategoryName = reader["CategoryName"].ToString()
            };
        }

        if (category == null)
        {
            return NotFound();
        }

        return category;
    }

    // PUT: api/Category/5
    [HttpPut("{categorycode}")]
    public IActionResult PutCategory(string? categorycode, Category category)
    {
        if (categorycode != category.CategoryCode)
        {
            return BadRequest();
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Check if exists
        var checkCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM Categories WHERE CategoryCode = @CategoryCode",
            connection);
        checkCommand.Parameters.AddWithValue("@CategoryCode", categorycode);
        var count = Convert.ToInt32(checkCommand.ExecuteScalar());

        if (count == 0)
        {
            return NotFound();
        }

        var command = new SqliteCommand(
            @"UPDATE Categories 
              SET CategoryName = @CategoryName 
              WHERE CategoryCode = @CategoryCode",
            connection);

        command.Parameters.AddWithValue("@CategoryCode", category.CategoryCode);
        command.Parameters.AddWithValue("@CategoryName", category.CategoryName);

        command.ExecuteNonQuery();

        return NoContent();
    }

    // POST: api/Category
    [HttpPost]
    public ActionResult<Category> PostCategory(Category category)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "INSERT INTO Categories (CategoryCode, CategoryName) VALUES (@CategoryCode, @CategoryName)",
            connection);

        command.Parameters.AddWithValue("@CategoryCode", category.CategoryCode);
        command.Parameters.AddWithValue("@CategoryName", category.CategoryName);

        command.ExecuteNonQuery();

        return CreatedAtAction("GetCategory", new { categorycode = category.CategoryCode }, category);
    }

    // DELETE: api/Category/5
    [HttpDelete("{categorycode}")]
    public IActionResult DeleteCategory(string? categorycode)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Check if exists
        var checkCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM Categories WHERE CategoryCode = @CategoryCode",
            connection);
        checkCommand.Parameters.AddWithValue("@CategoryCode", categorycode);
        var count = Convert.ToInt32(checkCommand.ExecuteScalar());

        if (count == 0)
        {
            return NotFound();
        }

        var command = new SqliteCommand(
            "DELETE FROM Categories WHERE CategoryCode = @CategoryCode",
            connection);
        command.Parameters.AddWithValue("@CategoryCode", categorycode);
        command.ExecuteNonQuery();

        return NoContent();
    }
}