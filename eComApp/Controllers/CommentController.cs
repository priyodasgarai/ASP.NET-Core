using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CoreApiResponse;
using eComApp.Dtos.Comment;
using eComApp.Extensions;
using eComApp.Interfaces;
using eComApp.Mappers;
using eComApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace eComApp.Controllers
{
    [Route("api/comment")]
    [ApiController]
    public class CommentController : BaseController
    {
        private readonly ICommentRepository _commentRepo;
        private readonly ILogger<CommentController> _logger;
        private readonly IStockRepository _stockRepo;
        private readonly UserManager<AppUser> _userManager;
        public CommentController(ILogger<CommentController> logger,
         ICommentRepository commentRepo,
         UserManager<AppUser> userManager,
         IStockRepository stockRepo)
        {
            _commentRepo = commentRepo;
            _logger = logger;
            _stockRepo = stockRepo;
            _userManager = userManager;

        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var comments = await _commentRepo.GetAllAsync();
                var commentDto = comments.Select(s => s.ToCommentDto());
                return CustomResult("Data loaded successfully", commentDto, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CustomResult(ex.Message, HttpStatusCode.BadRequest);
            }
        }
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var comment = await _commentRepo.GetByIdAsync(id);
                if (comment == null)
                {
                    return CustomResult("Data not found", HttpStatusCode.NotFound);
                }
                return CustomResult("Data loaded successfully", comment.ToCommentDto(), HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CustomResult(ex.Message, HttpStatusCode.BadRequest);
            }

        }
        [HttpPost("{stockId:int}")]
        public async Task<IActionResult> Create([FromRoute] int stockId, CreateCommentDto commentDto)
        {

            try
            {
                if (!ModelState.IsValid)
                    return CustomResult("One or more validation errors occurred", ModelState, HttpStatusCode.BadRequest);
                if (!await _stockRepo.StockExists(stockId))
                {
                    return CustomResult("Stock does not exist ", HttpStatusCode.BadRequest);
                }
                var username = User.GetUsername();
                var appUser = await _userManager.FindByNameAsync(username);


                var commentModel = commentDto.ToCommentFromCreate(stockId);
                commentModel.AppUserId = appUser.Id;

                await _commentRepo.CreateAsync(commentModel);
                var result = CreatedAtAction(nameof(GetById), new { id = commentModel.Id }, commentModel.ToCommentDto());
                return CustomResult("Data added successfully", result.Value, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CustomResult(ex.Message, HttpStatusCode.BadRequest);
            }
        }
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCommentRequestDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return CustomResult("Validation errors occurre", ModelState, HttpStatusCode.BadRequest);
                var comment = await _commentRepo.UpdateAsync(id, updateDto.ToCommentFromUpdate());
                if (comment == null)
                {
                    return CustomResult("Comment not found ", HttpStatusCode.NotFound);
                }
                return CustomResult("Data update successfully", comment.ToCommentDto(), HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CustomResult(ex.Message, HttpStatusCode.BadRequest);
            }
        }
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var commentModel = await _commentRepo.DeleteAsync(id);
                if (commentModel == null)
                {
                    return CustomResult("Comment dose note exist", HttpStatusCode.NotFound);
                }
                return CustomResult("Data delete successfully", commentModel, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CustomResult(ex.Message, HttpStatusCode.BadRequest);
            }
        }

    }
}