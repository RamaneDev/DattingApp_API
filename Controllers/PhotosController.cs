using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DattingApp.API.Data;
using DattingApp.API.Dtos;
using DattingApp.API.Helpers;
using DattingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DattingApp.API.Controllers
{
    [Authorize]
    [Route("users/{userId}/[Controller]")]
    [ApiController]    
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinaryOptions> _options;
        private Cloudinary _cloudinary;
        
        public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinaryOptions> options)
        {
            _options = options;
            _mapper = mapper;
            _repo = repo;
            _cloudinary = new Cloudinary(
                    new Account(_options.Value.Cloud_name, _options.Value.Api_key, _options.Value.Api_secret )
             );
        }

        [HttpGet("{id}", Name="GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);
            
            var photo = _mapper.Map<PhotoToReturnDto>(photoFromRepo);
            
            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoForCreationDto photoForCreationDto)       
        { 
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
               return Unauthorized();
            
            var userFormRepo = await _repo.GetUser(userId);
            
           

            var file = photoForCreationDto.File;
            
            var uploadResult = new ImageUploadResult();
            
            if(file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                   var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation()
                            .Width(500).Height(500).Gravity("face").Crop("fill")
                    };
                   
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            var photoToRepo = _mapper.Map<Photo>(photoForCreationDto);

            if(!userFormRepo.Photos.Any(u => u.IsMain)) photoToRepo.IsMain = true;
            
            userFormRepo.Photos.Add(photoToRepo);

            if(await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoToReturnDto>(photoToRepo);
                return CreatedAtRoute("GetPhoto", new {userId=userId, id=photoToRepo.Id}, photoToReturn);
            } 

            return BadRequest("Coud not add the photo !");        

        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> setMain(int id, int userId)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
               return Unauthorized();
            
            var userFormRepo = await _repo.GetUser(userId);

            if(!userFormRepo.Photos.Any(p => p.Id == id))
               return Unauthorized();
            
            var photoFormRepo = await _repo.GetPhoto(id);

            if(photoFormRepo.IsMain)
              return BadRequest("This Photo is already Main Photo !");
            
            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);

            currentMainPhoto.IsMain = false;

            photoFormRepo.IsMain = true;

            if(await _repo.SaveAll())
               return NoContent();
            
            return BadRequest("Could not set photo to main");
        }

    }

       
}