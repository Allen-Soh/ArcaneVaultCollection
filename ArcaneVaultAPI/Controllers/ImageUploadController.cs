using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class ImageUploadController : ControllerBase
{
    private readonly Cloudinary _cloudinary;

    public ImageUploadController(Cloudinary cloudinary)
    {
        _cloudinary = cloudinary;
    }

    // POST: api/ImageUpload
    [HttpPost]
    public async Task<ActionResult> UploadImage(IFormFile file)
    {
        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/png" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest("Only JPG and PNG files are allowed.");

        // Validate file size — 5MB max
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("File size must not exceed 5MB.");

        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "arcanevault/showoffs",
            Transformation = new Transformation()
                .Quality("auto")
                .FetchFormat("auto")
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
            return BadRequest(uploadResult.Error.Message);

        return Ok(new
        {
            ImagePath = uploadResult.SecureUrl.ToString(),
            ImagePublicId = uploadResult.PublicId
        });
    }

    // DELETE: api/ImageUpload
    [HttpDelete]
    public async Task<ActionResult> DeleteImage([FromBody] string publicId)
    {
        if (string.IsNullOrEmpty(publicId))
            return BadRequest("Public ID is required.");

        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);

        if (result.Error != null)
            return BadRequest(result.Error.Message);

        return NoContent();
    }
}